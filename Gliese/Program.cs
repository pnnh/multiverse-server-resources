


using System.Net;
using Amazon.AppConfig;
using Amazon.AppConfig.Model;
using Amazon.AppConfigData;
using Amazon.AppConfigData.Model;
using Gliese.Models;
using Microsoft.EntityFrameworkCore;
using Gliese.Services;
using StackExchange.Redis;

namespace Gliese
{
    public class Gliese
    {
        public static async Task Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);
            
            builder.Logging.ClearProviders().AddSimpleConsole(options => {
                options.SingleLine = true;
                options.IncludeScopes = true;
                options.UseUtcTimestamp = true;
            });

            builder.Services.AddControllersWithViews();

            var config = await AwsConfig.GetConfig();
            //Configure other services up here
            var redisConfig = config.Redis;
            if (redisConfig == null)
            {
                throw new Exception("未配置Redis");
            }
            var redisOptions = ConfigurationOptions.Parse(redisConfig.Host); // host1:port1, host2:port2, ...
            redisOptions.Password = redisConfig.Password;
            var multiplexer = ConnectionMultiplexer.Connect(redisOptions);
            builder.Services.AddSingleton<IConnectionMultiplexer>(multiplexer);
 
            builder.Services.AddDbContext<BloggingContext>(options =>
            {
                options.UseNpgsql(config.PgDsn);
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            //app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }

    }
}
