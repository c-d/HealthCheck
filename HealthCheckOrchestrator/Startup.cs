using HealthCheck;
using HealthCheck.Models;
using HealthCheckOrchestrator;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: WebJobsStartup(typeof(Startup))]
namespace HealthCheckOrchestrator
{
    public class Startup : IWebJobsStartup
    {
        
        public void Configure(IWebJobsBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            

            var dbClient = PrepareDBClient(config).Result;            
            var healthChecker = SetupHealthChecker(dbClient, config);

            builder.Services.AddSingleton<IDocumentClient>(x => dbClient);
            builder.Services.AddSingleton<IHealthChecker>(x => healthChecker);
            builder.Services.AddScoped(x => config);
        }

        private async Task<DocumentClient> PrepareDBClient(IConfigurationRoot config)
        {
            var serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            var dbClient = new DocumentClient(new Uri(config["Database_Url"]), config["Database_Key"], serializerSettings);
            config["Database_Collection_Link"] = UriFactory.CreateDocumentCollectionUri(config["Database_Name"], config["Database_Collection"]).ToString();

            var database = await dbClient.CreateDatabaseIfNotExistsAsync(new Database() { Id = config["Database_Name"] });

            var collection = await dbClient.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(config["Database_Name"]),
                new DocumentCollection { Id = config["Database_Collection"] });

            var httpServicesFromConfig = JsonConvert.DeserializeObject<List<HttpService>>(config["Services"]);
            foreach (var service in httpServicesFromConfig)
            {
                var docExists = dbClient.CreateDocumentQuery<HttpService>(config["Database_Collection_Link"])
                    .Where(x => x.ServiceType == "HttpService" && x.Name == service.Name && x.Url == service.Url)
                    .AsEnumerable()
                    .Any();

                if (!docExists)
                {
                    await dbClient.CreateDocumentAsync(config["Database_Collection_Link"], service);
                }
            }

            return dbClient;
        }

        private static HealthChecker SetupHealthChecker(DocumentClient dbClient, IConfigurationRoot config)
        {
            // By now the DB should have been populated with dependency definitions
            // Read those out (plus any other services defined in the DB) to create the healthchecker 
            var services = dbClient.CreateDocumentQuery<HttpService>(config["Database_Collection_Link"])
                .Where(x => x.ServiceType == "HttpService")
                .AsEnumerable()
                .ToList();

          return new HealthChecker(services);
        }
    }
}
