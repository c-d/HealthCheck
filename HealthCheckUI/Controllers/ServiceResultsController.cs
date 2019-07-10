using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthCheckDataAccess;
using HealthCheckUI.Models;
using Microsoft.AspNetCore.Mvc;

namespace HealthCheckUI.Controllers
{
    public class ServiceResultsController : Controller
    {
        private IHealthCheckDBClient DBClient;

        public ServiceResultsController(IHealthCheckDBClient dbClient)
        {
            DBClient = dbClient;
        }

        [ActionName("Index")]
        public async Task<ActionResult> Index()
        {
            var serviceResults = new List<ServiceResults>();

            var services = await DBClient.GetServices();
            foreach (var service in services)
            {
                var healthCheckResults = (await DBClient.GetHealthCheckResults(service.Id)).ToList();
                var serviceResultsItem = new ServiceResults()
                {
                    HttpService = service,
                    HealthCheckResults = healthCheckResults,
                    Status = healthCheckResults.First().Available ? "Available" : "Unavailable"
                };
                serviceResults.Add(serviceResultsItem);
            }
            return View(serviceResults);
        }
    }
}