using HealthCheck;
using HealthCheck.Models;
using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace HealthCheckDataAccess
{
    public interface IHealthCheckDBClient
    {
        HealthChecker CreateHealthChecker();
        void SaveHealthCheckResult(HealthCheckResult healthCheckResult);
        Task<IEnumerable<HttpService>> GetServices();
        Task<IEnumerable<HealthCheckResult>> GetHealthCheckResults(string serviceId = null);
    }
}
