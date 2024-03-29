using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using HealthCheckDataAccess;
using HealthCheck;

namespace HealthCheckApi
{
    public class HealthCheckFunction
    {
        private readonly IHealthCheckDBClient DBClient;
        private readonly IHealthChecker HealthChecker;

        public HealthCheckFunction(
            IHealthCheckDBClient dbClient,
            IHealthChecker healthChecker)
        {
            DBClient = dbClient;
            HealthChecker = healthChecker;
        }

        [FunctionName("GetHealthCheckDetails")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "services/{serviceName}/healthchecks")] HttpRequest req,
            string serviceName,
            ILogger log)
        {
            var results = await DBClient.GetHealthCheckResults(serviceName);

            return new OkObjectResult(results);
        }
    }
}
