using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Gliese.Models;
using System.Web;

namespace Gliese.Controllers;

[ApiController]
public class ArticleController : ControllerBase
{
    private readonly ILogger<ArticleController> logger;
    private readonly BloggingContext dataContext;

    public ArticleController(ILogger<ArticleController> logger, BloggingContext configuration)
    {
        this.logger = logger;
        this.dataContext = configuration;
    }

    [Route("/server/article/get")]
    public CommonResult<object> Get(string pk)
    {
        var model = dataContext.Articles.FirstOrDefault(m => m.Pk == pk);
        if (model == null)
        {
            return new CommonResult<object> { Code = 404, Message = "文章不存在" };
        }

        return new CommonResult<object> { Code = 200, Data = model };
    }

    [Route("/server/article/select")]
    public CommonResult<object> Select(int offset = 0, int limit = 10)
    {
        var models = dataContext.Articles.Skip(offset).Take(limit).ToList();
        if (models == null)
        {
            return new CommonResult<object> { Code = 404, Message = "文章不存在" };
        }
        var totalCount = dataContext.Articles.Count();

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

    [Route("/server/article/viewer/update")]
    public CommonResult<object> ArticleViewerUpdate(string article, string client_ip)
    {
        logger.LogDebug($"client_ip {client_ip}");
        if (String.IsNullOrEmpty(client_ip) || String.IsNullOrEmpty(article))
        {
            return new CommonResult<object> { Code = 400 };
        }

        using (var transaction = dataContext.Database.BeginTransaction())
        {
            var viewer = dataContext.ArticleViewerTable.FirstOrDefault(m => m.Article == article && m.NetAddr == client_ip);
            if (viewer != null)
            {
                if (viewer.UpdateTime.AddHours(24) > DateTime.UtcNow)
                {
                    logger.LogDebug($"24小时内更新过, 不再更新: ${client_ip}");
                    return new CommonResult<object> { Code = 200 };
                }
                else
                {
                    dataContext.Attach(viewer);
                    viewer.UpdateTime = DateTime.UtcNow;
                    dataContext.Entry(viewer).Property(p => p.UpdateTime).IsModified = true;
                }
            }
            else
            {
                var model = new ArticleViewerTable { Article = article, NetAddr = client_ip, CreateTime = DateTime.UtcNow, UpdateTime = DateTime.UtcNow };
                dataContext.ArticleViewerTable.Add(model);
            }

            var articleView = dataContext.ArticleExtendTable.FirstOrDefault(m => m.Pk == article);
            if (articleView == null)
            {
                var model = new ArticleExtendTable { Pk = article, Views = 1 };
                dataContext.ArticleExtendTable.Add(model);
            }
            else
            {
                dataContext.Attach(articleView);
                articleView.Views += 1;
                dataContext.Entry(articleView).Property(p => p.Views).IsModified = true;
            }

            dataContext.SaveChanges();

            transaction.Commit();
        } // using transaction


        return new CommonResult<object> { Code = 200 };
    }

}