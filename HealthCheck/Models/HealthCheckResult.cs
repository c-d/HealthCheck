using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCheck.Models
{
    public class HealthCheckResult
    {
        public string Name { get; set; }
        public bool Available { get; set; }
        public string Details { get; set; }
        public IList<HealthCheckResult> RequiredServices { get; set; }
    }
}
