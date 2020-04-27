using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Cnf.Project.Employee.Web.Models
{
    public class ProjectListViewModel:ListViewModel<ProjectViewState>
    {
        public ProjectState? SelectedState{get;set;}
        public string SearchName { get; set; }

        public static ProjectListViewModel Create(Entity.Project[] projects)=>
            new ProjectListViewModel{
                Data = projects.Select(p=>(ProjectViewState)p)?.ToArray(),
            };
    }
}