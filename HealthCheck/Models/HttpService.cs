
using Newtonsoft.Json;

namespace HealthCheck.Models
{
    public class HttpService : Service
    {
        [JsonProperty("serviceType")]
        public override string ServiceType { get => "HttpService"; }

        [JsonProperty("url")]
        public string Url { get; set; }
    }
}
