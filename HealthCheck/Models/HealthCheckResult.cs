using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace HealthCheck.Models
{
    public class HealthCheckResult
    {
        [JsonProperty("name")]
        public string Name { get; set; }
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
