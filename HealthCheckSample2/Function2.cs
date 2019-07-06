using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace HealthCheckSample2
{
    public static class Function2
    {
        private static string serviceName = "MyService2";
        // HttpClient should be a singleton
        private static readonly HttpClient httpClient = new HttpClient();

        [FunctionName("Function2")]
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

            var healthCheck = new HealthCheck.HealthChecker(httpClient, config["Dependencies"]);

            return await healthCheck.GetHealthCheckHttpResponse(serviceName, true, true);
        }
    }
}
