using HealthCheck;
using HealthCheck.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HealthCheckDataAccess
{
    public class HealthCheckDBClient : IHealthCheckDBClient
    {
        private DocumentClient DocumentClient;

        private string DatabaseUrl;
        private string DatabaseName;
        private string DatabaseCollectionName;
        private string DatabaseKey;

        private string DatabaseCollectionLink;

        public HealthCheckDBClient(string dbUrl, string dbName, string dbCollectionName, string dbKey, string serviceConfig)
        {
            DatabaseUrl = dbUrl;
            DatabaseName = dbName;
            DatabaseCollectionName = dbCollectionName;
            DatabaseKey = dbKey;

            InitialiseDocumentClient(serviceConfig);
        }

        private void InitialiseDocumentClient(string serviceConfig)
        {
            var serializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };

            DocumentClient = new DocumentClient(new Uri(DatabaseUrl), DatabaseKey, serializerSettings);
            DatabaseCollectionLink = UriFactory.CreateDocumentCollectionUri(DatabaseName, DatabaseCollectionName).ToString();

            // Create DB if not existing
            var database = DocumentClient.CreateDatabaseIfNotExistsAsync(new Database() { Id = DatabaseName });

            // Create collection if not existing
            var collection = DocumentClient.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(DatabaseName),
                new DocumentCollection { Id = DatabaseCollectionName });

            // Populate collection with service definitions if not existing
            var httpServicesFromConfig = JsonConvert.DeserializeObject<List<HttpService>>(serviceConfig);
            foreach (var service in httpServicesFromConfig)
            {
                var docExists = DocumentClient.CreateDocumentQuery<HttpService>(DatabaseCollectionLink)
                    .Where(x => x.ServiceType == "HttpService" && x.Name == service.Name && x.Url == service.Url)
                    .AsEnumerable()
                    .Any();

                if (!docExists)
                {
                    DocumentClient.CreateDocumentAsync(DatabaseCollectionLink, service);
                }
            }
        }

        public HealthChecker CreateHealthChecker()
        {
            var services = DocumentClient.CreateDocumentQuery<HttpService>(DatabaseCollectionLink)
                .Where(x => x.ServiceType == "HttpService")
                .AsEnumerable()
                .ToList();

            return new HealthChecker(services);
        }

        public async void SaveHealthCheckResult(HealthCheckResult healthCheckResult)
        {
            //TODO: Only retain the last success + last failure for each service
            await DocumentClient.CreateDocumentAsync(DatabaseCollectionLink, healthCheckResult);
        }
    }
}
