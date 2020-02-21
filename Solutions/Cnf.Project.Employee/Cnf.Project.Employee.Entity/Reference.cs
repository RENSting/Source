using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Cnf.CodeBase.Data;
using Newtonsoft.Json;

namespace Cnf.Project.Employee.Entity
{
    public enum ReferenceTypeEnum
    {
        Specialty,
        Qualification,
        Duty
    }

    [DbTable("tb_reference")]
    public class Reference : EntityBase
    {
        [DbField("ReferenceType", FieldType = SqlDbType.NVarChar, Size = 50)]
        public string ReferenceType { get; set; }

        [DbField("ReferenceCode", FieldType = SqlDbType.NVarChar, Size = 50)]
        public string ReferenceCode { get; set; }

        [DbField("ReferenceValue", FieldType = SqlDbType.NVarChar, Size = 200)]
        public string ReferenceValue { get; set; }

        [JsonIgnore]
        public ReferenceTypeEnum Type
        {
            get { return (ReferenceTypeEnum)Enum.Parse(typeof(ReferenceTypeEnum), ReferenceType, true); }
            set { ReferenceType = value.ToString(); }
        }

        public static implicit operator Reference(Specialty specialty)
        {
            return new Reference()
            {
                ID = specialty.SpecialtyID,
                Type = ReferenceTypeEnum.Specialty,
                ReferenceCode = specialty.Code,
                ReferenceValue = specialty.Name,
                ActiveStatus = specialty.ActiveStatus,
                CreatedOn = specialty.CreateOn,
                CreatedBy = specialty.CreatedBy,
            };
        }

        public static implicit operator Reference(Qualification qualification)
        {
            return new Reference()
            {
                ID = qualification.QualificationID,
                Type = ReferenceTypeEnum.Qualification,
                ReferenceCode = qualification.Code,
                ReferenceValue = qualification.Name,
                ActiveStatus = qualification.ActiveStatus,
                CreatedOn = qualification.CreateOn,
                CreatedBy = qualification.CreatedBy,
            };
        }

        public static implicit operator Reference(Duty duty)
        {
            return new Reference()
            {
                ID = duty.DutyID,
                Type = ReferenceTypeEnum.Duty,
                ReferenceCode = duty.Code,
                ReferenceValue = duty.Name,
                ActiveStatus = duty.ActiveStatus,
                CreatedOn = duty.CreateOn,
                CreatedBy = duty.CreatedBy,
            };
        }
    }
}
