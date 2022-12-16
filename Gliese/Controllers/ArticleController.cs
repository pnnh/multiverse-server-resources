using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Gliese.Models;
using StackExchange.Redis;

namespace Gliese.Controllers;

public class ArticleController : Controller
{
    private readonly ILogger<ArticleController> logger;
    private readonly BloggingContext dataContext;
    private readonly IConnectionMultiplexer redis;

    public ArticleController(ILogger<ArticleController> logger, BloggingContext configuration, IConnectionMultiplexer redis)
    {
        this.logger = logger;
        this.dataContext = configuration;
        this.redis = redis;
    }

    [Route("article/read/{pk}")]
    public async Task<IActionResult> Read(string pk)
    {
        var model = dataContext.Articles.FirstOrDefault(m => m.Pk == pk);
        if (model == null)
        {
            return Content("404");
        }
        var db = redis.GetDatabase();
        var foo = await db.StringGetAsync("foo");
        logger.LogInformation($"foo {foo}");


        var viewModel = model.ToViewModel();
        return View(viewModel);
    }
}