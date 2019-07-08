
namespace HealthCheck.Models
{
    public class HttpService : Service
    {
        public override string Type { get => "HttpService"; }

        //public string Id { get => $"{Name}-{Url}"; }

        public string Url { get; set; }
    }
}
