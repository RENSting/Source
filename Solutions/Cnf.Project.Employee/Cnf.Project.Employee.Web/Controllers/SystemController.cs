using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Cnf.Project.Employee.Entity;
using Cnf.Project.Employee.Web.Models;

namespace Cnf.Project.Employee.Web.Controllers
{
    [Authorize]
    public class SystemController : Controller
    {
        readonly ISysAdminService _sysAdminSvc;
        readonly IUserManager _userManager;

        public SystemController(ISysAdminService sysAdminService, IUserManager userManager)
        {
            _sysAdminSvc = sysAdminService;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            return View();
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
            RefViewMode model = new RefViewMode
            {
                ActiveStatus = true,
                Type = RefViewMode.Translate(t)
            };

            return View(model);
        }

        [HttpGet("{controller}/{action}/{id}")]
        public async Task<IActionResult> EditReference(int id)
        {
            RefViewMode model = await _sysAdminSvc.GetReference(id);
            if (model == null || model.ID <= 0)
            {
                ModelState.AddModelError("", "没有定位到要显示的参照项");
            }

            return View(model);
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

        #endregion
    }
}