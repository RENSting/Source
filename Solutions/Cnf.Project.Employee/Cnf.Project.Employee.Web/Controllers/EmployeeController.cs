using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Cnf.Project.Employee.Entity;
using Cnf.Project.Employee.Web.Models;
using Newtonsoft.Json;

namespace Cnf.Project.Employee.Web.Controllers
{
    [Authorize]
    public class EmployeeController : Controller
    {
        readonly IEmployeeService _employeeService;
        readonly ISysAdminService _sysAdminService;
        public EmployeeController(IEmployeeService employeeService, ISysAdminService sysAdminService)
        {
            _employeeService = employeeService;
            _sysAdminService = sysAdminService;
        }

        public async Task<IActionResult> Index(string selectedOrg)
        {
            var organizations = await _sysAdminService.GetOrganiztions();
            SelectList listItems = new SelectList(organizations,
                nameof(Organization.ID), nameof(Organization.Name));
            if (string.IsNullOrEmpty(selectedOrg))
            {
                selectedOrg = organizations.FirstOrDefault()?.ID.ToString();
            }
            var employees = await _employeeService.GetEmployeesInOrg(Convert.ToInt32(selectedOrg ?? "0"));

            EmployeeListViewModel model = new EmployeeListViewModel
            {
                OrgList = listItems,
                SelectedOrg = selectedOrg,
                Employees = employees
            };

            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                Entity.Employee employee;
                if (model.EmployeeId > 0)
                {
                    employee = await _employeeService.GetEmployee(model.EmployeeId);
                    if (employee == null || employee.ID <= 0)
                    {
                        ModelState.AddModelError("", "该人员已经被删除或者其它原因没有找到");
                        return View(model);
                    }
                }
                else
                {
                    employee = new Entity.Employee
                    {
                        CreatedBy = UserHelper.GetUserID(HttpContext),
                        CreatedOn = DateTime.Now,
                    };
                }
                try
                {
                    employee.ActiveStatus = model.ActiveStatus;
                    employee.IdNumber = model.IdNumber;
                    employee.Name = model.Name;
                    employee.OrganizationID = model.OrganizationID;
                    employee.SN = model.SN;
                    employee.SpecialtyID = model.SpecialtyID;
                    employee.Title = model.Title;

                    var id = await _employeeService.SaveEmployee(employee);
                    return RedirectToAction(nameof(Edit), new { id = id });
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "保存人员错误：" + ex.Message);
                }
            }
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(EmployeeViewModel model)
        {
            try
            {
                var certs = await _employeeService.GetCertifications(model.EmployeeId);
                if (certs?.Length > 0)
                {
                    throw new Exception("删除人员前必须先删除他的全部证书");
                }

                await _employeeService.DeleteEmployee(model.EmployeeId);
                return RedirectToAction(nameof(Index), new { selectedOrg = model.OrganizationID.ToString() });
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(Edit),
                    new { id = model.EmployeeId, err = ex.Message });
            }
        }

        public async Task<IActionResult> Edit(string selectedOrg)
        {
            await PrepareDropdownForEdit();

            EmployeeViewModel model = new EmployeeViewModel
            {
                ActiveStatus = true,
            };

            if (int.TryParse(selectedOrg, out var orgId))
            {
                model.OrganizationID = orgId;
            }

            return View(model);
        }

        [HttpGet("{controller}/{action}/{id}")]
        public async Task<IActionResult> Edit(int id, string err)
        {
            var model = (EmployeeViewModel)await _employeeService.GetEmployee(id);
            if (model == null)
            {
                return RedirectToAction(nameof(Edit));
            }
            if (!string.IsNullOrWhiteSpace(err))
            {
                ModelState.AddModelError("", err);
            }
            await PrepareDropdownForEdit();
            return View(model);
        }

        async Task PrepareDropdownForEdit()
        {
            ViewBag.Organizations = new SelectList(
                await _sysAdminService.GetOrganiztions(),
                nameof(Organization.ID), nameof(Organization.Name));
            ViewBag.Specialties = new SelectList(
                await _sysAdminService.GetReferences(ReferenceTypeEnum.Specialty),
                nameof(Reference.ID), nameof(Reference.ReferenceValue));
        }

        public async Task<IActionResult> EmpCerts(int id)
        {
            var employee = await _employeeService.GetEmployee(id);
            if (employee == null || employee.ID <= 0)
            {
                ModelState.AddModelError("", "没有找到指定的人员");
                return View(new EmployeeCertsViewModel
                {
                    NewCertIssueDate = DateTime.Today,
                    NewCertExpireDate = DateTime.Today.AddYears(3),
                });
            }
            else
            {
                //get all certifications
                var certifications = await _employeeService.GetCertifications(employee.ID);
                var model = EmployeeCertsViewModel.Create(employee, certifications,
                        await _sysAdminService.GetReferences(ReferenceTypeEnum.Qualification));
                if (TempData.ContainsKey("CertAdd_Error"))
                {
                    ModelState.AddModelError("", Convert.ToString(TempData["CertAdd_Error"]));
                    var old_model = JsonConvert.DeserializeObject<EmployeeCertsViewModel>(
                            Convert.ToString(TempData["Error_Model"]));
                    model.NewCertActive = old_model.NewCertActive;
                    model.NewCertAuthUnit = old_model.NewCertAuthUnit;
                    model.NewCertExpireDate = old_model.NewCertExpireDate;
                    model.NewCertIssueDate = old_model.NewCertIssueDate;
                    model.NewCertName = old_model.NewCertName;
                    model.NewCertQualifId = old_model.NewCertQualifId;
                }
                return View(model);
            }
        }


        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CertAdd(EmployeeCertsViewModel model)
        {
            if (model.EmployeeId <= 0)
            {
                TempData["CertAdd_Error"] = "没有指定要添加证书的人员";
            }
            else if (string.IsNullOrWhiteSpace(model.NewCertName)
                    || model.NewCertAuthUnit?.Length > 200)
            {
                TempData["CertAdd_Error"] = "必须给证书命名一个名称；且发证机关不能超过200字";
            }
            else if (model.NewCertExpireDate < model.NewCertIssueDate)
            {
                TempData["CertAdd_Error"] = "失效日期不能早于发证日期, 对于长期有效的证件，可以指定一个遥远的失效日期";
            }
            else
            {
                var cert = new Certification
                {
                    ActiveStatus = model.NewCertActive,
                    AuthorityUnit = model.NewCertAuthUnit,
                    CertifyingDate = model.NewCertIssueDate,
                    CreatedOn = DateTime.Now,
                    CreatedBy = UserHelper.GetUserID(HttpContext),
                    EmployeeID = model.EmployeeId,
                    ExpireDate = model.NewCertExpireDate,
                    Name = model.NewCertName,
                    QualificationID = model.NewCertQualifId.Value
                };
                try
                {
                    await _employeeService.SaveCertification(cert);
                }
                catch (Exception ex)
                {
                    TempData["CertAdd_Error"] = "添加证书失败：" + ex.Message;
                }
            }
            if (TempData.ContainsKey("CertAdd_Error"))
            {
                TempData["Error_Model"] = JsonConvert.SerializeObject(model);
            }
            return RedirectToAction(nameof(EmpCerts), new { id = model.EmployeeId });
        }

        [ActionName("CertEdit")]
        public async Task<IActionResult> CertEdit(int id, string err)
        {
            try
            {
                var cert = await _employeeService.GetCertification(id);
                if (cert?.ID > 0)
                {
                    var employee = await _employeeService.GetEmployee(cert.EmployeeID);
                    if (employee.ID > 0)
                    {
                        var qulifs = await _sysAdminService.GetReferences(ReferenceTypeEnum.Qualification);

                        var model = CertViewModel.Create(cert, employee, qulifs);
                        if (!string.IsNullOrWhiteSpace(err))
                        {
                            ModelState.AddModelError("", err);
                        }
                        return View(model);
                    }
                    else
                    {
                        throw (new Exception("证书对应的人员不存在，或许该人员已经被删除"));
                    }
                }
                else
                {
                    throw new Exception("没有找到指定的证书");
                }
            }
            catch (Exception ex)
            {
                var model = new CertViewModel();
                ModelState.AddModelError("", ex.Message);
                return View(model);
            }
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CertEdit(CertViewModel model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (model.ExpireDate < model.CertifyingDate)
                    {
                        throw new Exception("证书失效日期不能早于颁发日期，如果长期有效，请选择一个遥远的日期");
                    }
                    var cert = await _employeeService.GetCertification(model.CertId);
                    cert.ActiveStatus = model.ActiveStatus;
                    cert.AuthorityUnit = model.AuthorityUnit;
                    cert.CertifyingDate = model.CertifyingDate;
                    cert.ExpireDate = model.ExpireDate;
                    cert.Name = model.CertName;
                    await _employeeService.SaveCertification(cert);
                    return RedirectToAction(nameof(EmpCerts), new { id = model.EmployeeId });
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
            return View(model);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CertDelete(CertViewModel model)
        {
            try
            {
                await _employeeService.DeleteCertification(model.CertId);
                return RedirectToAction(nameof(EmpCerts), new { id = model.EmployeeId });
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(CertEdit), new { id = model.CertId, err = ex.Message });
            }
        }
    }
}