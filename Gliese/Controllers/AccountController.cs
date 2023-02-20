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


    public AccountController(ILogger<AccountController> logger)
    {
        this.logger = logger;
    }

    [Route("/server/oauth2/code")]
    public async Task<IActionResult> ExchangeToken(string code = "")
    {
        logger.LogInformation($"ExchangeToken call {code}");
        var clientId = "pwa";
        var parameters = new Dictionary<string, string>();
        parameters.Add("client_id", clientId);
        parameters.Add("grant_type", "authorization_code");
        parameters.Add("client_secret", "foobar");

        if (!string.IsNullOrEmpty(code))
        {
            parameters.Add("code", code);
            parameters.Add("redirect_uri", "https://debug.polaris.direct/server/oauth2/code");
        }
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

        var protectedUrl = $"https://debug.multiverse.direct/resource/protected?token={tokenModel.AccessToken}&scope={tokenModel.Scope}";
        logger.LogDebug($"protectedUrl {protectedUrl}");
        var resourceResponse = await _httpClient.GetAsync(protectedUrl);
        var resourceResponseValue = await resourceResponse.Content.ReadAsStringAsync();
        logger.LogInformation($"resourceResponseValue {resourceResponseValue}");


        HttpContext.Response.Cookies.Append("openid", "请求到的openid", new CookieOptions
        {
            Expires = DateTimeOffset.Now.AddMinutes(30),
            HttpOnly = true,
            SameSite = SameSiteMode.Strict,
            Secure = true
        });

        return Redirect("https://debug.polaris.direct");
    }
}