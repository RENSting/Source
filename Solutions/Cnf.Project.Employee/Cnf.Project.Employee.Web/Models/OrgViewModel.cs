using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Cnf.Project.Employee.Entity;

namespace Cnf.Project.Employee.Web.Models
{
    public class OrgViewModel
    {
        public int OrgId{get;set;}
        
        [Display(Name ="单位名称")]
        [Required(AllowEmptyStrings = false, ErrorMessage = "必须输入单位名称")]
        [MaxLength(20, ErrorMessage ="名称不能超过20个字")]
        public string OrgName { get; set; }

        [Display(Name ="是否有效")]
        public bool ActiveStatus { get; set; }

        public static implicit operator OrgViewModel(Organization organization)
        {
            return new OrgViewModel
            {
                OrgId = organization.ID,
                OrgName = organization.Name,
                ActiveStatus = organization.ActiveStatus
            };
        }
    }
}
