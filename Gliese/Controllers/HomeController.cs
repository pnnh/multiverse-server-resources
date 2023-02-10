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

    [Route("/restful/article")]
    public CommonResult Index(int page = 1)
    {
        return new CommonResult { Code = 200, Message = "Polaris业务接口服务" };
    }

}