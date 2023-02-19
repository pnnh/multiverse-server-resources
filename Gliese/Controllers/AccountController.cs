using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Gliese.Models;
using System.Text;
using System.Net.Http.Headers;
using System.Net;

namespace Gliese.Controllers;

[ApiController]
public class AccountController : ControllerBase
{
    private readonly ILogger<AccountController> logger;


    public AccountController(ILogger<AccountController> logger)
    {
        this.logger = logger;
    }

    [Route("/restful/login/finish")]
    public async Task<CommonResult> ExchangeToken(string code = "")
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
            parameters.Add("redirect_uri", "https://debug.polaris.direct/login/callback"); //和获取 authorization_code 的 redirect_uri 必须一致，不然会报错
        }
        HttpClient _httpClient = new HttpClient();
        // _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
        //     "Basic",
        //     Convert.ToBase64String(Encoding.ASCII.GetBytes(clientId + ":" + clientSecret)));

        var response = await _httpClient.PostAsync("https://debug.multiverse.direct/server/oauth2/token",
        new FormUrlEncodedContent(parameters));
        var responseValue = await response.Content.ReadAsStringAsync();


        logger.LogInformation($"responseValue {responseValue}");

        return new CommonResult { Code = 200, Message = "Polaris业务接口服务" };
    }


}