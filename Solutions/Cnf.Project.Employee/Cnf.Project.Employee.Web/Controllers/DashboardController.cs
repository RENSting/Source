using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Cnf.Project.Employee.Web.Models;

namespace Cnf.Project.Employee.Web.Controllers
{
    public class DashboardController:Controller
    {
        IAnalysisService _analysisService;
        public DashboardController(IAnalysisService analysisService)
        {
            _analysisService = analysisService;
        }

        public async Task<IActionResult> Index()
        {
            var pivot = await _analysisService.GetProjectDistribution();
            
            return View(pivot);
        }

        public async Task<IActionResult> QualifDist()
        {
            var pivot = await _analysisService.GetOrganizationDistribution();

            return View(pivot);
        }
    }
}