using System;
using System.ComponentModel.DataAnnotations;

namespace Cnf.Project.Employee.Web.Models
{
    public class EntityViewModel
    {
        public int ID{get;set;}

        [Display(Name="有效")]
        public bool ActiveStatus{get;set;}

    }
}