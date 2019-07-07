using System;
using System.Net.Http;
using HealthCheck;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HealthCheckOrchestrator
{
    public static class HealthCheckOrchestratorFunction
    {

        [FunctionName("HealthCheckOrchestratorFunction")]
        public static void Run([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Should be a singleton...
            var healthChecker = new HealthChecker(config["Services"]);
            var healthCheckResults = healthChecker.GetHealthCheckResult("Orchestrator", true, false).Result.RequiredServices;

            foreach (var healthCheckResult in healthCheckResults)
            {
                //TODO: Update stored healthcheck results for each service.
            }
        }
    }
}
