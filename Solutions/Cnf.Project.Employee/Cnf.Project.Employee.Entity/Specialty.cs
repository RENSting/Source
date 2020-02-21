using System;
using System.Collections.Generic;
using System.Text;

namespace Cnf.Project.Employee.Entity
{
    public class Specialty
    {
        Reference _innerReference;

        public Specialty (Reference reference)
        {
            if (reference.Type != ReferenceTypeEnum.Specialty)
                throw new Exception("参照项不是‘专业’类型");

            _innerReference = reference;
        }

        public Specialty(string code, string name, int createdBy)
        {
            _innerReference = new Reference()
            {
                ActiveStatus = true,
                CreatedBy = createdBy,
                CreatedOn = DateTime.Now,
                ReferenceCode = code,
                ReferenceValue = name,
                Type = ReferenceTypeEnum.Specialty
            };
        }

        public int SpecialtyID { get { return _innerReference.ID; } }

        public string Code { get { return _innerReference.ReferenceCode; } }

        public string Name { get { return _innerReference.ReferenceValue; } }

        public bool ActiveStatus { get { return _innerReference.ActiveStatus; } }

        public int CreatedBy { get { return _innerReference.CreatedBy; } }

        public DateTime CreateOn { get { return _innerReference.CreatedOn; } }

        public static implicit operator Specialty(Reference reference)
        {
            if (reference.Type != ReferenceTypeEnum.Specialty)
                throw new Exception("参照项不是‘专业’类型");

            return new Specialty(reference);
        }
    }
}
