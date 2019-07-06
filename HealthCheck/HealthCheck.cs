using HealthCheck.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HealthCheck
{
    public class HealthCheck

    {
        private readonly HttpClient httpClient;
        private readonly JsonSerializerSettings jsonSerializerSettings;
        private static List<HttpDependency> dependencies;

        public HealthCheck(HttpClient httpClient, string config)
        {
            this.httpClient = httpClient;
            jsonSerializerSettings = new JsonSerializerSettings()
             {
                 NullValueHandling = NullValueHandling.Ignore
             };

            dependencies = JsonConvert.DeserializeObject<List<HttpDependency>>(config);
        }

        public async Task<HttpResponseMessage> PerformHealthCheck(string serviceName, bool showDependencies = true, bool failuresOnly = false)
        {
            var dependencyResults = new List<HealthCheckResult>();
            foreach (var dependency in dependencies)
            {
                dependencyResults.Add(await CheckHttpDependency(dependency));
            }

            var allDependenciesAvailable = dependencyResults.TrueForAll(x => x.Available);
            var statusCode = allDependenciesAvailable ? HttpStatusCode.OK : HttpStatusCode.ServiceUnavailable;
            dependencyResults = FilterDependencies(dependencyResults, showDependencies, failuresOnly);

            var healthCheckResult = new HealthCheckResult()
            {
                Name = serviceName,
                Available = allDependenciesAvailable,
                Details = allDependenciesAvailable ? "Service available" : "A required service is unavailable.",
                Dependencies = dependencyResults
            };

            return new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(
                    JsonConvert.SerializeObject(healthCheckResult, Formatting.None, jsonSerializerSettings), 
                    Encoding.UTF8, 
                    "application/json")
            };
        }

        private async Task<HealthCheckResult> CheckHttpDependency(HttpDependency dependency)
        {
            var result = new HealthCheckResult()
            {
                Name = dependency.Name
            };

            try
            {
                HttpResponseMessage response = null;
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, dependency.Url))
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
                    result.Dependencies = healthCheckResult.Dependencies;
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
