using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using HealthCheck;
using HealthCheck.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HealthCheckOrchestrator
{
    public class HealthCheckOrchestratorFunction
    {   
        private readonly IDocumentClient DocumentClient;
        private readonly IHealthChecker HealthChecker;
        private readonly IConfigurationRoot Config;
        
        public HealthCheckOrchestratorFunction(
            IDocumentClient documentClient, 
            IHealthChecker healthChecker,
            IConfigurationRoot config)
        {
            DocumentClient = documentClient;
            HealthChecker = healthChecker;
            Config = config;
        }
        

        [FunctionName("HealthCheckOrchestratorFunction")]
        public async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");
            
            var healthCheckResults = await HealthChecker.GetHealthCheckResult("Orchestrator", true, false);

            foreach (var healthCheckResult in healthCheckResults.RequiredServices)
            {
                var resultString = healthCheckResult.Available ? "up" : "down";
                log.LogInformation($"{healthCheckResult.Name}: {resultString}");

                await DocumentClient.CreateDocumentAsync(Config["Database_Collection_Link"], healthCheckResult);
            }
        }
    }
}
