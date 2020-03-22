using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Cnf.Project.Employee.Web.Models;
using Cnf.Project.Employee.Entity;

namespace Cnf.Project.Employee.Web.Controllers
{
    public class ProjectController : Controller
    {
        readonly IEmployeeService _employeeService;
        readonly ISysAdminService _sysAdminService;
        readonly IProjectService _projectService;

        public ProjectController(IEmployeeService employeeService,
            ISysAdminService sysAdminService, IProjectService projectService)
        {
            _employeeService = employeeService;
            _sysAdminService = sysAdminService;
            _projectService = projectService;
        }

        public async Task<IActionResult> Index(ProjectState? selectedState)
        {
            var projects = await _projectService.GetProjectByStatus(selectedState);
            var model = ProjectListViewModel.Create(projects);
            model.SelectedState = selectedState;
            return View(model);
        }

        public async Task<IActionResult> Edit(int? id, string err)
        {
            ProjectViewState model;
            if (id.HasValue)
            {
                model = await _projectService.GetProjectById(id.Value);
            }
            else
            {
                model = new ProjectViewState
                {
                    ActiveStatus = true,
                    BeginDate = DateTime.Today,
                    EndDate = DateTime.Today.AddYears(1),
                    State = ProjectState.Preparing
                };
            }
            if (!string.IsNullOrWhiteSpace(err))
                ModelState.AddModelError("", err);

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProjectViewState model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    Entity.Project project;
                    if (model.ProjectId > 0)
                    {
                        project = await _projectService.GetProjectById(model.ProjectId);
                        if (project == null || project.ID <= 0)
                            throw new Exception("没有找到要保存的项目");
                    }
                    else
                    {
                        project = new Entity.Project
                        {
                            CreatedBy = UserHelper.GetUserID(HttpContext),
                            CreatedOn = DateTime.Now,
                        };
                    }
                    project.ActiveStatus = model.ActiveStatus;
                    project.ContractCode = model.ContractCode;
                    project.EndTime = model.EndDate;
                    project.FullName = model.FullName;
                    project.Owner = model.Owner;
                    project.ShortName = model.ShortName;
                    project.SitePlace = model.SitePlace;
                    project.StartTime = model.BeginDate;
                    project.Status = (int)model.State;

                    await _projectService.SaveProject(project);
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(ProjectViewState model)
        {
            try
            {
                //check employees
                if ((await _employeeService.GetEmployeesInProject(model.ProjectId, true))?.Length > 0)
                    throw new Exception("项目有人员不能删除");
                //check in log
                if ((await _projectService.GetTransferInLogs(null, model.ProjectId, true))?.Length > 0)
                    throw new Exception("项目已经存在调入日志");
                //check out log
                if ((await _projectService.GetTransferOutLogs(null, model.ProjectId, true))?.Length > 0)
                    throw new Exception("项目已经存在调出记录");

                await _projectService.Delete(model.ProjectId);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Edit), new { id = model.ProjectId, err = ex.Message });
            }
        }

        // the result is type of EmployeeQualifState
        public async Task<JsonResult> GetQualifyState(int employeeId, int? dutyId)
        {
            if (!dutyId.HasValue)
                dutyId = (await _employeeService.GetEmployee(employeeId)).InDutyID;

            var dutyQualifications = await _projectService.GetDutyQualifications(dutyId.Value);
            var certifications = await _employeeService.GetCertifications(employeeId);
            EmployeeQualifState state = ProjEmployeeViewModel.EvalQualif(
                employeeId, dutyId.Value, dutyQualifications, certifications, DateTime.Today);
            return new JsonResult(state);
        }

        // the result is DutyQualification array

        public async Task<JsonResult> GetDutyQualifications(int dutyId)
        {
            return new JsonResult(await _projectService.GetDutyQualifications(dutyId));
        }

        // the result is type of Reference[]
        public async Task<JsonResult> GetCertsOfEmployee(int employeeId) =>
            new JsonResult(await _employeeService.GetCertifications(employeeId));

        public async Task<IActionResult> ListEmployees(int? id, string msg)
        {
            if (!id.HasValue)
                throw new Exception("必须传递项目的唯一ID");

            var project = await _projectService.GetProjectById(id.Value);
            var employees = await _employeeService.GetEmployeesInProject(id.Value, false);
            var specialities = await _sysAdminService.GetReferences(ReferenceTypeEnum.Specialty);
            //var duties = await _sysAdminService.GetReferences(ReferenceTypeEnum.Duty);
            //var qualifs = await _sysAdminService.GetReferences(ReferenceTypeEnum.Qualification);

            ViewBag.ProjectName = project.FullName;
            ViewBag.ProjectId = id;
            var model = (from e in employees
                         select ProjEmployeeViewModel.Create(e, specialities)).ToArray();
            if (!string.IsNullOrWhiteSpace(msg))
            {
                ModelState.AddModelError("", msg);
            }
            return View(model);
        }

        /// <summary>
        /// 处理视图模型，包括ViewBag
        /// </summary>
        /// <param name="originModel"></param>
        /// <returns></returns>
        async Task<RecruitViewModel> ProcessRecruitViewModel(RecruitViewModel originModel)
        {
            var orgs = await _sysAdminService.GetOrganiztions();
            var specs = await _sysAdminService.GetReferences(ReferenceTypeEnum.Specialty);
            var duties = await _sysAdminService.GetReferences(ReferenceTypeEnum.Duty);
            ViewBag.OrgList = new SelectList(orgs, nameof(Organization.ID), nameof(Organization.Name));
            ViewBag.SpecList = new SelectList(specs, nameof(Reference.ID), nameof(Reference.ReferenceValue));
            ViewBag.DutyList = new SelectList(duties, nameof(Reference.ID), nameof(Reference.ReferenceValue));
            var result = await _employeeService.SearchEmployee(originModel.GetCriteria());
            originModel.Total = result.Total;
            originModel.Candidates = result.Records.Select(e => ProjEmployeeViewModel.Create(e, specs)).ToArray();
            return originModel;
        }

        /// <summary>
        /// 向项目中添加人员
        /// </summary>
        /// <param name="id">项目的ProjectID</param>
        /// <returns></returns>
        public async Task<IActionResult> Recruit(int? id)
        {
            if (!id.HasValue) throw new Exception("必须指定要加入的项目");
            var project = await _projectService.GetProjectById(id.Value);
            var originModel = new RecruitViewModel
            {
                ProjectId = project.ID,
                ProjectName = project.FullName,
                PageIndex = 0,
                PageSize = 20
            };
            return View(await ProcessRecruitViewModel(originModel));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Recruit(RecruitViewModel model)
        {
            return View(await ProcessRecruitViewModel(model));
        }

        public async Task<IActionResult> VerifyTransfer(int employeeId, int projectId, int dutyId)
        {
            return Content(await _projectService.VerifyTransfer(employeeId, projectId, dutyId));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RecruitConfirm(RecruitViewModel model, string from)
        {
            var transferList = new List<TransferViewModel>();
            foreach (var candidate in model.Candidates)
            {
                if (candidate.Selected)
                {
                    transferList.Add(new TransferViewModel
                    {
                        ProjectId = model.ProjectId,
                        ProjectName = model.ProjectName,
                        DutyId = candidate.RecruitDutyId,
                        EmployeeId = candidate.EmployeeId,
                        EmployeeName = candidate.EmployeeName,
                        SpecialityName = candidate.SpecialityName,
                    });
                }
            }
            ViewBag.DutyList = new SelectList(
                await _sysAdminService.GetReferences(ReferenceTypeEnum.Duty),
                nameof(Reference.ID), nameof(Reference.ReferenceValue));
            ViewBag.From = from;
            return View(transferList);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RecuitConfirmed(List<TransferViewModel> model)
        {
            int errCount = 0, succCount = 0;
            int projectId = 0;
            foreach (var item in model)
            {
                projectId = item.ProjectId;
                try
                {
                    await _projectService.Transfer(item.EmployeeId, item.ProjectId,
                        item.DutyId, UserHelper.GetUserID(HttpContext));
                    succCount++;
                }
                catch
                {
                    errCount++;
                }
            }

            return RedirectToAction(nameof(ListEmployees),
                new
                {
                    id = projectId,
                    msg = $"共处理了{errCount + succCount}, 成功了{succCount}人， 失败了{errCount}人",
                });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult RecuitCancelled(List<TransferViewModel> model)
        {
            if (model.Count > 0)
            {
                return RedirectToAction(nameof(ListEmployees), new { id = model[0].ProjectId });
            }
            else
            {
                throw new Exception("没有发送需要的参数");
            }
        }

        public async Task<IActionResult> Withdraw(int id, int fromProjectId)
        {
            try
            {
                await _projectService.Transfer(id, 0, 0, UserHelper.GetUserID(HttpContext));
                return RedirectToAction(nameof(ListEmployees), new { id = fromProjectId });
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(ListEmployees), new
                {
                    id = fromProjectId,
                    msg = ex.Message
                });
            }
        }

        public async Task<IActionResult> Require(string msg)
        {
            ViewBag.Duties = await _sysAdminService.GetReferences(ReferenceTypeEnum.Duty);
            ViewBag.Qualifs = await _sysAdminService.GetReferences(ReferenceTypeEnum.Qualification);
            ViewBag.Message = msg;
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Require(DutyQualifViewModel model)
        {
            string msg;
            try
            {
                await _projectService.UpdateRequiredQualifcations(model);
                msg = "ok";
            }
            catch (Exception ex)
            {
                msg = "错误：" + ex.Message;
            }
            return RedirectToAction(nameof(Require), new { msg = msg });
        }

        public async Task<JsonResult> SearchProjects(ProjectState? projectState, string searchName, int pageIndex)
        {
            var pageSize = 8;
            var result = await _projectService.SearchProject(projectState, searchName, pageIndex, pageSize);
            return new JsonResult(new
            {
                total = result.Total,
                projects = result.Records,
                pageCount = result.Total == 0 ? 1
                        : (result.Total % pageSize == 0 ? result.Total / pageSize
                            : result.Total / pageSize + 1)
            });
        }

        public async Task<IActionResult> Dispatch()
        {
            var originModel = new RecruitViewModel
            {
                PageIndex = 0,
                PageSize = 20,
            };
            return View(await ProcessRecruitViewModel(originModel));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Dispatch(RecruitViewModel model)
        {
            return View(await ProcessRecruitViewModel(model));
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Withdraw(RecruitViewModel model)
        {
            foreach (var staff in model.Candidates)
            {
                await _projectService.Transfer(staff.EmployeeId, 0, 0, UserHelper.GetUserID(HttpContext));
            }
            return RedirectToAction(nameof(Dispatch));
        }
    }
}