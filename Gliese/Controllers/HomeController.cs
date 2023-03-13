using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Gliese.Models;

namespace Gliese.Controllers;

[ApiController]
public class HomeController : ControllerBase
{
    public HomeController()
    {
    }

    [Route("/")]
    public CommonResult<object> Index(int page = 1)
    {
        return new CommonResult<object> { Code = 200, Message = "Multiverse业务接口服务" };
    }

}