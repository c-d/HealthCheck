
using Newtonsoft.Json;

namespace HealthCheck.Models
{
    public class HttpService : Service
    {
        [JsonProperty("type")]
        public override string Type { get => "HttpService"; }

        //public string Id { get => $"{Name}-{Url}"; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
