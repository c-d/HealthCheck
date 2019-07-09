using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthCheckDataAccess;
using Microsoft.AspNetCore.Mvc;

namespace HealthCheckUI.Controllers
{
    public class ServiceController : Controller
    {
        private IHealthCheckDBClient DBClient;

        public ServiceController(IHealthCheckDBClient dbClient)
        {
            DBClient = dbClient;
        }

        [ActionName("Index")]
        public async Task<ActionResult> Index()
        {
            var services = await DBClient.GetServices();
            return View(services);
        }
    }
}