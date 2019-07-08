using HealthCheck.Models;
using System.Net.Http;
using System.Threading.Tasks;

namespace HealthCheck
{
    public interface IHealthChecker
    {
        Task<HealthCheckResult> GetHealthCheckResult(string serviceName, bool showDependencies = true, bool failuresOnly = false);
        Task<HttpResponseMessage> GetHealthCheckHttpResponse(string serviceName, bool showDependencies = true, bool failuresOnly = false);
    }
}
