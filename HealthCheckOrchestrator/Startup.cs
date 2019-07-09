using HealthCheck;
using HealthCheck.Models;
using HealthCheckDataAccess;
using HealthCheckOrchestrator;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host.Config;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[assembly: WebJobsStartup(typeof(Startup))]
namespace HealthCheckOrchestrator
{
    public class Startup : IWebJobsStartup
    {
        
        public void Configure(IWebJobsBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();
            
            var healthCheckDBClient = new HealthCheckDBClient(
                    config["Database_Url"],
                    config["Database_Name"],
                    config["Database_Collection"],
                    config["Database_Key"],
                    config["Services"]);

            builder.Services.AddSingleton<IHealthCheckDBClient>(
                x => healthCheckDBClient);
            builder.Services.AddScoped<IHealthChecker>(x => healthCheckDBClient.CreateHealthChecker());
        }
    }
}
