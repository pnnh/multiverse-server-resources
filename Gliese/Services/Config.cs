
using System.Collections;
using System.Net;
using Gliese.Models;
using Microsoft.EntityFrameworkCore;

namespace Gliese.Services
{
    public class PolarisConfig
    {
        private static Dictionary<string, string> configContent = new Dictionary<string, string>();
        public static string SelfUrl = "https://www.polaris.direct";

        static PolarisConfig()
        {
#if DEBUG
            SelfUrl = "https://debug.polaris.direct";
#endif
            configContent = LoadConfigFromEnv();
        }
 
        static Dictionary<string, string> LoadConfigFromEnv()
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

        public static string GetConfig(string configKey)
        {  
            return configContent[configKey]; 
        }
    }
}