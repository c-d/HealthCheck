using HealthCheck;
using HealthCheck.Models;
using Microsoft.Azure.Documents;
using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCheckDataAccess
{
    public interface IHealthCheckDBClient
    {
        HealthChecker CreateHealthChecker();
        void SaveHealthCheckResult(HealthCheckResult healthCheckResult);
    }
}
