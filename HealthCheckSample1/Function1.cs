using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Microsoft.Extensions.Configuration;

namespace HealthCheck
{
    public static class Function1
    {
        private static string serviceName = "MyService1";
        // HttpClient should be a singleton
        private static HttpClient httpClient = new HttpClient();

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
            
            var healthCheck = new HealthChecker(httpClient, config["Dependencies"]);

            return await healthCheck.GetHealthCheckHttpResponse(serviceName);
        }
    }
}
