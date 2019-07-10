using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthCheckDataAccess;
using Microsoft.AspNetCore.Mvc;

namespace HealthCheckUI.Controllers
{
    public class HealthCheckResultController : Controller
    {
        private IHealthCheckDBClient DBClient;

        public HealthCheckResultController(IHealthCheckDBClient dbClient)
        {
            DBClient = dbClient;
        }

        [ActionName("Index")]
        public async Task<ActionResult> Index()
        {
            var healthCheckResults = await DBClient.GetHealthCheckResults();
            return View(healthCheckResults);
        }

        [ActionName("Details")]
        public async Task<ActionResult> Details(string id)
        {
            var healthCheckResults = await DBClient.GetHealthCheckResults(id);
            return View("Index", healthCheckResults);
        }
    }
}