using Cnf.CodeBase.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Cnf.Project.Employee.Entity
{
    [DbTable("tb_organization")]
    public class Organization:EntityBase
    {
        [DbField("Name", FieldType =SqlDbType.NVarChar, Size =50)]
        public string Name { get; set; }
    }
}
