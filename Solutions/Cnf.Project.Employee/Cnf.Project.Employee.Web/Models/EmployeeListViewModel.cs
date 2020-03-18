using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Cnf.Project.Employee;

namespace Cnf.Project.Employee.Web.Models
{
    public class EmployeeListViewModel : ListViewModel<Entity.Employee>
    {
        public SelectList OrgList { get; set; }
        public string SelectedOrg { get; set; }

        public Entity.Employee[] Employees
        { get { return Data; } set { Data = value; } }
    }
}