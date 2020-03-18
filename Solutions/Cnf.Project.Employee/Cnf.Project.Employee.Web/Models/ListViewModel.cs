using System.Collections.Generic;

namespace Cnf.Project.Employee.Web.Models
{
    public class ListViewModel<T>
    {
        public T[] Data{get;set;}

        public int Total{get;set;}

        public int PageIndex{get;set;}

        public int PageNumber{get;set;}
    }
}