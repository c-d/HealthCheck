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
        public async Task<IActionResult> Index()
        {
            var healthCheckResults = await DBClient.GetHealthCheckResults();
            return View(healthCheckResults);
        }
    }
}