


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

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseSwagger();
                app.UseSwaggerUI();

            }

            app.UseRouting();

            app.MapControllers();

            app.Run();
        }

    }
}
