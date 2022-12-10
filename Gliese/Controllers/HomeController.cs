using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Gliese.Models;

namespace Gliese.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Data()
    {
        //using var db = new BloggingContext();


        // Create
        // Console.WriteLine("Inserting a new blog");
        // db.Add(new Blog { Url = "http://blogs.msdn.com/adonet" });
        // db.SaveChanges();

        // // Read
        // Console.WriteLine("Querying for a blog");
        // var fBlogs = db.Articles.Where(b => b.Title != "").ToList();
        // foreach (var a in fBlogs)
        // {
        //     Console.WriteLine($"article: {a.Pk} {a.Title}");
        // }

        // // Update
        // Console.WriteLine("Updating the blog and adding a post");
        // blog.Url = "https://devblogs.microsoft.com/dotnet";
        // blog.Posts.Add(
        //     new Post { Title = "Hello World", Content = "I wrote an app using EF Core!" });
        // db.SaveChanges();

        // // Delete
        // Console.WriteLine("Delete the blog");
        // db.Remove(blog);
        // db.SaveChanges();

        return Content("Ok");
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