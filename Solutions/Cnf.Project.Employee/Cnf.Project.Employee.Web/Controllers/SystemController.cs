using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cnf.Project.Employee.Entity;
using Cnf.Project.Employee.Web.Models;
using Microsoft.Extensions.Options;

namespace Cnf.Project.Employee.Web.Controllers
{
    [Authorize]
    public class SystemController : Controller
    {
        readonly ISysAdminService _sysAdminSvc;
        readonly IUserManager _userManager;
        readonly IProjectService _projectService;
        readonly IEmployeeService _employeeService;

        readonly Dictionary<string, ExcelMap> _excelMappings;

        public SystemController(ISysAdminService sysAdminService,
                IUserManager userManager, IProjectService projectService,
                IEmployeeService employeeService, IOptionsSnapshot<List<ExcelMap>> options)
        {
            _sysAdminSvc = sysAdminService;
            _userManager = userManager;
            _projectService = projectService;
            _employeeService = employeeService;
            if (options != null)
            {
                _excelMappings = new Dictionary<string, ExcelMap>();
                foreach (var map in options.Value)
                {
                    if (!_excelMappings.ContainsKey(map.EntityName))
                        _excelMappings.Add(map.EntityName, map);
                }
            }
        }

        public IActionResult Index()
        {
            var model = new FileUploadViewModel();
            return View(model);
        }

        bool checkStaffFile(FileUploadViewModel model)
        {
            if (model.StaffFile == null || !model.StaffFile.FileName.EndsWith(
                ".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                model.ResultMessage = "没有上传人员文件，或者文件不是Excel2007格式的(.xlsx)";
                return false;
            }
            if (_excelMappings == null || !_excelMappings.ContainsKey(typeof(Entity.Employee).FullName))
            {
                model.ResultMessage = "尚未配置人员导入的Excel文件映射";
                return false;
            }
            return true;
        }

        bool checkProjectFile(FileUploadViewModel model)
        {
            if (model.ProjectFile == null || !model.ProjectFile.FileName.EndsWith(
                ".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                model.ResultMessage = "没有上传项目文件，或者文件不是Excel2007格式的(.xlsx)";
                return false;
            }
            if (_excelMappings == null || !_excelMappings.ContainsKey(typeof(Entity.Project).FullName))
            {
                model.ResultMessage = "尚未配置项目导入的Excel文件映射";
                return false;
            }
            return true;
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Index(FileUploadViewModel model, string uploadType)
        {
            string mime = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            if (uploadType.Equals("staff", StringComparison.OrdinalIgnoreCase))
            {
                if (checkStaffFile(model) == false)
                {
                    ModelState.AddModelError(nameof(FileUploadViewModel.StaffFile), model.ResultMessage);
                }
                else
                {
                    try
                    {
                        var result = await FileUploadHelper.Upload(
                            model.StaffFile.OpenReadStream(),
                            _excelMappings[typeof(Entity.Employee).FullName], 
                            employeeService: _employeeService);
                        model.ShowResult = true;
                        model.ResultMessage = result;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", ex.Message);
                    }
                }

            }
            else if (uploadType.Equals("project", StringComparison.OrdinalIgnoreCase))
            {
                if (checkProjectFile(model) == false)
                {
                    ModelState.AddModelError(nameof(FileUploadViewModel.ProjectFile), model.ResultMessage);
                }
                else
                {
                    try
                    {
                        var result = await FileUploadHelper.Upload(
                            model.ProjectFile.OpenReadStream(),
                            _excelMappings[typeof(Entity.Project).FullName], 
                            projectService: _projectService);
                        model.ShowResult = true;
                        model.ResultMessage = result;
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", ex.Message);
                    }
                }
            }
            else if (uploadType.Equals("dl_staff", StringComparison.OrdinalIgnoreCase))
            {
                //下载人员模板文件
                var mapKey = typeof(Entity.Employee).FullName;
                if (_excelMappings != null && _excelMappings.ContainsKey(mapKey))
                {
                    var map = _excelMappings[mapKey];
                    using var stream = new MemoryStream();
                    FileUploadHelper.WriteExcelTemplate(stream, map);
                    return File(stream.ToArray(), mime, "staff_to_import.xlsx");
                }
                ModelState.AddModelError("", "尚未配置人员导入的Excel文件映射");
            }
            else if (uploadType.Equals("dl_project", StringComparison.OrdinalIgnoreCase))
            {
                //下载项目模板文件
                var mapKey = typeof(Entity.Project).FullName;
                if (_excelMappings != null && _excelMappings.ContainsKey(mapKey))
                {
                    var map = _excelMappings[mapKey];
                    using var stream = new MemoryStream();
                    FileUploadHelper.WriteExcelTemplate(stream, map);
                    return File(stream.ToArray(), mime, "project_to_import.xlsx");
                }
                ModelState.AddModelError("", "尚未配置项目导入的Excel文件映射");
            }
            else if(uploadType.Equals("dl_reference", StringComparison.OrdinalIgnoreCase))
            {
                //下载组织单位和专业清单（一个Excel文件）
                using var stream = new MemoryStream();
                await FileUploadHelper.WriteExcelReference(stream, _sysAdminSvc);
                return File(stream.ToArray(), mime, "org_spec_list.xlsx");
            }
            else
            {
                ModelState.AddModelError("", "页面回发的姿势不对");
            }
            return View(model);
        }

        [HttpGet]
        public JsonResult GetEntityMap(string name)
        {
            if (name.Equals("project", StringComparison.OrdinalIgnoreCase))
            {
                return Json(FileUploadHelper.MapEntity(typeof(Entity.Project),
                    new string[] { nameof(Entity.Project.FullName) }));
            }
            else if (name.Equals("staff", StringComparison.OrdinalIgnoreCase))
            {
                return Json(FileUploadHelper.MapEntity(typeof(Entity.Employee),
                    new string[] { nameof(Entity.Employee.IdNumber) }));
            }
            else
            {
                return Json("Wrong Name.");
            }
        }

        #region organization management

        [HttpGet]
        public IActionResult EditOrganization()
        {
            OrgViewModel model = new OrgViewModel
            {
                OrgId = 0,
                ActiveStatus = true
            };
            return View(model);
        }

        [HttpGet("{controller}/{action}/{id}")]
        public async Task<IActionResult> EditOrganization(int id)
        {
            OrgViewModel model = await _sysAdminSvc.GetOrganiztion(id);
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteOrganization([FromForm] OrgViewModel model)
        {
            if (model.ActiveStatus == true)
            {
                ModelState.AddModelError("", "正在使用的组织不可以删除，请首先设置它为无效");
            }
            else
            {
                if (await _sysAdminSvc.DeleteOrganization(model.OrgId) == true)
                {
                    return RedirectToAction(nameof(Organizations));
                }
                ModelState.AddModelError("", "删除单位出现错误，没有成功。");
            }
            return View("EditOrganization", model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditOrganization([FromForm] OrgViewModel model)
        {
            if (ModelState.IsValid)
            {
                Organization organization;
                if (model.OrgId > 0)
                {
                    organization = await _sysAdminSvc.GetOrganiztion(model.OrgId);
                }
                else
                {
                    organization = new Organization();
                    organization.CreatedBy = UserHelper.GetUserID(HttpContext);
                    organization.CreatedOn = DateTime.Now;
                }

                organization.Name = model.OrgName;
                organization.ActiveStatus = model.ActiveStatus;

                if (await _sysAdminSvc.SaveOrganization(organization) == true)
                {
                    return RedirectToAction(nameof(Organizations));
                }
                else
                {
                    ModelState.AddModelError("", "向数据库提交保存单位出现错误");
                }
            }
            return View(model);
        }

        public async Task<IActionResult> Organizations()
        {
            Organization[] organizations = await _sysAdminSvc.GetOrganiztions();
            return View(organizations);
        }

        #endregion

        #region user management

        [HttpGet]
        public async Task<IActionResult> Users()
        {
            User[] users = await _userManager.GetUsers();
            return View(users);
        }

        [HttpGet]
        public IActionResult EditUser()
        {
            UserViewModel model = new UserViewModel
            {
                ActiveStatus = true
            };

            return View(model);
        }

        [HttpGet("{controller}/EditUser/{id}")]
        public async Task<IActionResult> EditUser(int id)
        {
            UserViewModel model = await _userManager.GetUser(id);
            return View(model);
        }

        int GetRole(UserViewModel model)
        {
            RoleEnum role = 0;
            if (model.IsSysAdmin) role |= RoleEnum.SystemAdmin;
            if (model.IsHumanResourceAdmin) role |= RoleEnum.HumanResourceAdmin;
            if (model.IsProjectAdmin) role |= RoleEnum.ProjectAdmin;
            if (model.IsManager) role |= RoleEnum.Manager;
            return (int)role;
        }

        public async Task<IActionResult> ValidateLogin(string Login, int UserId)
        {
            if (string.IsNullOrWhiteSpace(Login))
            {
                return new JsonResult(true);
            }
            var user = await _userManager.GetUserByLogin(Login);
            if (user == null || user.UserID <= 0)
            {
                return new JsonResult(true);
            }
            if (user.UserID == UserId)
            {
                return new JsonResult(true);
            }
            return new JsonResult(false);
        }

        [HttpPost]
        [ActionName("EditUser")]
        public async Task<IActionResult> EditUser(UserViewModel model)
        {
            if (ModelState.IsValid)
            {
                User user;
                if (model.UserId > 0)
                {
                    user = await _userManager.GetUser(model.UserId);
                    if (user == null || user.UserID <= 0)
                    {
                        ModelState.AddModelError("", "编辑的用户已经被删除，请返回用户列表");
                        return View(model);
                    }
                }
                else
                {
                    user = new User();
                }

                user.ActiveStatus = model.ActiveStatus;
                user.Login = model.Login;
                user.Name = model.Name;
                user.Role = GetRole(model);

                if (await _userManager.SaveUser(user) == true)
                {
                    return RedirectToAction(nameof(Users));
                }

                ModelState.AddModelError("", "保存用户到数据库出错");
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var model = (UserViewModel)await _userManager.GetUser(id);
            return View(model);
        }

        [HttpPost]
        [ActionName("DeleteUser")]
        public async Task<IActionResult> DeleteUser(UserViewModel model)
        {
            if (model.Login.Equals("admin", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("", "不能删除admin用户");
            }
            else if (model.UserId == UserHelper.GetUserID(HttpContext))
            {
                ModelState.AddModelError("", "不能删除当前登录的用户");
            }
            else if (model.ActiveStatus == true)
            {
                ModelState.AddModelError("", "不可以删除有效状态的用户");
            }
            else
            {
                if (await _userManager.DeleteUser(model.UserId) == true)
                {
                    return RedirectToAction(nameof(Users));
                }
                else
                {
                    ModelState.AddModelError("", "删除用户出现错误");
                }
            }
            return View(model);
        }

        #endregion

        #region 参照项管理

        public async Task<IActionResult> References(ReferenceTypeEnum t)
        {
            ViewBag.ReferenceType = t;
            var model = await _sysAdminSvc.GetReferences(t);
            return View(model);
        }

        public IActionResult EditReference(ReferenceTypeEnum t)
        {
            if (t == ReferenceTypeEnum.Duty)
            {
                return RedirectToAction(nameof(EditDuty));
            }

            RefViewMode model = new RefViewMode
            {
                ActiveStatus = true,
                Type = RefViewMode.Translate(t)
            };

            return View(model);
        }

        public IActionResult EditDuty()
        {
            var duty = new DutyViewMode
            {
                Category = DutyCategoryEnum.LeadingGroup,
                IsActive = true,
            };
            return View(duty);
        }

        [HttpGet("{controller}/{action}/{id}")]
        public async Task<IActionResult> EditReference(int id)
        {
            var reference = await _sysAdminSvc.GetReference(id);
            if (reference.Type == ReferenceTypeEnum.Duty)
            {
                return RedirectToAction(nameof(EditDuty), new { id = id });
            }

            RefViewMode model = await _sysAdminSvc.GetReference(id);
            if (model == null || model.ID <= 0)
            {
                ModelState.AddModelError("", "没有定位到要显示的参照项");
            }

            return View(model);
        }

        [HttpGet("{controller}/{action}/{id}")]
        public async Task<IActionResult> EditDuty(int id)
        {
            DutyViewMode duty = await _sysAdminSvc.GetReference(id);
            return View(duty);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditReference(RefViewMode model)
        {
            if (ModelState.IsValid)
            {
                Reference reference;
                if (model.ID > 0)
                {
                    reference = await _sysAdminSvc.GetReference(model.ID);
                }
                else
                {
                    reference = new Reference
                    {
                        CreatedBy = UserHelper.GetUserID(HttpContext),
                        CreatedOn = DateTime.Now,
                    };
                }
                reference.ActiveStatus = model.ActiveStatus;
                reference.ReferenceCode = model.Code;
                reference.Type = RefViewMode.Parse(model.Type);
                reference.ReferenceValue = model.Name;
                try
                {
                    await _sysAdminSvc.SaveReference(reference);
                    return RedirectToAction(nameof(References), new { t = reference.Type });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "保存参照项失败：" + ex.Message);
                }
            }
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDuty(DutyViewMode duty)
        {
            if (ModelState.IsValid)
            {
                Reference reference;
                if (duty.DutyId > 0)
                {
                    reference = await _sysAdminSvc.GetReference(duty.DutyId);
                    if (reference.Type != ReferenceTypeEnum.Duty)
                    {
                        ModelState.AddModelError("", "视图模型不是岗位职责类型");
                        return View(duty);
                    }
                }
                else
                {
                    reference = new Reference
                    {
                        CreatedBy = UserHelper.GetUserID(HttpContext),
                        CreatedOn = DateTime.Now,
                        Type = ReferenceTypeEnum.Duty,
                    };
                }
                reference.ActiveStatus = duty.IsActive;
                if (duty.Category == DutyCategoryEnum.LeadingGroup)
                {
                    reference.ReferenceCode = DutyViewMode.LEADING_GROUP + duty.NativeCode?.Trim();
                }
                else
                {
                    reference.ReferenceCode = DutyViewMode.OTHERS + duty.NativeCode?.Trim();
                }
                reference.ReferenceValue = duty.Name;
                try
                {
                    await _sysAdminSvc.SaveReference(reference);
                    return RedirectToAction(nameof(References), new { t = reference.Type });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "保存岗位职责失败：" + ex.Message);
                }
            }
            return View(duty);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReference(RefViewMode model)
        {
            if (ModelState.IsValid)
            {
                if (model.ActiveStatus == true)
                {
                    ModelState.AddModelError("", $"不能删除正在使用中的{model.Type}, 请首先停用它");
                }
                else
                {
                    try
                    {
                        await _sysAdminSvc.DeleteReference(model.ID);
                        return RedirectToAction(nameof(References), new { t = RefViewMode.Parse(model.Type) });
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "删除参照项失败：" + ex.Message);
                    }
                }
            }
            return View(nameof(EditReference), model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDuty(DutyViewMode duty)
        {
            if (ModelState.IsValid)
            {
                if (duty.IsActive == true)
                {
                    ModelState.AddModelError("", $"不能删除正在使用中的岗位职责, 请首先停用它");
                }
                else
                {
                    try
                    {
                        await _sysAdminSvc.DeleteReference(duty.DutyId);
                        return RedirectToAction(nameof(References), new { t = ReferenceTypeEnum.Duty });
                    }
                    catch (Exception ex)
                    {
                        ModelState.AddModelError("", "删除参照项失败：" + ex.Message);
                    }
                }
            }
            return View(nameof(EditDuty), duty);
        }

        #endregion
    }
}