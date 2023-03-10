using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Gliese.Models;
using System.Web;
using Venus.Models;

namespace Gliese.Controllers;

[ApiController]
public class PictureController : ControllerBase
{
    private readonly ILogger<PictureController> logger;
    private readonly VenusDatabaseContext dataContext;

    public PictureController(ILogger<PictureController> logger, VenusDatabaseContext configuration)
    {
        this.logger = logger;
        this.dataContext = configuration;
    }

    [Route("/restful/picture/get")]
    public CommonResult Get(string pk)
    {
        var model = dataContext.Pictures.FirstOrDefault(m => m.Pk == pk);
        if (model == null)
        {
            return new CommonResult { Code = 404, Message = "图片不存在" };
        }

        return new CommonResult { Code = 200, Data = model };
    }

    [Route("/restful/picture/select")]
    public CommonResult Select(int offset = 0, int limit = 10)
    {
        var models = dataContext.Pictures.Skip(offset).Take(limit).ToList();
        if (models == null)
        {
            return new CommonResult { Code = 404, Message = "图片不存在" };
        }
        var totalCount = dataContext.Pictures.Count();

        return new CommonResult
        {
            Code = 200,
            Data = new
            {
                list = models,
                count = totalCount
            }
        };
    }
}