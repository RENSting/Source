using System;
using System.Collections.Generic;
using System.Text;

namespace Cnf.Project.Employee.Entity
{
    public class Qualification
    {
        Reference _innerReference;

        public Qualification(Reference reference)
        {
            if (reference.Type != ReferenceTypeEnum.Qualification)
                throw new Exception("参照项不是‘资格’类型");

            _innerReference = reference;
        }

        public Qualification(string code, string name, int createdBy)
        {
            _innerReference = new Reference()
            {
                ActiveStatus = true,
                CreatedBy = createdBy,
                CreatedOn = DateTime.Now,
                ReferenceCode = code,
                ReferenceValue = name,
                Type = ReferenceTypeEnum.Qualification
            };
        }

        public int QualificationID { get { return _innerReference.ID; } }

        public string Code { get { return _innerReference.ReferenceCode; } }

        public string Name { get { return _innerReference.ReferenceValue; } }

        public bool ActiveStatus { get { return _innerReference.ActiveStatus; } }

        public int CreatedBy { get { return _innerReference.CreatedBy; } }

        public DateTime CreateOn { get { return _innerReference.CreatedOn; } }

        public static implicit operator Qualification(Reference reference)
        {
            if (reference.Type != ReferenceTypeEnum.Qualification)
                throw new Exception("参照项不是‘资格’类型");

            return new Qualification(reference);
        }
    }
}
