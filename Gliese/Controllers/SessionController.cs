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
            Data = new {
                session = session.Pk,
                options = options,
            }
        };
    }

    [HttpPost]
    [Route("/session/makeAssertion")]
    public async Task<CommonResult<AccountMakeAssertion>> MakeAssertion([FromBody] MakeAssertionFormBody clientResponse, CancellationToken cancellationToken)
    {

        // var sessionPk = Request.Cookies["assertionOptions"];
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
        var sessionModel = dataContext.Sessions.FirstOrDefault(s => s.Pk == clientResponse.session);
        if (sessionModel == null)
        {
            return new CommonResult<AccountMakeAssertion>
            {
                Code = 200,
                Message = "Session is empty2"
            };
        }
        //var jsonOptions = HttpContext.Session.GetString("fido2.assertionOptions");
        var options = AssertionOptions.FromJson(sessionModel.Content);

        var creds = _fido2Storage.GetCredentialById(clientResponse.credential.Id) ?? throw new Exception("Unknown credentials");

        var storedCounter = creds.SignatureCounter;

        IsUserHandleOwnerOfCredentialIdAsync callback = async (args, cancellationToken) =>
        {
            var storedCreds = await _fido2Storage.GetCredentialsByUserHandleAsync(args.UserHandle, cancellationToken);
            return storedCreds.Exists(c => c.Descriptor.Id.SequenceEqual(args.CredentialId));
        };

        var res = await _fido2.MakeAssertionAsync(clientResponse.credential, options, creds.PublicKey, storedCounter, callback, cancellationToken: cancellationToken);

        _fido2Storage.UpdateCounter(res.CredentialId, res.Counter);

        var token = JwtHelper.GenerateToken("fakeUserId");
        HttpContext.Response.Cookies.Append("Authorization", $"Bearer {token}", new CookieOptions
        {
            Expires = DateTimeOffset.Now.AddMinutes(30),
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Secure = true
        });

        return new CommonResult<AccountMakeAssertion>
        {
            Code = 200,
            Data = new AccountMakeAssertion
            {
                Authorization = $"Bearer {token}"
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

    [Route("/session/introspection")]
    public CommonResult<object> Introspection(string token = "")
    {
        if (string.IsNullOrEmpty(token))
        {
            return new CommonResult<object> { Code = 401, Message = "无效token" };
        }
        var claims = JwtHelper.ValidateToken(token);
        var abc = claims?.Claims.FirstOrDefault();
        return new CommonResult<object>
        {
            Code = 200,
            Data = new
            {
                Username = abc?.Value,
            }
        };
    }
}

public class MakeAssertionFormBody {
    public string session { get; set; } = "";
    public AuthenticatorAssertionRawResponse? credential { get; set; }
}