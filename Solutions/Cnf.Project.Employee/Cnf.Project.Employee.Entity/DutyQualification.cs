using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Cnf.CodeBase.Data;

namespace Cnf.Project.Employee.Entity
{
    [DbTable("tr_Duty_Qualification")]
    public class DutyQualification
    {
        [DbField("DutyQualificationID", FieldType = SqlDbType.Int, IsPrimaryKey = true)]
        public int ID { get; set; }

        [DbField("DutyID", FieldType = SqlDbType.Int, IsForeignKey = true)]
        public int DutyID { get; set; }

        [DbField("QualificationID", FieldType = SqlDbType.Int, IsForeignKey = true)]
        public int QualificationID { get; set; }

        [DbField("DutyName", FieldType = SqlDbType.NVarChar, Size = 200)]
        public string DutyName { get; set; }

        [DbField("QualificationName", FieldType = SqlDbType.NVarChar, Size = 200)]
        public string QualificationName { get; set; }
    }

    public class DutyQualificationInfo
    {
        public int DutyID{get;set;}

        public int[] QualifIDs{get;set;}
    }
}
