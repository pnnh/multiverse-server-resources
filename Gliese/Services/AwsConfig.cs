
using System.Net;
using Amazon.AppConfig;
using Amazon.AppConfig.Model;
using Amazon.AppConfigData;
using Amazon.AppConfigData.Model;
using Gliese.Models;
using Microsoft.EntityFrameworkCore;
namespace Gliese.Services
{
    public class PolarisConfig
    {
        public static string SelfUrl = "https://www.polaris.direct";
    }

    public class RedisConfigModel
    {
        public string Host = "";
        public string Password = "";
        public int Database = 0;

        public RedisConfigModel(string uri)
        {
            Uri myUri = new Uri(uri, UriKind.Absolute);
            this.Host = myUri.Host;
            var auth = myUri.AbsoluteUri.Split(":");
            if (auth.Length > 1)
            {
                this.Password = auth[1];
            }
            var queryDictionary = System.Web.HttpUtility.ParseQueryString(myUri.Query);
            var dbStr = queryDictionary["db"];
            if (!String.IsNullOrEmpty(dbStr))
            {
                var db = int.Parse(dbStr);
                this.Database = db;
            }
        }

    }

    public class ConfigModel
    {
        public string PgDsn { get; set; } = "";
        public RedisConfigModel? Redis { get; set; }
    }

    public class AwsConfig
    {
        static private ConfigModel? currentConfig;
        //static Dictionary<string, string> DefaultMap = new Dictionary<string, string>();

        static AwsConfig()
        {
            //ParseAppConfig();
        }

        static async Task<string> LoadConfigFromAws(string fileName, string envName)
        {

            IAmazonAppConfigData client = new AmazonAppConfigDataClient(Amazon.RegionEndpoint.APEast1);

            var sessionRequest = new StartConfigurationSessionRequest();

            sessionRequest.ApplicationIdentifier = "polaris.direct";

#if (DEBUG)
            sessionRequest.ApplicationIdentifier = "debug.polaris.direct";
#endif

            sessionRequest.ConfigurationProfileIdentifier = fileName;
            sessionRequest.EnvironmentIdentifier = envName;

            var sessionResponse = await client.StartConfigurationSessionAsync(sessionRequest);
            if (sessionResponse.HttpStatusCode != HttpStatusCode.Created)
                throw new Exception("StartConfigurationSession Response HTTP Status Code does not indicate success");


            var request = new GetLatestConfigurationRequest();

            request.ConfigurationToken = sessionResponse.InitialConfigurationToken;

            var response = await client.GetLatestConfigurationAsync(request);
            if (response.HttpStatusCode != HttpStatusCode.OK)
                throw new Exception("GetLatestConfigurationAsync Response HTTP Status Code does not indicate success");
  
            StreamReader reader = new StreamReader(response.Configuration);
            string text = reader.ReadToEnd();
 
            return text;
        }

        public static async Task<ConfigModel> GetConfig()
        {
            if (currentConfig != null)
            {
                return currentConfig;
            }
            var configModel = new ConfigModel();
            var configContent = await LoadConfigFromAws("main.config", "default");
            if (String.IsNullOrEmpty(configContent))
                throw new Exception("aws 配置为空");
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
                    case "REDIS":
                        configModel.Redis = new RedisConfigModel(value);
                        break;
                }
            }
            currentConfig = configModel;
            return configModel;
        }
    }
}