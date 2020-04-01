using System.Data;
using System.Collections.Generic;
using Cnf.Project.Employee.Entity;
using Cnf.CodeBase.Serialize;
using System.Threading.Tasks;

namespace Cnf.Project.Employee.Web.Models
{
    public interface IAnalysisService
    {
        Task<GroupPivot> GetProjectDistribution(bool activeOnly, IEnumerable<DutyCategoryEnum> categories);

        Task<GroupPivot> GetOrganizationDistribution(bool includeInactive);

        Task<TransferInLog[]> GetStaffInLogs(int staffId);
        Task<TransferOutLog[]> GetStaffOutLogs(int staffId);
    }

    public class AnalysisService : IAnalysisService
    {
        const string ROUTE_PROJ_DISTRIBUTION = "api/Analysis/DistributionInProject";
        const string ROUTE_ORG_DISTRIBUTION = "api/Analysis/DistributionInOrganization";
        const string ROUTE_IN_LOGS = "api/Project/Inlog";   //?employeeId=&projectId=&forCheck="
        const string ROUTE_OUT_LOGS = "api/Project/Outlog";  //?employeeId=&projectId=&forCheck="

        readonly IApiConnector _apiConnector;

        public AnalysisService(IApiConnector apiConnector)=> _apiConnector = apiConnector;

        public async Task<GroupPivot> GetProjectDistribution(
            bool activeOnly, IEnumerable<DutyCategoryEnum> categories) =>
            await _apiConnector.HttpGet<GroupPivot>(ROUTE_PROJ_DISTRIBUTION,
                $"activeOnly={activeOnly}&categories={DutyViewMode.GenerateCategoriesQuery(categories)}");

        public async Task<GroupPivot> GetOrganizationDistribution(bool includeInactive)=>
            await _apiConnector.HttpGet<GroupPivot>(ROUTE_ORG_DISTRIBUTION,
                includeInactive? "": "activeOnly=true");

        public async Task<TransferInLog[]> GetStaffInLogs(int staffId)=>
            (await _apiConnector.HttpGet<PagedQuery<TransferInLog>>(
                ROUTE_IN_LOGS, $"employeeId={staffId}")).Records;

        public async Task<TransferOutLog[]> GetStaffOutLogs(int staffId)=>
            (await _apiConnector.HttpGet<PagedQuery<TransferOutLog>>(
                ROUTE_OUT_LOGS, $"employeeId={staffId}")).Records;
    }
}