using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using Newtonsoft.Json;

namespace DBSizeChecker.ConfigModel
{
    public class Configuration
    {
        public readonly List<HostSettings> Hosts = new List<HostSettings>();

        public Configuration(GoogleSettings google, int retryInterval)
        {
            Google = google;
            RetryInterval = retryInterval;
        }

        [JsonProperty("Output")]
        public GoogleSettings Google { get; }

        public int RetryInterval { get; }

        public static Configuration LoadFromPath(string path)
        {
            if(!File.Exists(path)) throw new ArgumentException("File not exists!");
            var settings = new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.All};
            var json = File.ReadAllText(path);
            Configuration cfg;
            try
                {
                    cfg = JsonConvert.DeserializeObject<Configuration>(json, settings);
                    return cfg;
                } catch(Exception e)
                {
                    throw new DataException("Bad config format!");
                }
        }
    }
}
