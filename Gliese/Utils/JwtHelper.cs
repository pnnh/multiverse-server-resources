
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Gliese.Services;
using Microsoft.IdentityModel.Tokens;

namespace Gliese.Utils;

public class JwtHelper
{
    public static string GenerateToken(string username, int expireMinutes = 60)
    {
        var secretKey = PolarisConfig.GetConfig("JWT_SECRET");
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var claims = new List<Claim> {
            new Claim(ClaimTypes.Name,"呆呆地Name"),
            new Claim(ClaimTypes.Role,"呆呆地Role"),
            new Claim(JwtRegisteredClaimNames.UniqueName, username),
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier,"身份证号")
        };
        var token = new JwtSecurityToken(
            issuer: "Polaris",
            audience: "Polaris",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expireMinutes),
            signingCredentials: credentials
        );
        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public static ClaimsPrincipal ValidateToken(string token)
    {
        var secretKey = PolarisConfig.GetConfig("JWT_SECRET");
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(secretKey);
        tokenHandler.ValidateToken(token, new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        }, out SecurityToken validatedToken);
        var jwtToken = (JwtSecurityToken)validatedToken;
        var username = jwtToken.Claims.First(x => x.Type == "sub").Value;
        var claims = new List<Claim> {
            new Claim(JwtRegisteredClaimNames.Sub, username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };
        var identity = new ClaimsIdentity(claims);
        return new ClaimsPrincipal(identity);
    }
}