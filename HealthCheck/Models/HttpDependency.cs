using System;
using System.Collections.Generic;
using System.Text;

namespace HealthCheck.Models
{
    public class HttpDependency
    {

        public HttpDependency(string name, string url)
        {
            Url = url;
            Name = name;
        }

        public string Url { get; set; }
        public string Name { get; set; }
    }
}
