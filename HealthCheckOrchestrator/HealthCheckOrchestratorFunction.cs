using System;
using System.Threading.Tasks;
using HealthCheck;
using HealthCheckDataAccess;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HealthCheckOrchestrator
{
    public class HealthCheckOrchestratorFunction
    {   
        private readonly IHealthCheckDBClient DBClient;
        private readonly IHealthChecker HealthChecker;
        
        public HealthCheckOrchestratorFunction(
            IHealthCheckDBClient dbClient, 
            IHealthChecker healthChecker)
        {
            DBClient = dbClient;
            HealthChecker = healthChecker;
        }
        

        [FunctionName("HealthCheckOrchestratorFunction")]
        public async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            
            var healthCheckResults = await HealthChecker.GetHealthCheckResult("Orchestrator", true, false);

            foreach (var healthCheckResult in healthCheckResults.RequiredServices)
            {
                var resultString = healthCheckResult.Available ? "up" : "down";
                log.LogInformation($"{healthCheckResult.ServiceName}: {resultString}");

                DBClient.SaveHealthCheckResult(healthCheckResult);
            }
        }
    }
}
