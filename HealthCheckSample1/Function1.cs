using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using HealthCheck.Models;
using System.Collections.Generic;

namespace HealthCheck
{
    public static class Function1
    {
        private static string serviceName = "MyService1";
        // HealthChecker should be a singleton
        private static HealthChecker healthChecker = new HealthChecker(InitDependencies());

        [FunctionName("Function1")]
        public static async Task<HttpResponseMessage> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "healthcheck")] HttpRequest req,
            ILogger log,
            ExecutionContext context)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            return await healthChecker.GetHealthCheckHttpResponse(serviceName);
        }

        // Can either provide dependencies as JSON config or create them 
        public static List<HttpService> InitDependencies()
        {
            var dependencies = new List<HttpService>
            {
                new HttpService() { Name = "Google", Url = "https://google.com" },
                new HttpService() { Name = "Azure", Url = "https://azure.com" },
                new HttpService() { Name = "HealthCheckSample2", Url = "http://localhost:5860/api/healthcheck" }
            };
            return dependencies;
        }
    }
}
