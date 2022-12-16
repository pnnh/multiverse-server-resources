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

    public IActionResult Index()
    {
        var fBlogs = dataContext.Articles.Where(b => b.Title != "").ToList();
        var model = new IndexViewModel {
            Range = fBlogs,
        };
        return View(model);
    }
  
    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}