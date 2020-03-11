using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Cnf.Project.Employee;

namespace Cnf.Project.Employee.Web.Models
{
    public class EmployeeListViewModel
    {
        public SelectList OrgList{get;set;}
        public string SelectedOrg{get;set;}

        public IEnumerable<Entity.Employee> Employees{get;set;}

        public int Total{get;set;}

        public int PageIndex{get;set;}

        public int PageNumber{get;set;}
    }
}