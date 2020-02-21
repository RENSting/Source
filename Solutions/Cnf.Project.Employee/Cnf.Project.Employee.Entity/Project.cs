using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Cnf.CodeBase.Data;
using Newtonsoft.Json;

namespace Cnf.Project.Employee.Entity
{
    public enum ProjectStatusEnum
    {
        Preparing,
        Processing,
        Completed,
        Closed
    }

    [DbTable("tb_project")]
    public class Project : EntityBase
    {
        [DbField("FullName", FieldType = SqlDbType.NVarChar)]
        public string FullName { get; set; }

        [DbField("ShortName", FieldType = SqlDbType.NVarChar, Size = 50)]
        public string ShortName { get; set; }

        [DbField("SitePlace", FieldType = SqlDbType.NVarChar, Size = 200)]
        public string SitePlace { get; set; }

        [DbField("Owner", FieldType = SqlDbType.NVarChar, Size = 200)]
        public string Owner { get; set; }

        [DbField("ContractCode", FieldType = SqlDbType.NVarChar, Size = 200)]
        public string ContractCode { get; set; }

        [DbField("StartDate", FieldType = SqlDbType.DateTime)]
        public DateTime StartTime { get; set; }

        [DbField("EndDate", FieldType = SqlDbType.DateTime)]
        public DateTime EndTime { get; set; }

        [DbField("Status", FieldType = SqlDbType.Int)]
        public int Status { get; set; }

        [JsonIgnore]
        public ProjectStatusEnum CurrentStatus
        {
            get { return (ProjectStatusEnum)Status; }
            set { Status = (int)value; }
        }
    }
}
