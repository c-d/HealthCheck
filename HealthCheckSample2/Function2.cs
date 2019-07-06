using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Collections.Generic;
using HealthCheck.Models;

namespace HealthCheckSample2
{
    public static class Function2
    {
        // HttpClient should be a singleton
        private static readonly HealthCheck.HealthCheck healthCheck = new HealthCheck.HealthCheck(new HttpClient());

        private static string serviceName = "MyService2";
        private static List<HttpDependency> dependencies;

        [FunctionName("Function2")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "healthcheck")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            InitDependencies();

            return await healthCheck.PerformHealthCheck(serviceName, dependencies, true, true);
        }

        public static void InitDependencies()
        {
            dependencies = new List<HttpDependency>();
            dependencies.Add(new HttpDependency("Wikipedia", "https://en.wikipedia.org"));
            dependencies.Add(new HttpDependency("Azure", "https://azure.com"));
            dependencies.Add(new HttpDependency("Argl-bargl", "https://asdagrfdsg.zzz"));
        }
    }
}
