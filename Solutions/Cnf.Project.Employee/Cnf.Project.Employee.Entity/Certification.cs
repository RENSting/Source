using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Cnf.CodeBase.Data;
using Newtonsoft.Json;

namespace Cnf.Project.Employee.Entity
{
    [DbTable("tb_certification")]
    public class Certification : EntityBase
    {
        [DbField("Name", FieldType = SqlDbType.NVarChar)]
        public string Name { get; set; }

        [DbField("EmployeeID", FieldType = SqlDbType.Int)]
        public int EmployeeID { get; set; }

        [DbField("QualificationID", FieldType = SqlDbType.Int)]
        public int QualificationID { get; set; }

        [DbField("AuthorityUnit", FieldType = SqlDbType.NVarChar, Size = 200)]
        public string AuthorityUnit { get; set; }

        [DbField("CertifyingDate", FieldType = SqlDbType.DateTime)]
        public DateTime CertifyingDate { get; set; }

        [DbField("ExpireDate", FieldType = SqlDbType.DateTime)]
        public DateTime ExpireDate { get; set; }

    }
}
