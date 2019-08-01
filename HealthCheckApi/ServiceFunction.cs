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
    public class ServiceFunction
    {
        private readonly IHealthCheckDBClient DBClient;
        private readonly IHealthChecker HealthChecker;

        public ServiceFunction(
            IHealthCheckDBClient dbClient,
            IHealthChecker healthChecker)
        {
            DBClient = dbClient;
            HealthChecker = healthChecker;
        }

        [FunctionName("GetServices")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "services/{serviceName?}")] HttpRequest req,
            string serviceName,
            ILogger log)
        {
            var includeFullDetails = req.Query.ContainsKey("expanded");

            var results = await DBClient.GetServices(includeFullDetails, serviceName);

            return new OkObjectResult(results);
        }
    }
}
