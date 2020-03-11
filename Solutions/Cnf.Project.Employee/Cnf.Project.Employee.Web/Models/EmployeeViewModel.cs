using System;
using System.ComponentModel.DataAnnotations;

namespace Cnf.Project.Employee.Web.Models
{
    public class EmployeeViewModel
    {
        public int EmployeeId{get;set;}

        [Display(Name="组织")]
        public int OrganizationID { get; set; }
        [Display(Name="专业")]
        public int SpecialtyID { get; set; }
        [Display(Name="人员编号")]
        [MaxLength(50, ErrorMessage="人员编号不能超过50位")]
        public string SN { get; set; }
        [Display(Name="身份证号"), Required(ErrorMessage="必须输入身份证号码")]
        [RegularExpression(@"(^\d{15}$)|(^\d{18}$)|(^\d{17}(\d|X|x)$)", ErrorMessage="身份证号无效")]
        public string IdNumber { get; set; }
        [Display(Name="姓名"), Required(ErrorMessage="必须输入姓名")]
        [MaxLength(50, ErrorMessage="姓名不能超过50位")]
        public string Name { get; set; }
        [Display(Name="职务/职称")]
        [MaxLength(50, ErrorMessage="职务/职称不能超过50位")]
        public string Title { get; set; }
        [Display(Name="有效性")]
        public bool ActiveStatus{get;set;}

        public static implicit operator EmployeeViewModel(Entity.Employee employee) =>
            new EmployeeViewModel
            {
                ActiveStatus = employee.ActiveStatus,
                EmployeeId = employee.ID,
                IdNumber = employee.IdNumber,
                Name = employee.Name,
                OrganizationID = employee.OrganizationID,
                SN = employee.SN,
                SpecialtyID = employee.SpecialtyID,
                Title = employee.Title,
            };
    }
}