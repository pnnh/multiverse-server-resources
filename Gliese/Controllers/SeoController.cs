using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Gliese.Models;
using X.Web.Sitemap;
using System.Text;
using Gliese.Services;

namespace Gliese.Controllers;

public class SeoController : Controller
{
    private readonly ILogger<SeoController> logger;
    private readonly BloggingContext dataContext;


    public SeoController(ILogger<SeoController> logger, BloggingContext configuration)
    {
        this.logger = logger;
        dataContext = configuration;
    }

    [Route("seo/sitemap")]
    public IActionResult Sitemap()
    {
        //using var db = new BloggingContext(_configuration); 

        // Read
        // Console.WriteLine("Querying for a blog");

        var sitemap = new Sitemap();
 
        sitemap.Add(new Url
        {
            ChangeFrequency = ChangeFrequency.Daily,
            Location = PolarisConfig.SelfUrl,
            Priority = 0.5,
            TimeStamp = DateTime.Now
        });

        var fBlogs = dataContext.Articles.Where(b => b.Title != "").ToList();
        foreach (var a in fBlogs)
        {
            //Console.WriteLine($"article: {a.Pk} {a.Title}");
            var readUrl = PolarisConfig.SelfUrl + $"/article/read/{a.Pk}";
            var item = new Url
            {
                ChangeFrequency = ChangeFrequency.Yearly,
                Location = readUrl,
                Priority = 0.5,
                TimeStamp = a.UpdateTime
            };
            sitemap.Add(item);
        }

        var sitemapXmlStr = sitemap.ToXml();

        return new ContentResult
        {
            ContentType = "application/xml",
            Content = sitemapXmlStr,
            StatusCode = 200
        };
    }

}