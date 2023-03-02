using Fido2NetLib;
using Fido2NetLib.Development;
using Fido2NetLib.Objects;
using Gliese.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
            return new CommonResult<object>
            {
                Code = 400,
                Message = "Username is required",
                Data = null
            };
        }
        if (string.IsNullOrEmpty(displayName) || string.IsNullOrEmpty(displayName.Trim()))
        {
            displayName = username;
        }

        var user = _fido2Storage.GetOrAddUser(username, () => new Fido2User
        {
            DisplayName = displayName,
            Name = username,
            Id = System.Text.Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()), 
        });

        var existingKeys = _fido2Storage.GetCredentialsByUser(user).Select(c => c.Descriptor).ToList();

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
            return new CommonResult<object>
            {
                Code = 400,
                Message = "Registration failed",
                Data = null
            };
        }
        var options = CredentialCreateOptions.FromJson(session.Content);

        IsCredentialIdUniqueToUserAsyncDelegate callback = async (args, cancellationToken) =>
        {
            var users = await _fido2Storage.GetUsersByCredentialIdAsync(args.CredentialId, cancellationToken);
            if (users.Count > 0)
                return false;

            return true;
        };

        var success = await _fido2.MakeNewCredentialAsync(attestationResponse.credential, options, callback, cancellationToken: cancellationToken);

        if (success == null || success.Result == null)
        {
            return new CommonResult<object>
            {
                Code = 400,
                Message = "Registration failed",
                Data = null
            };
        }

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

        return new CommonResult<object>
        {
            Code = 200,
            Message = "success",
            Data = success
        };
    }

    [HttpPost]
    [Route("/account/userinfo")]
    public CommonResult<AccountTable> UserInfo()
    {
        var user = HttpContext.User;
        user.Claims.ToList().ForEach(c =>
        {
            logger.LogDebug($"{c.Type} {c.Value}");
        });
        logger.LogDebug($"user name: {user.Identity?.Name}");
        if (user.Identity == null || string.IsNullOrEmpty(user.Identity.Name))
        {
            return new CommonResult<AccountTable>
            {
                Code = 400,
                Message = "用户未登录"
            };
        }
        var userInfo = dataContext.Accounts.FirstOrDefault(a => a.Account == user.Identity.Name);
        if (userInfo == null)
        {
            return new CommonResult<AccountTable>
            {
                Code = 400,
                Message = "获取用户信息出错: 用户不存在"
            };
        }
        return new CommonResult<AccountTable>
        {
            Code = 200,
            Message = "success",
            Data = userInfo
        };
    }
}

public class MakeCredentialFormBody
{
    public string session { get; set; } = "";
    public AuthenticatorAttestationRawResponse? credential { get; set; }
}