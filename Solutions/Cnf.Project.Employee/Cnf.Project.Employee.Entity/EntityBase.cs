using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using Cnf.CodeBase.Data;

namespace Cnf.Project.Employee.Entity
{
    public class EntityBase
    {
        [DbField("ID", FieldType = SqlDbType.Int, IsPrimaryKey = true)]
        public int ID { get; set; }

        [DbField("ActiveStatus", FieldType = SqlDbType.Bit)]
        public bool ActiveStatus { get; set; }

        [DbField("CreatedBy", FieldType = SqlDbType.Int, IsForeignKey = true, Usage = DbFieldUsage.MapResultTable | DbFieldUsage.InsertParameter)]
        public int CreatedBy { get; set; }

        [DbField("CreatedOn", FieldType = SqlDbType.DateTime, Usage = DbFieldUsage.InsertParameter | DbFieldUsage.MapResultTable)]
        public DateTime CreatedOn { get; set; }

    }
}
