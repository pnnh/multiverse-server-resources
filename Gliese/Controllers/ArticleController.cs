using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Gliese.Models;

namespace Gliese.Controllers;

public class ArticleController : Controller
{
    private readonly ILogger<ArticleController> logger;
    private readonly BloggingContext dataContext;


    public ArticleController(ILogger<ArticleController> logger, BloggingContext configuration)
    {
        this.logger = logger;
        dataContext = configuration;
    }

    [Route("article/read/{pk}")]
    public IActionResult Read(string pk)
    {
        var model = dataContext.Articles.FirstOrDefault(m => m.Pk == pk);
        if (model == null)
        {
            return Content("404");
        }
        
        var viewModel = model.ToViewModel();
        return View(viewModel);
    }
}