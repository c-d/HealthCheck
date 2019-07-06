using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using HealthCheck.Models;
using System.Net.Http;
using System.Collections.Generic;

namespace HealthCheck
{
    public static class Function1
    {
        private static string serviceName = "MyService1";
        private static List<HttpDependency> dependencies;
        // HttpClient should be a singleton
        private static readonly HealthCheck healthCheck = new HealthCheck(new HttpClient());

        [FunctionName("Function1")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "healthcheck")] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            InitDependencies();

            return await healthCheck.PerformHealthCheck(serviceName, dependencies);
        }

        public static void InitDependencies()
        {
            dependencies = new List<HttpDependency>();
            dependencies.Add(new HttpDependency("Google", "https://google.com"));
            dependencies.Add(new HttpDependency("Azure", "https://azure.com"));
            dependencies.Add(new HttpDependency("HealthCheckSample2", "http://localhost:5860/api/healthcheck"));
        }
    }
}
