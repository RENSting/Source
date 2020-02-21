using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cnf.Project.Employee.Entity;

namespace Cnf.Project.Employee.Web.Controllers
{
    [Authorize]
    public class SystemController : Controller
    {
        public SystemController()
        {
            ;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Organizations()
        {
            List<Organization> organizations = new List<Organization>()
            {
                new Organization
                {
                    ID=1,Name="上海分公司",ActiveStatus=true,CreatedBy=1
                },
                new Organization
                {
                    ID=2,Name="广东分公司",ActiveStatus=true,CreatedBy=1
                },
                new Organization
                {
                    ID=3,Name="江苏分公司",ActiveStatus=true,CreatedBy=1
                }
            };

            return View(organizations);
        }
    }
}