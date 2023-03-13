using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Gliese.Models;
using X.Web.Sitemap;
using System.Text;
using Gliese.Services;

namespace Gliese.Controllers;

[ApiController]
public class SeoController : ControllerBase
{
    private readonly ILogger<SeoController> logger;
    private readonly DatabaseContext dataContext;


    public SeoController(ILogger<SeoController> logger, DatabaseContext configuration)
    {
        this.logger = logger;
        dataContext = configuration;
    }

    [Route("/sitemap")]
    public IActionResult Sitemap()
    {
        var sitemap = new Sitemap();

        sitemap.Add(new Url
        {
            ChangeFrequency = ChangeFrequency.Daily,
            Location = PolarisConfig.SelfUrl,
            Priority = 0.5,
            TimeStamp = DateTime.UtcNow
        });

        var fBlogs = dataContext.Resources.Where(b => b.Title != "").ToList();
        foreach (var a in fBlogs)
        {
            var readUrl = PolarisConfig.SelfUrl + $"/resource/read/{a.Pk}";
            var item = new Url
            {
                ChangeFrequency = ChangeFrequency.Yearly,
                Location = readUrl,
                Priority = 0.5,
                TimeStamp = a.UpdateAt
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