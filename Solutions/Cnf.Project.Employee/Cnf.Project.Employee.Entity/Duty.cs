using System;
using System.Collections.Generic;
using System.Text;

namespace Cnf.Project.Employee.Entity
{
    public class Duty
    {
        Reference _innerReference;

        public Duty(Reference reference)
        {
            if (reference.Type != ReferenceTypeEnum.Duty)
                throw new Exception("参照项不是‘职责’类型");

            _innerReference = reference;
        }

        public Duty(string code, string name, int createdBy)
        {
            _innerReference = new Reference()
            {
                ActiveStatus = true,
                CreatedBy = createdBy,
                CreatedOn = DateTime.Now,
                ReferenceCode = code,
                ReferenceValue = name,
                Type = ReferenceTypeEnum.Duty
            };
        }

        public int DutyID { get { return _innerReference.ID; } }

        public string Code { get { return _innerReference.ReferenceCode; } }

        public string Name { get { return _innerReference.ReferenceValue; } }

        public bool ActiveStatus { get { return _innerReference.ActiveStatus; } }

        public int CreatedBy { get { return _innerReference.CreatedBy; } }

        public DateTime CreateOn { get { return _innerReference.CreatedOn; } }

        public static implicit operator Duty(Reference reference)
        {
            if (reference.Type != ReferenceTypeEnum.Duty)
                throw new Exception("参照项不是‘职责’类型");

            return new Duty(reference);
        }
    }
}
