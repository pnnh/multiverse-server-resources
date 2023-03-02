using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fido2NetLib;
using Fido2NetLib.Development;
using Fido2NetLib.Objects;
using Gliese.Models;
using Gliese.Utils;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using static Fido2NetLib.Fido2;

namespace Gliese.Controllers;
public class SessionController : Controller
{
    private readonly ILogger<OAuth2Controller> logger;
    private readonly BloggingContext dataContext;
    private readonly Fido2Storage _fido2Storage;
    private IFido2 _fido2;
    public static IMetadataService? _mds;
    //public static readonly DevelopmentInMemoryStore DemoStorage = new DevelopmentInMemoryStore();

    public SessionController(ILogger<OAuth2Controller> logger, IFido2 fido2, BloggingContext configuration)
    {
        this.logger = logger;
        _fido2 = fido2;
        this.dataContext = configuration;
        _fido2Storage = new Fido2Storage(configuration);
    }

    [HttpPost]
    [Route("/session/assertionOptions")]
    public CommonResult<object> AssertionOptionsPost([FromForm] string username, [FromForm] string userVerification)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(username.Trim()))
        {
            //return Json(new CredentialMakeResult(status: "error", errorMessage: "Username is required", result: null));
            return new CommonResult<object>
            {
                Code = 400,
                Message = "Username is required",
                Data = null
            };
        }
        var existingCredentials = new List<PublicKeyCredentialDescriptor>();


        Fido2User? user = null;

        // 1. Get user from DB
        user = _fido2Storage.GetUser(username);
        if (user == null)
        {
            return new CommonResult<object>
            {
                Code = 400,
                Message = "Username is not registered",
                Data = null
            };
        }

        // 2. Get registered credentials from database
        existingCredentials = _fido2Storage.GetCredentialsByUser(user).Select(c => c.Descriptor).ToList();


        var exts = new AuthenticationExtensionsClientInputs()
        {
            UserVerificationMethod = true
        };

        // 3. Create options
        var uv = string.IsNullOrEmpty(userVerification) ? UserVerificationRequirement.Discouraged : userVerification.ToEnum<UserVerificationRequirement>();
        var options = _fido2.GetAssertionOptions(
            existingCredentials,
            uv,
            exts
        );

        // 4. Temporarily store options, session/in-memory cache/redis/db
        //HttpContext.Session.SetString("fido2.assertionOptions", options.ToJson());
        var session = new SessionTable
        {
            Pk = Guid.NewGuid().ToString(),
            Content = options.ToJson(),
            //User = System.Text.Encoding.UTF8.GetString(user.Id),
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow,
            Type = "fido2.assertionOptions",
        };
        dataContext.Sessions.Add(session);
        dataContext.SaveChanges();

        // 5. Return options to client
        //return Json(options); 
        return new CommonResult<object>
        {
            Code = 200,
            Data = new
            {
                session = session.Pk,
                options = options,
            }
        };
    }

    [HttpPost]
    [Route("/session/makeAssertion")]
    public async Task<CommonResult<AccountMakeAssertion>> MakeAssertion([FromBody] MakeAssertionFormBody clientResponse, CancellationToken cancellationToken)
    {
        logger.LogDebug($"sessionPk {clientResponse}");
        if (clientResponse == null || clientResponse.credential == null)
        {
            return new CommonResult<AccountMakeAssertion>
            {
                Code = 200,
                Message = "credential is empty"
            };
        }
        if (string.IsNullOrEmpty(clientResponse.session))
        {
            return new CommonResult<AccountMakeAssertion>
            {
                Code = 200,
                Message = "Session is empty"
            };
        }
        //var sessionModel = dataContext.Sessions.FirstOrDefault(s => s.Pk == clientResponse.session);
        var sessionResult = dataContext.Sessions.Join(dataContext.Accounts, s => s.User, u => u.Pk, (s, u) => new
        {
            Session = s,
            User = u
        }).FirstOrDefault();
        if (sessionResult == null || sessionResult.Session == null || sessionResult.User == null)
        {
            return new CommonResult<AccountMakeAssertion>
            {
                Code = 200,
                Message = "Session is empty2"
            };
        } 
        var userModel = sessionResult.User;
        var options = AssertionOptions.FromJson(sessionResult.Session.Content);

        var creds = _fido2Storage.GetCredentialById(clientResponse.credential.Id) ?? throw new Exception("Unknown credentials");

        var storedCounter = creds.SignatureCounter;

        IsUserHandleOwnerOfCredentialIdAsync callback = async (args, cancellationToken) =>
        {
            var storedCreds = await _fido2Storage.GetCredentialsByUserHandleAsync(args.UserHandle, cancellationToken);
            return storedCreds.Exists(c => c.Descriptor.Id.SequenceEqual(args.CredentialId));
        };

        var res = await _fido2.MakeAssertionAsync(clientResponse.credential, options, creds.PublicKey, storedCounter, callback, cancellationToken: cancellationToken);

        _fido2Storage.UpdateCounter(res.CredentialId, res.Counter);

        var token = JwtHelper.GenerateToken(userModel.Account);

        return new CommonResult<AccountMakeAssertion>
        {
            Code = 200,
            Data = new AccountMakeAssertion
            {
                Authorization = token
            },
            Message = "登录成功"
        };
    }

}

public class MakeAssertionFormBody
{
    public string session { get; set; } = "";
    public AuthenticatorAssertionRawResponse? credential { get; set; }
}