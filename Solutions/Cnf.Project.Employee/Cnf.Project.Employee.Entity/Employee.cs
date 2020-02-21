using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Cnf.CodeBase.Data;
using Newtonsoft.Json;

namespace Cnf.Project.Employee.Entity
{
    [DbTable("tb_employee")]
    public class Employee : EntityBase
    {
        [DbField("ProjectName", FieldType = SqlDbType.NVarChar, Size = 400)]
        public string ProjectName { get; set; }

        [DbField("DutyName", FieldType = SqlDbType.NVarChar, Size = 200)]
        public string DutyName { get; set; }

        [DbField("InProjectID", FieldType = SqlDbType.Int, IsForeignKey =true)]
        public int InProjectID { get; set; }

        [DbField("InDutyID", FieldType = SqlDbType.Int, IsForeignKey =true)]
        public int InDutyID { get; set; }

        [DbField("InDate", FieldType = SqlDbType.DateTime)]
        public DateTime InDate { get; set; }

        [DbField("OrganizationID", FieldType = SqlDbType.Int, IsForeignKey =true)]
        public int OrganizationID { get; set; }

        [DbField("SpecialtyID", FieldType = SqlDbType.Int, IsForeignKey =true)]
        public int SpecialtyID { get; set; }

        [DbField("SN", FieldType = SqlDbType.NVarChar, Size = 50)]
        public string SN { get; set; }

        [DbField("IdNumber", FieldType = SqlDbType.Char, Size = 18)]
        public string IdNumber { get; set; }

        [DbField("Name", FieldType = SqlDbType.NVarChar, Size = 50)]
        public string Name { get; set; }

        [DbField("Title", FieldType = SqlDbType.NVarChar, Size = 50)]
        public string Title { get; set; }

    }
}
