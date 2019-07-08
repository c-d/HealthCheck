using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace HealthCheck.Models
{
    public class HealthCheckResult
    {

        [JsonProperty("serviceName")]
        public string ServiceName { get; set; }

        [JsonProperty("serviceId")]
        public string ServiceId { get; set; }

        [JsonProperty("available")]
        public bool Available { get; set; }

        [JsonProperty("details")]
        public string Details { get; set; }

        [JsonProperty("timeChecked")]
        public DateTime TimeChecked { get; set; }

        [JsonProperty("requiredServices")]
        public IList<HealthCheckResult> RequiredServices { get; set; }
    }
}
