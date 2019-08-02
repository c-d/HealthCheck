using HealthCheck;
using HealthCheck.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheckDataAccess
{
    public class HealthCheckDBClient : IHealthCheckDBClient
    {
        private DocumentClient DocumentClient;

        private readonly string DatabaseUrl;
        private string DatabaseName;
        private string DatabaseCollectionName;
        private readonly string DatabaseKey;

        private string DatabaseCollectionLink;
        private readonly int HealthCheckLimitDefault = 100;

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
            var database = DocumentClient.CreateDatabaseIfNotExistsAsync(new Database() { Id = DatabaseName }).Result;

            // Create collection if not existing
            var collection = DocumentClient.CreateDocumentCollectionIfNotExistsAsync(
                UriFactory.CreateDatabaseUri(DatabaseName),
                new DocumentCollection { Id = DatabaseCollectionName }).Result;

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

        // TODO: Refactor this, re-use GetServices()
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

        public async Task<IEnumerable<HealthCheckResult>> GetHealthCheckResults(string serviceName = null)
        {
            return await GetHealthCheckResults(HealthCheckLimitDefault, serviceName);
        }

        public async Task<IEnumerable<HealthCheckResult>> GetHealthCheckResults(int limit, string serviceName = null)
        {
            var sqlQuery = "SELECT TOP @limit * FROM s WHERE (s.serviceId != null)";
            var sqlParams = new SqlParameterCollection
            {
                new SqlParameter("@limit", limit)
            };

            if (serviceName != null)
            {
                sqlQuery += " AND (s.serviceName = @name)";
                sqlParams.Add(new SqlParameter("@name", serviceName));
            }
            sqlQuery += " ORDER BY s.timeChecked DESC";

            var querySpec = new SqlQuerySpec(sqlQuery, sqlParams);            
            var healthCheckResults = Task.Run(() => DocumentClient
                .CreateDocumentQuery<HealthCheckResult>(DatabaseCollectionLink, querySpec)
                .AsEnumerable()
                .ToList());

            return await healthCheckResults;
        }

        public async Task<IEnumerable<HttpService>> GetServices(bool fullDetails, string serviceName = null)
        {
            var sqlQuery = "SELECT * FROM s WHERE (s.serviceType = 'HttpService')";
            var sqlParams = new SqlParameterCollection();
            
            if (serviceName != null)
            {
                sqlQuery += " AND (s.name = @name)";
                sqlParams.Add(new SqlParameter("@name", serviceName));
            }

            var querySpec = new SqlQuerySpec(sqlQuery, sqlParams);
            var services = DocumentClient
                .CreateDocumentQuery<HttpService>(DatabaseCollectionLink, querySpec)
                .AsEnumerable()
                .ToList();

            foreach (var service in services)
            {
                var lastHealthCheckResult = (await GetHealthCheckResults(service.Name)).First();
                service.Status = lastHealthCheckResult.Available ? "Available" : lastHealthCheckResult.Details;
                // Also include
                // - time since last available
                // - time since last offline
            }

            return services;
        }
    }
}
