using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Gliese.Models;
using System.Web;
using Microsoft.AspNetCore.Authorization;

namespace Gliese.Controllers;

[Authorize()]
[ApiController]
public class ResourceController : ControllerBase
{
    private readonly ILogger<ResourceController> logger;
    private readonly DatabaseContext dataContext;

    public ResourceController(ILogger<ResourceController> logger, DatabaseContext configuration)
    {
        this.logger = logger;
        this.dataContext = configuration;
    }

    [Route("/resource/get")]
    public CommonResult<object> Get(string pk)
    {
        var model = dataContext.Resources.FirstOrDefault(m => m.Pk == pk);
        if (model == null)
        {
            return new CommonResult<object> { Code = 404, Message = "文章不存在" };
        }

        return new CommonResult<object> { Code = 200, Data = model };
    }

    [Route("/resource/select")]
    public CommonResult<object> Select(int offset = 0, int limit = 10)
    {
        var models = dataContext.Resources.Skip(offset).Take(limit).ToList();
        if (models == null)
        {
            return new CommonResult<object> { Code = 404, Message = "文章不存在" };
        }
        var totalCount = dataContext.Resources.Count();

        return new CommonResult<object>
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