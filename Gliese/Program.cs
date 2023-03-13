


using System.Net;
using Gliese.Models;
using Microsoft.EntityFrameworkCore;
using Gliese.Services;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Authentication;

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

            builder.Services.AddDbContext<DatabaseContext>(options =>
            {
                options.UseNpgsql(pgDsn);
            });

            services.AddMemoryCache();

            services.AddAuthentication("BasicAuthentication")
                .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>
                ("BasicAuthentication", null);

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication();

            app.MapControllers();

            app.UseAuthorization();

            app.Run();
        }

    }
}

