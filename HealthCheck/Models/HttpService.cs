using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCheck.Models
{
    public class HttpService
    {
        public string Type { get => "HttpService"; }
        //public string Id { get => $"{Name}-{Url}"; }

        public string Url { get; set; }
        public string Name { get; set; }
    }
}
