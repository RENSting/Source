using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Cnf.CodeBase.Serialize;
using Cnf.CodeBase.Data;
using Cnf.Project.Employee.Entity;
using Cnf.DataAccess.SqlServer;

namespace Cnf.Project.Employee.Api
{
    internal static class Helper
    {
        public static async Task<PagedQuery<Reference>> GetRerencesByType(
           DbConnector connector, ReferenceTypeEnum referenceType, bool? activeOnly)
        {
            var criteria = new Dictionary<string, object>();
            criteria.Add(nameof(Reference.ReferenceType), referenceType.ToString());
            if(activeOnly.HasValue && activeOnly.Value)
            {
                criteria.Add(nameof(Reference.ActiveStatus), activeOnly.Value);
            }

            return await DbHelper.SearchEntities<Reference>(connector, criteria,
                    0, int.MaxValue, nameof(Reference.ReferenceCode), false);
        }
        
    }
}