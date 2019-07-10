using HealthCheck.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HealthCheckUI.Models
{
    public class ServiceResults
    {
        public HttpService HttpService { get; set; } 
        public List<HealthCheckResult> HealthCheckResults { get; set; }
        public string Status { get; set; }
    }
}
