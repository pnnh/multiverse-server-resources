using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Gliese.Models;

namespace Gliese.Controllers;

[ApiController]
public class HomeController : ControllerBase
{
    private readonly ILogger<HomeController> logger;


    public HomeController(ILogger<HomeController> logger)
    {
        this.logger = logger;
    }

    [Route("/")]
    public CommonResult<object> Index(int page = 1)
    {
        return new CommonResult<object> { Code = 200, Message = "Polaris业务接口服务" };
    }

}