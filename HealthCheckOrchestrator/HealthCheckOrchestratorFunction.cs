using System;
using System.Collections.Generic;
using System.Net.Http;
using HealthCheck;
using HealthCheck.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace HealthCheckOrchestrator
{
    public static class HealthCheckOrchestratorFunction
    {

        [FunctionName("HealthCheckOrchestratorFunction")]
        public static void Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, ILogger log, ExecutionContext context)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var config = new ConfigurationBuilder()
                .SetBasePath(context.FunctionAppDirectory)
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            var dbClient = PrepareDBClient(config);

            // Should be a singleton...
            var healthChecker = new HealthChecker(config["Services"]);
            var healthCheckResults = healthChecker.GetHealthCheckResult("Orchestrator", true, false).Result.RequiredServices;

            foreach (var healthCheckResult in healthCheckResults)
            {
                //TODO: Update stored healthcheck results for each service.
            }
        }

        private static DocumentClient PrepareDBClient(IConfigurationRoot config)
        {
            var serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var dbClient = new DocumentClient(new Uri(config["Database_Url"]), config["Database_Key"], serializerSettings);
            
            // Should really be a one-time setup... not the responsibility of the orchestrator
            // InitialiseDB(dbClient, config);

            return dbClient;
        }

        private static async void InitialiseDB(DocumentClient dbClient, IConfigurationRoot config)
        {
            var dbName = config["Database_Name"];
            var collectionName = config["Database_Collection"];

            var database = await dbClient.CreateDatabaseIfNotExistsAsync(new Database() { Id = dbName });

            var collection = await dbClient.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(dbName),
                new DocumentCollection { Id = collectionName });

            var httpServicesFromConfig = JsonConvert.DeserializeObject<List<HttpService>>(config["Services"]);
            foreach (var service in httpServicesFromConfig)
            {
                var document = await dbClient.CreateDocumentAsync(UriFactory.CreateDocumentCollectionUri(dbName, collectionName), service);
            }
        }
    }
}
