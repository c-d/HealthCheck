using HealthCheck.Models;
using System.Threading.Tasks;

namespace HealthCheck.ServiceTypeCheckers
{
    interface IServiceChecker
    {
        Task<HealthCheckResult> CheckService(Service service);
    }
}
