
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
        static private ConfigModel? currentConfig;
        //static Dictionary<string, string> DefaultMap = new Dictionary<string, string>();

        static AwsConfig()
        {
            //ParseAppConfig();
        }

        static async Task<string> LoadConfigFromAws(string fileName, string envName)
        {
            const string configUrl = "http://127.0.0.1:8001/config/select?project=polaris"; 
            using var client = new HttpClient();

            var response = await client.GetAsync(configUrl); 
            var result = await response.Content.ReadAsStringAsync();
    
            return result;
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
                }
            }
            currentConfig = configModel;
            return configModel;
        }
    }
}