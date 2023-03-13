
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Gliese.Services;


public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public BasicAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock
        ) : base(options, logger, encoder, clock)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers["Authorization"].ToString();
        if (authHeader != null && authHeader.StartsWith("basic", StringComparison.OrdinalIgnoreCase))
        {
            var token = authHeader.Substring("Basic ".Length).Trim();
            System.Console.WriteLine(token);
            var credentialstring = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var credentials = credentialstring.Split(':');
            //if (credentials[0] == "admin" && credentials[1] == "admin")
            // ToDo 需要校验用户名和密码
            if (credentials.Length == 2)
            {
                var username = credentials[0];
                var claims = new[] { 
                    //new Claim("name", credentials[0]),  
                    new Claim(ClaimTypes.Name, username),
                    new Claim(ClaimTypes.Role, "user"),
                    new Claim(ClaimTypes.NameIdentifier, username)
                };
                var identity = new ClaimsIdentity(claims, "Basic");
                var claimsPrincipal = new ClaimsPrincipal(identity);
                return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name)));
            }

            //Response.StatusCode = 401;
            //Response.Headers.Add("WWW-Authenticate", "Basic realm=\"dotnetthoughts.net\"");
            return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header"));
        }
        else
        {
          //Response.StatusCode = 401;
            //Response.Headers.Add("WWW-Authenticate", "Basic realm=\"dotnetthoughts.net\"");
            return Task.FromResult(AuthenticateResult.Fail("Invalid Authorization Header2"));
        }
    }
}