using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using HealthCheck.Models;
using Newtonsoft.Json;

namespace HealthCheck.ServiceTypeCheckers
{
    class HttpServiceChecker : IServiceChecker
    {
        private readonly HttpClient httpClient;

        public HttpServiceChecker(HttpClient httpClient)
        {
            this.httpClient = httpClient;
        }

        public async Task<HealthCheckResult> CheckService(Service service)
        {
            if (!typeof(HttpService).IsEquivalentTo(service.GetType())) {
                throw new ArgumentException("Incorrect service type.");
            }
            var httpService = (HttpService) service;

            var result = new HealthCheckResult()
            {
                ServiceName = httpService.Name,
                ServiceId = service.Id,
                // To UTC?
                TimeChecked = DateTime.Now
            };

            try
            {
                HttpResponseMessage response = null;
                using (var requestMessage = new HttpRequestMessage(HttpMethod.Get, httpService.Url))
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
    }
}
