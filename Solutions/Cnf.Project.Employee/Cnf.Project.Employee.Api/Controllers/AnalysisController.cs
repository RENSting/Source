using System;
using System.Linq;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Cnf.CodeBase.Serialize;
using Cnf.DataAccess.SqlServer;
using Cnf.Project.Employee.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Cnf.Project.Employee.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalysisController : EmployeeControllerBase
    {
        public AnalysisController(IConfiguration configuration) : base(configuration) { }

        //
        // api/Analysis/DistributionInProject?activeOnly=true/false/null&categories=ABC...
        //

        [HttpGet("DistributionInProject")]
        public async Task<ActionResult<ApiResult<GroupPivot>>> ProjectDistribution(bool? activeOnly, string categories)
        {
            var dutyQuery = await Helper.GetRerencesByType(Connector, ReferenceTypeEnum.Duty, activeOnly);
            var duties = string.IsNullOrWhiteSpace(categories)?
                        dutyQuery.Records:
                        from d in dutyQuery.Records
                        where (d.ReferenceCode != null && d.ReferenceCode.Length > 0)
                            && categories.Contains(d.ReferenceCode.ToCharArray()[0], StringComparison.OrdinalIgnoreCase)
                        select d;
                        
            StringBuilder dutyListBuilder = new StringBuilder();
            foreach(var duty in duties)
            {
                dutyListBuilder.Append($", [{duty.ReferenceValue}]");
            }
            if(dutyListBuilder.Length > 0)
                dutyListBuilder.Remove(0, 2);
            
            string hideInactive = activeOnly==null?"":activeOnly.Value?"[ActiveStatus]=1":"";
            
            string sql = @"
WITH base AS (
    SELECT ProjectName, DutyName, 
           STUFF((SELECT ',' + CONVERT(nvarchar(max), ID)+ '|' + Name + '|'
                    + CONVERT(NVARCHAR(max), ActiveStatus) + '|' + CONVERT(NVARCHAR(max), InDutyID)
                    FROM tb_employee
                    WHERE ProjectName = e.ProjectName
                        AND DutyName = e.DutyName
                    FOR xml PATH('')
                ), 1, 1, '') AS EmployeeName
    FROM tb_employee e
    WHERE ISNULL(ProjectName, '') <> ''" 
        + (string.IsNullOrEmpty(hideInactive)?"":(" AND " + hideInactive)) + @"
    GROUP BY ProjectName, DutyName
)
SELECT ProjectName AS [项目名称]," + dutyListBuilder.ToString() + @" 
FROM base
 PIVOT (
    MAX(EmployeeName)
    FOR DutyName
    IN (" + dutyListBuilder.ToString() + @")
) AS final_result
            ";
            
            GroupPivot pivot = await Connector.ExecuteSqlQueryTable(sql);

            return Success(pivot);
        }

        //
        // api/Analysis/DistributionInOrganization?activeOnly=true/false/null
        //

        [HttpGet("DistributionInOrganization")]
        public async Task<ActionResult<ApiResult<GroupPivot>>> OrganizationDistribution(bool? activeOnly)
        {
            var qualifQuery = await Helper.GetRerencesByType(Connector, ReferenceTypeEnum.Qualification, activeOnly);
            StringBuilder qualifListBuilder = new StringBuilder();
            foreach(var duty in qualifQuery.Records)
            {
                qualifListBuilder.Append($", [{duty.ReferenceValue}]");
            }
            if(qualifListBuilder.Length > 0)
                qualifListBuilder.Remove(0, 2);
            
            string hideInactive = activeOnly==null?"":activeOnly.Value?"e.[ActiveStatus]=1":"";

            string sql = @"
WITH src AS (
    SELECT  o.Name AS OrgName, 
            r.ReferenceValue AS QualifName, 
            CONVERT(NVARCHAR(MAX), e.ID) + '|' + e.Name 
            + '|' + CONVERT(NVARCHAR(max), e.ActiveStatus) AS EmployeeName
    FROM tb_employee e INNER JOIN tb_organization o ON e.OrganizationID=o.ID
            INNER JOIN tb_certification c ON e.ID=c.EmployeeID
            INNER JOIN tb_reference r ON c.QualificationID = r.ID"
        + (string.IsNullOrEmpty(hideInactive)?"":(" WHERE " + hideInactive)) + @"
)
SELECT OrgName AS [单位名称], " + qualifListBuilder.ToString() + @" 
FROM (
    SELECT OrgName, QualifName, 
        STUFF(
            (
                SELECT ',' + EmployeeName FROM src
                WHERE OrgName = src1.OrgName
                    AND QualifName = src1.QualifName
                FOR xml PATH('')
            ), 1, 1, ''
        ) AS StaffList
    FROM src src1
    GROUP BY OrgName, QualifName
) base PIVOT (
    MAX(StaffList)
    FOR QualifName
    IN (" + qualifListBuilder.ToString() + @")
) AS final_result;";

            GroupPivot pivot = await Connector.ExecuteSqlQueryTable(sql);

            return Success(pivot);
        }
    }
}
