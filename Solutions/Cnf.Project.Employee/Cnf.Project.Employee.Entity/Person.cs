using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using Cnf.CodeBase.Data;

namespace Cnf.Project.Employee.Entity
{
    [DbTable("tb_person")]
    public class Person: EntityBase
    {
        [DbField("Name", FieldType = SqlDbType.NVarChar, Size = 50)]
        public string Name { get; set; }
    }
}
