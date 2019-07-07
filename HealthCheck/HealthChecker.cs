using HealthCheck.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HealthCheck
{
    public class HealthChecker
    {
        private readonly HttpClient httpClient = new HttpClient();
        private readonly List<HttpService> httpServices;
        private readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public HealthChecker(string config)
        {
            httpServices = JsonConvert.DeserializeObject<List<HttpService>>(config);
        }

        public HealthChecker(List<HttpService> httpServices)
        {
            this.httpServices = httpServices;
        }

        public async Task<HealthCheckResult> GetHealthCheckResult(string serviceName, bool showDependencies = true, bool failuresOnly = false)
        {
            var dependencyResults = new List<HealthCheckResult>();
            foreach (var dependency in httpServices)
            {
                dependencyResults.Add(await CheckHttpService(dependency));
            }

            var allDependenciesAvailable = dependencyResults.TrueForAll(x => x.Available);
            dependencyResults = FilterDependencies(dependencyResults, showDependencies, failuresOnly);

            var healthCheckResult = new HealthCheckResult()
            {
                Name = serviceName,
                Available = allDependenciesAvailable,
                Details = allDependenciesAvailable ? "Service available" : "A required service is unavailable.",
                RequiredServices = dependencyResults
            };

            return healthCheckResult;
        }

        public async Task<HttpResponseMessage> GetHealthCheckHttpResponse(string serviceName, bool showDependencies = true, bool failuresOnly = false)
        {
            var healthCheckResult = await GetHealthCheckResult(serviceName, showDependencies, failuresOnly);
            var statusCode = healthCheckResult.Available ? HttpStatusCode.OK : HttpStatusCode.ServiceUnavailable;

            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(healthCheckResult, Formatting.None, jsonSerializerSettings), 
                    Encoding.UTF8, 
                    "application/json")
            };
        }

        private async Task<HealthCheckResult> CheckHttpService(HttpService service)
        {
            var result = new HealthCheckResult()
            {
                Name = service.Name
            };

            try
            {
                HttpResponseMessage response = null;
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, service.Url))
                {
                    response = await httpClient.SendAsync(requestMessage);
                }
                
                result.Available = response.StatusCode == HttpStatusCode.OK;
                
                if (!result.Available)
                {
                    // If we can't parse it into a HealthCheckResult, it will be treated like any other unknown exception in the catch below
                    var healthCheckResult = JsonConvert.DeserializeObject<HealthCheckResult>(await response.Content.ReadAsStringAsync());

                    result.Available = healthCheckResult.Available;
                    result.Details = healthCheckResult.Details;
                    result.RequiredServices = healthCheckResult.RequiredServices;
                }
            }
            catch (Exception)
            {
                result.Available = false;
                result.Details = "An error occurred while attempting to call the service.";
            }

            return result;
        }

        private List<HealthCheckResult> FilterDependencies(List<HealthCheckResult> dependencies, bool showDependencies, bool failuresOnly)
        {
            if (!showDependencies)
            {
                dependencies.Clear();
            }
            else if (failuresOnly) {
                dependencies.RemoveAll(x => x.Available);
            }

            return dependencies;
        }
    }
}
