using Cnf.Project.Employee.Entity;
using System.Threading.Tasks;

namespace Cnf.Project.Employee.Web.Models
{
    public interface IAnalysisService
    {
        Task<GroupPivot> GetProjectDistribution();

        Task<GroupPivot> GetOrganizationDistribution();
    }

    public class AnalysisService : IAnalysisService
    {
        const string ROUTE_PROJ_DISTRIBUTION = "api/Analysis/DistributionInProject";
        const string ROUTE_ORG_DISTRIBUTION = "api/Analysis/DistributionInOrganization";

        readonly IApiConnector _apiConnector;

        public AnalysisService(IApiConnector apiConnector)=> _apiConnector = apiConnector;

        public async Task<GroupPivot> GetProjectDistribution() =>
            await _apiConnector.HttpGet<GroupPivot>(ROUTE_PROJ_DISTRIBUTION);

        public async Task<GroupPivot> GetOrganizationDistribution()=>
            await _apiConnector.HttpGet<GroupPivot>(ROUTE_ORG_DISTRIBUTION);
    }
}