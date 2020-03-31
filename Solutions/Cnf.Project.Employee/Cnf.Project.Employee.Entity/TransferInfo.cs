using System;

namespace Cnf.Project.Employee.Entity
{
    public class TransferInfo
    {
        public int EmployeeId{get;set;}
        public int ProjectId{get;set;}
        public int DutyId{get;set;}
        public DateTime? TransferDate{get;set;}
        public int UserId{get;set;}
    }
}