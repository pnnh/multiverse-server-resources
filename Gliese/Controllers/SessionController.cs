using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Fido2NetLib;
using Fido2NetLib.Development;
using Fido2NetLib.Objects;
using Gliese.Models;
using Gliese.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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

    private string FormatException(Exception e)
    {
        return string.Format("{0}{1}", e.Message, e.InnerException != null ? " (" + e.InnerException.Message + ")" : "");
    }

    [HttpPost]
    [Route("/session/assertionOptions")]
    public ActionResult AssertionOptionsPost([FromForm] string username, [FromForm] string userVerification)
    {
        try
        {
            var existingCredentials = new List<PublicKeyCredentialDescriptor>();

            if (!string.IsNullOrEmpty(username))
            {
                // 1. Get user from DB
                var user = _fido2Storage.GetUser(username) ?? throw new ArgumentException("Username was not registered");

                // 2. Get registered credentials from database
                existingCredentials = _fido2Storage.GetCredentialsByUser(user).Select(c => c.Descriptor).ToList();
            }

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
            HttpContext.Session.SetString("fido2.assertionOptions", options.ToJson());

            // 5. Return options to client
            return Json(options);
        }

        catch (Exception e)
        {
            return Json(new AssertionOptions { Status = "error", ErrorMessage = FormatException(e) });
        }
    }

    [HttpPost]
    [Route("/session/makeAssertion")]
    public async Task<CommonResult<AccountMakeAssertion>> MakeAssertion([FromBody] AuthenticatorAssertionRawResponse clientResponse, CancellationToken cancellationToken)
    {
        var jsonOptions = HttpContext.Session.GetString("fido2.assertionOptions");
        var options = AssertionOptions.FromJson(jsonOptions);

        var creds = _fido2Storage.GetCredentialById(clientResponse.Id) ?? throw new Exception("Unknown credentials");

        var storedCounter = creds.SignatureCounter;

        IsUserHandleOwnerOfCredentialIdAsync callback = async (args, cancellationToken) =>
        {
            var storedCreds = await _fido2Storage.GetCredentialsByUserHandleAsync(args.UserHandle, cancellationToken);
            return storedCreds.Exists(c => c.Descriptor.Id.SequenceEqual(args.CredentialId));
        };

        var res = await _fido2.MakeAssertionAsync(clientResponse, options, creds.PublicKey, storedCounter, callback, cancellationToken: cancellationToken);

        _fido2Storage.UpdateCounter(res.CredentialId, res.Counter);

        var token = JwtHelper.GenerateToken("fakeUserId");

        return new CommonResult<AccountMakeAssertion>
        {
            Code = 200,
            Data = new AccountMakeAssertion
            {
                Token = token
            }
        };
    }

    [HttpPost]
    [Route("/session/validate")]
    public CommonResult<AccountValidate> Validate(string? token = "")
    {
        if (string.IsNullOrEmpty(token))
        {
            return new CommonResult<AccountValidate>
            {
                Code = 400,
                Message = "Token is empty"
            };
        }

        var result = JwtHelper.ValidateToken(token);

        if (result == null)
        {
            return new CommonResult<AccountValidate>
            {
                Code = 400,
                Message = "Token is invalid"
            };
        }
        var claim = result.Claims.FirstOrDefault(c => c.Type == "userId");
        var name = claim?.Subject?.Name;
        if (name == null)
        {
            return new CommonResult<AccountValidate>
            {
                Code = 400,
                Message = "Token is invalid"
            };
        }

        return new CommonResult<AccountValidate>
        {
            Code = 200,
            Data = new AccountValidate
            {
                Name = name
            }
        };
    }

    [Route("/session/logined")]
    public CommonResult<object> Logined()
    {
        var openid = Request.Cookies["openid"];
        logger.LogDebug($"openid {openid}");
        if (string.IsNullOrEmpty(openid))
        {
            return new CommonResult<object> { Code = 401, Message = "未登录" };
        }
        var dbUser = dataContext.Users.FirstOrDefault(m => m.Username == openid);
        if (dbUser == null)
        {
            return new CommonResult<object> { Code = 401, Message = "未登录" };
        }
        var parameters = new Dictionary<string, string>();
        parameters.Add("nickname", dbUser.Nickname ?? "未知用户");

        return new CommonResult<object> { Code = 200, Data = parameters };
    }
}