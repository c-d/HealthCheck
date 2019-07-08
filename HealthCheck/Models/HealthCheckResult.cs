using System;
using System.Collections.Generic;

namespace HealthCheck.Models
{
    public class HealthCheckResult
    {
        public string Name { get; set; }
        public bool Available { get; set; }
        public string Details { get; set; }
        public DateTime TimeChecked { get; set; }
        public IList<HealthCheckResult> RequiredServices { get; set; }
    }
}
