using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Cnf.Project.Employee.Entity;

namespace Cnf.Project.Employee.Web.Models
{
    public class ProjDistViewModel
    {
        [Display(Name="仅班子成员")]
        public bool LeadingGroupOnly{get;set;}

        [Display(Name="仅有效人员")]
        public bool ActiveOnly{get;set;}

        public GroupPivot Pivot{get;set;}

        public IEnumerable<DutyCategoryEnum> GetSearchCategories() =>
            LeadingGroupOnly? new List<DutyCategoryEnum>{
                DutyCategoryEnum.LeadingGroup,
            }: null;
    }
}