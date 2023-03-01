


using System.Net;
using Gliese.Models;
using Microsoft.EntityFrameworkCore;
using Gliese.Services;
using System.Text.Encodings.Web;
using System.Text.Unicode; 
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens; 

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

            var secretKey = PolarisConfig.GetConfig("JWT_SECRET");
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
              
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256, SecurityAlgorithms.RsaSha256 },
                    ValidTypes = new[] { JwtConstants.HeaderType },

                    ValidIssuer = "Polaris",
                    ValidateIssuer = true,

                    ValidAudience = "Polaris",
                    ValidateAudience = true,

                    IssuerSigningKey = securityKey,
                    ValidateIssuerSigningKey = true,

                    ValidateLifetime = true,

                    RequireSignedTokens = true,
                    RequireExpirationTime = true,

                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role,

                    ClockSkew = TimeSpan.Zero,
                };

                options.SaveToken = true;

                options.SecurityTokenValidators.Clear();
                options.SecurityTokenValidators.Add(new JwtSecurityTokenHandler());
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

