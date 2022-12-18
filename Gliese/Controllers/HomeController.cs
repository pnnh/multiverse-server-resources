using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Gliese.Models;

namespace Gliese.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> logger;
    private readonly BloggingContext dataContext;


    public HomeController(ILogger<HomeController> logger, BloggingContext configuration)
    {
        this.logger = logger;
        dataContext = configuration;
    }

    public IActionResult Index(int page = 1)
    {
        var fBlogs = dataContext.Articles.Where(b => b.Title != "").ToList();
        var articlesCount = dataContext.Articles.Count();


        var indexPageSize = 8;
        var currentPage = page;
        var maxPage = articlesCount / indexPageSize;
        if (articlesCount % indexPageSize != 0)
        {
            maxPage += 1;
        }
        if (currentPage > maxPage)
        {
            currentPage = maxPage;
        }
        int startPage = currentPage - 5;
        int endPage = currentPage + 5;
        if (startPage < 1)
        {
            startPage = 1;
        }
        if (endPage > maxPage)
        {
            endPage = maxPage;
        }
        int prevPage = currentPage - 1;
        int nextPage = currentPage + 1;

        var model = new IndexViewModel
        {
            Range = fBlogs,
            Count = articlesCount,
            CurrentPage = page,
            StartPage = startPage,
            EndPage = endPage,
            PrevPage = prevPage,
            NextPage = nextPage,
            MaxPage = maxPage,
        };
        return View(model);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}