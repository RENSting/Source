using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Cnf.CodeBase.Data;
using Newtonsoft.Json;

namespace Cnf.Project.Employee.Entity
{
    [DbTable("tb_TransferOutLog")]
    public class TransferOutLog : EntityBase
    {
        [DbField("OutEmployeeID", FieldType = SqlDbType.Int, IsForeignKey =true)]
        public int OutEmployeeID { get; set; }

        [DbField("OutProjectID", FieldType = SqlDbType.Int, IsForeignKey =true)]
        public int OutProjectID { get; set; }

        [DbField("OutDutyID", FieldType = SqlDbType.Int, IsForeignKey =true)]
        public int OutDutyID { get; set; }

        [DbField("OutDate", FieldType = SqlDbType.DateTime)]
        public DateTime OutDate { get; set; }

        [DbField("EmployeeName", FieldType = SqlDbType.NVarChar, Size = 50)]
        public string EmployeeName { get; set; }

        [DbField("ProjectName", FieldType = SqlDbType.NVarChar, Size = 400)]
        public string ProjectName { get; set; }

        [DbField("DutyName", FieldType = SqlDbType.NVarChar, Size = 200)]
        public string DutyName { get; set; }
    }
}
