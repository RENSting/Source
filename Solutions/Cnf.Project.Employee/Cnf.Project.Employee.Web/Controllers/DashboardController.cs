using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Cnf.Project.Employee.Web.Models;
using Cnf.Project.Employee.Entity;

namespace Cnf.Project.Employee.Web.Controllers
{
    public class DashboardController : Controller
    {
        IAnalysisService _analysisService;
        IEmployeeService _employeeService;
        ISysAdminService _sysAdminService;
        public DashboardController(IAnalysisService analysisService,
                IEmployeeService employeeService, ISysAdminService sysAdminService)
        {
            _analysisService = analysisService;
            _employeeService = employeeService;
            _sysAdminService = sysAdminService;
        }

        async Task<GroupPivot> GetProjectDistribution(ProjDistViewModel model) =>
            await _analysisService.GetProjectDistribution(
                model.ActiveOnly, model.GetSearchCategories()
            );

        public async Task<IActionResult> Index()
        {
            var model = new ProjDistViewModel{
                ActiveOnly = true,
                LeadingGroupOnly = true,
            };

            model.Pivot = await GetProjectDistribution(model);
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(ProjDistViewModel model)
        {
            model.Pivot = await GetProjectDistribution(model);
            return View(model);
        }

        public async Task<IActionResult> QualifDist(int? activeonly)
        {
            bool includeInactive;
            if (activeonly.HasValue && activeonly.Value == 0)
            {
                includeInactive = true;
            }
            else
            {
                includeInactive = false;
            }
            ViewBag.IncludeInactive = includeInactive;
            var pivot = await _analysisService.GetOrganizationDistribution(includeInactive);

            return View(pivot);
        }

        public async Task<IActionResult> Resume(int id)
        {
            var employee = await _employeeService.GetEmployee(id);
            var qualifs = await _sysAdminService.GetReferences(ReferenceTypeEnum.Qualification);
            var model = new ResumeViewModel
            {
                ActiviteStatus = employee.ActiveStatus,
                CurrentDutyName = employee.DutyName,
                CurrentProjectName = employee.ProjectName,
                EmployeeID = employee.ID,
                EmployeeName = employee.Name,
                OrganizationName = (await _sysAdminService.GetOrganiztion(employee.OrganizationID)).Name,
                SpecialtyName = (await _sysAdminService.GetReference(employee.SpecialtyID)).ReferenceValue,
                Title = employee.Title,
                Certifications = (await _employeeService.GetCertifications(id))
                        .Select(c => CertViewModel.Create(c, employee, qualifs)).ToArray(),
                Logs = (from il in await _analysisService.GetStaffInLogs(id)
                        select new TransferLog
                        {
                            Date = il.InDate,
                            FromDuty = "",
                            FromProject = "",
                            LogType = TransferLogType.In,
                            TimeStamp = il.CreatedOn,
                            ToProject = il.ProjectName,
                            ToDuty = il.DutyName,
                        }).Union(
                                from ol in await _analysisService.GetStaffOutLogs(id)
                                select new TransferLog
                                {
                                    Date = ol.OutDate,
                                    FromDuty = ol.DutyName,
                                    FromProject = ol.ProjectName,
                                    LogType = TransferLogType.Out,
                                    TimeStamp = ol.CreatedOn,
                                    ToProject = "",
                                    ToDuty = "",
                                }
                            ).OrderBy(l=>l.Date).ThenBy(l => l.TimeStamp).ToArray(),
            };

            return View(model);
        }
    }
}