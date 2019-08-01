using HealthCheck;
using HealthCheckApi;
using HealthCheckDataAccess;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;

[assembly: WebJobsStartup(typeof(Startup))]
namespace HealthCheckApi
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
