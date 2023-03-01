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
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using static Fido2NetLib.Fido2;

namespace Gliese.Controllers;


[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class AccountController : Controller
{
    private readonly ILogger<OAuth2Controller> logger;
    private readonly BloggingContext dataContext;
    private readonly Fido2Storage _fido2Storage;
    private IFido2 _fido2;
    public static IMetadataService? _mds;
    // private readonly JwtBearerOptions _jwtBearerOptions;
    // private readonly SigningCredentials _signingCredentials;
    public AccountController(ILogger<OAuth2Controller> logger, IFido2 fido2, BloggingContext configuration//,
                                                                                                          // IOptionsSnapshot<JwtBearerOptions> jwtBearerOptions,
                                                                                                          // SigningCredentials signingCredentials
        )
    {
        this.logger = logger;
        _fido2 = fido2;
        this.dataContext = configuration;
        _fido2Storage = new Fido2Storage(configuration);
        // _jwtBearerOptions = jwtBearerOptions.Get(JwtBearerDefaults.AuthenticationScheme);
        // _signingCredentials = signingCredentials;
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("/account/makeCredentialOptions")]
    public CommonResult<object> MakeCredentialOptions([FromForm] string username,
                                            [FromForm] string displayName,
                                            [FromForm] string attType,
                                            [FromForm] string authType,
                                            [FromForm] bool requireResidentKey,
                                            [FromForm] string userVerification)
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

        // 1. Get user from DB by username (in our example, auto create missing users)
        var user = _fido2Storage.GetOrAddUser(username, () => new Fido2User
        {
            DisplayName = displayName,
            Name = username,
            Id = System.Text.Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()),  // Encoding.UTF8.GetBytes(username) // byte representation of userID is required
        });

        // 2. Get user existing keys by username
        var existingKeys = _fido2Storage.GetCredentialsByUser(user).Select(c => c.Descriptor).ToList();

        // 3. Create options
        var authenticatorSelection = new AuthenticatorSelection
        {
            RequireResidentKey = requireResidentKey,
            UserVerification = userVerification.ToEnum<UserVerificationRequirement>()
        };

        if (!string.IsNullOrEmpty(authType))
            authenticatorSelection.AuthenticatorAttachment = authType.ToEnum<AuthenticatorAttachment>();

        var exts = new AuthenticationExtensionsClientInputs()
        {
            Extensions = true,
            UserVerificationMethod = true,
        };

        var options = _fido2.RequestNewCredential(user, existingKeys, authenticatorSelection, attType.ToEnum<AttestationConveyancePreference>(), exts);

        var session = new SessionTable
        {
            Pk = Guid.NewGuid().ToString(),
            Content = options.ToJson(),
            User = System.Text.Encoding.UTF8.GetString(user.Id),
            CreateTime = DateTime.UtcNow,
            UpdateTime = DateTime.UtcNow,
            Type = "fido2.attestationOptions",
        };
        dataContext.Sessions.Add(session);
        dataContext.SaveChanges();

        return new CommonResult<object>
        {
            Code = 200,
            Message = "success",
            Data = new
            {
                session = session.Pk,
                options = options,
            }
        };
    }

    [HttpPost]
    [AllowAnonymous]
    [Route("/account/makeCredential")]
    public async Task<CommonResult<object>> MakeCredential([FromBody] MakeCredentialFormBody attestationResponse, CancellationToken cancellationToken)
    {
        logger.LogDebug($"attestationResponse {attestationResponse}");
        if (attestationResponse == null || attestationResponse.credential == null)
        {
            //return Json(new CredentialMakeResult(status: "error", errorMessage: "No credentials object found in request", result: null));
            return new CommonResult<object>
            {
                Code = 400,
                Message = "No credentials object found in request",
                Data = null
            };
        }

        var session = dataContext.Sessions.FirstOrDefault(s => s.Pk == attestationResponse.session);
        if (session == null)
        {
            //return Json(new CredentialMakeResult(status: "error", errorMessage: "Registration failed", result: null));
            return new CommonResult<object>
            {
                Code = 400,
                Message = "Registration failed",
                Data = null
            };
        }
        // 1. get the options we sent the client
        //var jsonOptions = HttpContext.Session.GetString("fido2.attestationOptions");
        var options = CredentialCreateOptions.FromJson(session.Content);

        // 2. Create callback so that lib can verify credential id is unique to this user
        IsCredentialIdUniqueToUserAsyncDelegate callback = async (args, cancellationToken) =>
        {
            var users = await _fido2Storage.GetUsersByCredentialIdAsync(args.CredentialId, cancellationToken);
            if (users.Count > 0)
                return false;

            return true;
        };

        // 2. Verify and make the credentials
        var success = await _fido2.MakeNewCredentialAsync(attestationResponse.credential, options, callback, cancellationToken: cancellationToken);

        if (success == null || success.Result == null)
        {
            //return Json(new CredentialMakeResult(status: "error", errorMessage: "Registration failed", result: null));
            return new CommonResult<object>
            {
                Code = 400,
                Message = "Registration failed",
                Data = null
            };
        }

        // 3. Store the credentials in db
        _fido2Storage.AddCredentialToUser(options.User, new StoredCredential
        {
            Descriptor = new PublicKeyCredentialDescriptor(success.Result.CredentialId),
            PublicKey = success.Result.PublicKey,
            UserHandle = success.Result.User.Id,
            SignatureCounter = success.Result.Counter,
            CredType = success.Result.CredType,
            RegDate = DateTime.UtcNow,
            AaGuid = success.Result.Aaguid
        });

        // 4. return "ok" to the client
        //return Json(success); 
        return new CommonResult<object>
        {
            Code = 200,
            Message = "success",
            Data = success
        };
    }

    [HttpPost]
    [Route("/account/userinfo")]
    public CommonResult<object> UserInfo()
    {
        var user = HttpContext.User;
        user.Claims.ToList().ForEach(c =>
        {
            logger.LogDebug($"{c.Type} {c.Value}");
        });
        logger.LogDebug($"user name: {user.Identity?.Name}");
        return new CommonResult<object>
        {
            Code = 200,
            Message = "success",
            Data = "这是获取到的用户信息"
        };
    }
}

public class MakeCredentialFormBody
{
    public string session { get; set; } = "";
    public AuthenticatorAttestationRawResponse? credential { get; set; }
}