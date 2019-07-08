using HealthCheck.Models;
using HealthCheck.ServiceTypeCheckers;
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
        private readonly HttpServiceChecker httpServiceChecker;
        private readonly List<HttpService> httpServices;
        private readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };

        public HealthChecker(string config)
        {
            // Some additional work to be done to create different service checkers and 
            // provide a mapping between services and service checkers.
            httpServices = JsonConvert.DeserializeObject<List<HttpService>>(config);
            httpServiceChecker = new HttpServiceChecker(new HttpClient());
        }

        public HealthChecker(List<HttpService> httpServices)
        {
            this.httpServices = httpServices;
            httpServiceChecker = new HttpServiceChecker(new HttpClient());
        }

        public async Task<HealthCheckResult> GetHealthCheckResult(string serviceName, bool showDependencies = true, bool failuresOnly = false)
        {
            var dependencyResults = new List<HealthCheckResult>();
            foreach (var service in httpServices)
            {
                dependencyResults.Add(await httpServiceChecker.CheckService(service));
            }

            var allDependenciesAvailable = dependencyResults.TrueForAll(x => x.Available);
            dependencyResults = FilterDependencies(dependencyResults, showDependencies, failuresOnly);

            var healthCheckResult = new HealthCheckResult()
            {
                Name = serviceName,
                TimeChecked = DateTime.Now,
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
