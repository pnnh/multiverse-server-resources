
using System.Net;
using Amazon.AppConfig;
using Amazon.AppConfig.Model;
using Amazon.AppConfigData;
using Amazon.AppConfigData.Model;
using Gliese.Models;
using Microsoft.EntityFrameworkCore;
namespace Gliese.Services
{
    public class PolarisConfig {
        public static string SelfUrl = "https://www.polaris.direct";
    }


    public class ConfigModel
    {
        public string PgDsn { get; set; } = "";
    }

    public class AwsConfig
    {
        static private ConfigModel? currentConfig;
        //static Dictionary<string, string> DefaultMap = new Dictionary<string, string>();

        static AwsConfig()
        {
            //ParseAppConfig();
        }

        static async Task<string> LoadConfigFromAws()
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

            //Console.WriteLine($"Config Content \n{text}");
            return text;
        }

        public static async Task<ConfigModel> GetConfig()
        {
            if (currentConfig != null)
            {
                return currentConfig;
            }
            var configModel = new ConfigModel();
            var configContent = await LoadConfigFromAws();
            if (String.IsNullOrEmpty(configContent))
                throw new Exception("aws 配置为空");
            //var configMap = new Dictionary<string, string>();
            var configArray = configContent.Split("\n");
            foreach (var e in configArray)
            {
                var index = e.IndexOf("=");
                if (index < 0) continue;
                var key = e.Substring(0, index);
                var value = e.Substring(index + 1);
                switch (key)
                {
                    case "CSHARP_DSN":
                        configModel.PgDsn = value;
                        break;
                }
            }
            currentConfig = configModel;
            return configModel;
        }
    }
}