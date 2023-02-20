using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Gliese.Models;
using System.Text;
using System.Net.Http.Headers;
using System.Net;
using Newtonsoft.Json;

namespace Gliese.Controllers;

[ApiController]
public class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> logger;
    private readonly BloggingContext dataContext;


    public AccountController(ILogger<AccountController> logger, BloggingContext configuration)
    {
        this.logger = logger;
        this.dataContext = configuration;
    }

    [Route("/server/oauth2/code")]
    public async Task<IActionResult> ExchangeToken(string code = "")
    {
        var query = Request.QueryString;
        logger.LogInformation($"ExchangeToken call {code} | {query}");
        var clientId = "pwa";
        var parameters = new Dictionary<string, string>();
        parameters.Add("client_id", clientId);
        parameters.Add("grant_type", "authorization_code");
        parameters.Add("client_secret", "foobar");

        parameters.Add("code", code);
        parameters.Add("redirect_uri", "https://debug.polaris.direct/server/oauth2/code");
        HttpClient _httpClient = new HttpClient();

        var response = await _httpClient.PostAsync("https://debug.multiverse.direct/server/oauth2/token",
        new FormUrlEncodedContent(parameters));
        var responseValue = await response.Content.ReadAsStringAsync();
        logger.LogInformation($"responseValue {responseValue}");

        var tokenModel = JsonConvert.DeserializeObject<OAuth2Token>(responseValue);
        if (tokenModel == null)
        {
            return Content("tokenModel is null");
        }
        var resourceParameters = new Dictionary<string, string>();
        resourceParameters.Add("access_token", tokenModel.AccessToken);
        logger.LogInformation($"AccessToken {tokenModel.AccessToken}");


        var protectedUrl = $"https://debug.multiverse.direct/resource/protected?token={tokenModel.AccessToken}&scope={tokenModel.Scope}";
        logger.LogDebug($"protectedUrl {protectedUrl}");
        var resourceResponse = await _httpClient.GetAsync(protectedUrl);
        var resourceResponseValue = await resourceResponse.Content.ReadAsStringAsync();
        logger.LogInformation($"resourceResponseValue {resourceResponseValue}");

        var oauth2User = JsonConvert.DeserializeObject<CommonResult<OAuth2User>>(resourceResponseValue);
        if (oauth2User == null || oauth2User.Data == null)
        {
            return Content("oauth2User is null");
        }
        using (var transaction = dataContext.Database.BeginTransaction())
        {
            var dbUser = dataContext.Users.FirstOrDefault(m => m.Username == oauth2User.Data.Username);
            if (dbUser != null)
            {
                dataContext.Attach(dbUser);
                dbUser.Nickname = oauth2User.Data.Nickname;
                dataContext.Entry(dbUser).Property(p => p.Nickname).IsModified = true;
                dbUser.AccessToken = tokenModel.AccessToken;
                dataContext.Entry(dbUser).Property(p => p.AccessToken).IsModified = true;
            }
            else
            {
                dbUser = new UserTable
                {
                    Pk = Guid.NewGuid().ToString(),
                    Username = oauth2User.Data.Username,
                    Nickname = oauth2User.Data.Nickname,
                    AccessToken = tokenModel.AccessToken
                };
                dataContext.Users.Add(dbUser);
            }
            dataContext.SaveChanges();

            transaction.Commit();
        }

        HttpContext.Response.Cookies.Append("openid", oauth2User.Data.Username, new CookieOptions
        {
            Expires = DateTimeOffset.Now.AddMinutes(30),
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Secure = true
        });

        return Redirect("https://debug.polaris.direct");
    }

    [Route("/server/account/logined")]
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