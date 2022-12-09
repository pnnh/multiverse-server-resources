


using System.Net;
using Amazon.AppConfig;
using Amazon.AppConfig.Model;
using Amazon.AppConfigData;
using Amazon.AppConfigData.Model;

namespace Gliese
{
    public class Gliese
    {
        public static async Task Main(string[] args)
        {

            getConfig();

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

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


        static async void getConfig()
        {


            IAmazonAppConfigData client = new AmazonAppConfigDataClient(Amazon.RegionEndpoint.APEast1);

            var sessionRequest = new StartConfigurationSessionRequest();

            sessionRequest.ApplicationIdentifier = "sfx";

            sessionRequest.ConfigurationProfileIdentifier = "debug.config";
            sessionRequest.EnvironmentIdentifier = "debug";



            var sessionResponse = await client.StartConfigurationSessionAsync(sessionRequest);
            if (sessionResponse.HttpStatusCode != HttpStatusCode.Created)
                throw new Exception("StartConfigurationSession Response HTTP Status Code does not indicate success");


            var request = new GetLatestConfigurationRequest();

            request.ConfigurationToken = sessionResponse.InitialConfigurationToken;

            var response = await client.GetLatestConfigurationAsync(request);
            if (response.HttpStatusCode != HttpStatusCode.OK)
                throw new Exception("GetLatestConfigurationAsync Response HTTP Status Code does not indicate success");

            //var stream = new MemoryStream();
            // response.Configuration.CopyTo(stream);


            StreamReader reader = new StreamReader(response.Configuration);
            string text = reader.ReadToEnd();

            Console.WriteLine($"Config Content \n{text}");

        }
    }
}
