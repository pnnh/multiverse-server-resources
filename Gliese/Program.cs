


using System.Net;
using Gliese.Models;
using Microsoft.EntityFrameworkCore;
using Gliese.Services;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.CookiePolicy;

namespace Gliese
{
    public class Gliese
    {
        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            var services = builder.Services;

            builder.Logging.ClearProviders().AddSimpleConsole(options =>
            {
                options.SingleLine = true;
                options.IncludeScopes = true;
                options.UseUtcTimestamp = true;
            });


            builder.Services.AddControllers().AddJsonOptions(options =>
            options.JsonSerializerOptions.Encoder = JavaScriptEncoder.Create(UnicodeRanges.All));
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var pgDsn = PolarisConfig.GetConfig("CSHARP_DSN");

            builder.Services.AddDbContext<BloggingContext>(options =>
            {
                options.UseNpgsql(pgDsn);
            });

            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
            services.AddCookiePolicy(options =>
            {  
                options.HttpOnly = HttpOnlyPolicy.Always;
                options.MinimumSameSitePolicy = SameSiteMode.Strict;
            });


            services.AddFido2(options =>
            {
                options.ServerDomain = "debug.polaris.direct";
                options.ServerName = "Polaris";
                options.Origins = new HashSet<string>() { "https://debug.polaris.direct" };
                options.TimestampDriftTolerance = 300000;
                options.MDSCacheDirPath = "";
            })
            .AddCachedMetadataService(config =>
            {
                config.AddFidoMetadataRepository(httpClientBuilder =>
                {
                    //TODO: any specific config you want for accessing the MDS
                });
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseSwagger();
                app.UseSwaggerUI();
            }
 
            app.UseRouting();
            app.UseStaticFiles();

            app.MapControllers();

            app.Run();
        }

    }
}
