
using Newtonsoft.Json;

namespace HealthCheck.Models
{
    public abstract class Service
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("type")]
        public abstract string Type { get; }
    }
}
