using Gliese.Models; 
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gliese.Controllers;


[Authorize()]
public class AccountController : Controller
{
    private readonly ILogger<AccountController> logger;
    private readonly BloggingContext dataContext;
    public AccountController(ILogger<AccountController> logger, BloggingContext configuration)
    {
        this.logger = logger;
        this.dataContext = configuration;
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