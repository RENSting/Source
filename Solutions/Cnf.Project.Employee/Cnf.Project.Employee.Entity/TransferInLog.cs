using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Cnf.CodeBase.Data;
using Newtonsoft.Json;

namespace Cnf.Project.Employee.Entity
{
    [DbTable("tb_TransferInLog")]
    public class TransferInLog : EntityBase
    {
        [DbField("InEmployeeID", FieldType = SqlDbType.Int, IsForeignKey =true)]
        public int InEmployeeID { get; set; }

        [DbField("InProjectID", FieldType = SqlDbType.Int, IsForeignKey = true)]
        public int InProjectID { get; set; }

        [DbField("InDutyID", FieldType = SqlDbType.Int, IsForeignKey = true)]
        public int InDutyID { get; set; }

        [DbField("InDate", FieldType = SqlDbType.DateTime)]
        public DateTime InDate { get; set; }

        [DbField("EmployeeName", FieldType = SqlDbType.NVarChar, Size = 50)]
        public string EmployeeName { get; set; }

        [DbField("ProjectName", FieldType = SqlDbType.NVarChar, Size = 400)]
        public string ProjectName { get; set; }

        [DbField("DutyName", FieldType = SqlDbType.NVarChar, Size = 200)]
        public string DutyName { get; set; }
    }
}
