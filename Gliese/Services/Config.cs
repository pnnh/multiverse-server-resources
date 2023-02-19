
using System.Collections;
using System.Net;
using Gliese.Models;
using Microsoft.EntityFrameworkCore;

namespace Gliese.Services
{
    public class PolarisConfig
    {
        public static string SelfUrl = "https://www.polaris.direct";
        static PolarisConfig()
        {
#if DEBUG
            SelfUrl = "https://debug.polaris.direct";
#endif
        }
    }


    public class ConfigModel
    {
        public string PgDsn { get; set; } = "";
    }

    public class AwsConfig
    {
        static AwsConfig()
        {
        }

        static Dictionary<string, string> LoadConfigFromAws(string fileName, string envName)
        {
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