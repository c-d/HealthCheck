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
        Task<IEnumerable<HttpService>> GetServices(bool fullDetails, string serviceName = null);
        Task<IEnumerable<HealthCheckResult>> GetHealthCheckResults(string serviceName = null);
    }
}
