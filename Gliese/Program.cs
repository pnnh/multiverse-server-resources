


using System.Net;
using Gliese.Models;
using Microsoft.EntityFrameworkCore;
using Gliese.Services;
using System.Text.Encodings.Web;
using System.Text.Unicode;

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

            var config = AwsConfig.GetConfig();

            builder.Services.AddDbContext<BloggingContext>(options =>
            {
                options.UseNpgsql(config.PgDsn);
            });

            services.AddMemoryCache();
            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                // Set a short timeout for easy testing.
                options.IdleTimeout = TimeSpan.FromMinutes(2);
                options.Cookie.HttpOnly = true;
                // Strict SameSite mode is required because the default mode used
                // by ASP.NET Core 3 isn't understood by the Conformance Tool
                // and breaks conformance testing
                options.Cookie.SameSite = SameSiteMode.Strict;
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

            app.UseSession();
            app.UseRouting();
            app.UseStaticFiles();

            app.MapControllers();

            app.Run();
        }

    }
}
