
using System.Collections;
using System.Net;
using Gliese.Models;
using Microsoft.EntityFrameworkCore;

namespace Gliese.Services
{
    public class PolarisConfig
    {
        public static string SelfUrl = "https://www.polaris.direct";
    }

    public class ConfigModel
    {
        public string PgDsn { get; set; } = "";
    }

    public class AwsConfig
    {
        static AwsConfig()
        {
            //ParseAppConfig();
        }

        static Dictionary<string, string> LoadConfigFromAws(string fileName, string envName)
        {
            // const string configUrl = "http://127.0.0.1:8001/config/select?project=polaris"; 
            // using var client = new HttpClient();

            // var response = await client.GetAsync(configUrl); 
            // var result = await response.Content.ReadAsStringAsync();

            // return result;
            var dict = new Dictionary<string, string>();
            foreach (DictionaryEntry item in Environment.GetEnvironmentVariables())
            {
                if (item.Key == null || item.Value == null)
                    continue;
                var key = item.Key.ToString();
                var value = item.Value.ToString();
                if (key == null || value == null)
                    continue;
                dict[key] = value;
            }
            return dict;
        }

        public static ConfigModel GetConfig()
        {
            var configModel = new ConfigModel();
            var configContent = LoadConfigFromAws("main.config", "default");
            foreach (var e in configContent)
            {
                switch (e.Key)
                {
                    case "CSHARP_DSN":
                        configModel.PgDsn = e.Value;
                        break;
                }
            }
            return configModel;
        }
    }
}