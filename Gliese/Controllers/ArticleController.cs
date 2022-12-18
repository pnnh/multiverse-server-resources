using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Gliese.Models;
using StackExchange.Redis;
using System.Web;

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
        await UpdateViews(pk);

        var viewModel = model.ToViewModel(); 
        return View(viewModel);
    }

    private async Task UpdateViews(string pk)
    {
        var clientIp = HttpUtils.GetClientAddress(HttpContext);
        logger.LogInformation($"clientIp {clientIp}");
        var redisKey = $"article:{pk}:viewer:{clientIp}";
        var db = redis.GetDatabase();
        var foo = await db.StringGetAsync(redisKey);
        logger.LogInformation($"foo {foo}");
        if (!foo.IsNull)
        {
            return;
        }

        logger.LogDebug("未查看，更新浏览次数");

        using (var transaction = dataContext.Database.BeginTransaction())
        {

            var articleView = dataContext.ArticleViewTable.FirstOrDefault(m => m.Pk == pk);
            if (articleView == null)
            {
                var model = new ArticleViewTable { Pk = pk, Views = 1 };
                dataContext.ArticleViewTable.Add(model);
            }
            else
            {
                dataContext.Attach(articleView);
                articleView.Views += 1;
                dataContext.Entry(articleView).Property(p => p.Views).IsModified = true;
            }

            dataContext.SaveChanges();

            transaction.Commit();
        }
        var readTime = DateTime.UtcNow.ToString();
        var setOk = await db.StringSetAsync(redisKey, readTime, TimeSpan.FromHours(24));
        logger.LogDebug($"setOk {setOk}");

        return;
    }
}