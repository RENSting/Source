using System.Threading.Tasks;
using Cnf.CodeBase.Serialize;
using Cnf.Project.Employee.Entity;

namespace Cnf.Project.Employee.Web.Models
{
    public interface IEmployeeService
    {
        Task<Entity.Employee[]> GetEmployeesInOrg(int? orgId);
        Task<int> SaveEmployee(Entity.Employee employee);
        Task<Entity.Employee> GetEmployee(int employeeId);
        Task<Certification[]> GetCertifications(int employeeId);
        Task SaveCertification(Certification certification);
        Task<Certification> GetCertification(int certId);
        Task DeleteCertification(int certId);
        Task DeleteEmployee(int employeeId);
        Task<Entity.Employee[]> GetEmployeesInProject(int projectId, bool forCheck);
        Task<PagedQuery<Entity.Employee>> SearchEmployee(CriteriaForEmployee criteria);
    }
    public class EmployeeService: IEmployeeService
    {
        const string ROUTE_EMPLOYEE_ORG = "api/Employee/InOrganization"; //need add /{orgId}
        const string ROUTE_SAVE = "api/Employee/Save";
        const string ROUTE_GET_ID = "api/Employee"; //need add /{id}
        const string ROUTE_DELETE = "api/Employee/Delete"; //post json(id)
        const string ROUTE_EMPLOYEE_CERTs = "api/Certification/Employee"; //need add /{emplId}
        const string ROUTE_SAVE_CERT = "api/Certification/Save";
        const string ROUTE_GET_CERT = "api/Certification";  //need add /{id}
        const string ROUTE_DELETE_CERT = "api/Certification/Delete"; //post json(id)
        const string ROUTE_PROJECT_EMPLOYEEs = "api/Employee/InProject"; //need add /{id}
        const string ROUTE_SEARCH = "api/Employee/Search";  //post CriterialForEmployee

        readonly IApiConnector _apiConnector;

        public EmployeeService(IApiConnector apiConnector) => _apiConnector = apiConnector;

        public async Task DeleteCertification(int certId)
        {
            await _apiConnector.HttpPost<int, bool>(ROUTE_DELETE_CERT, certId);
        }

        public async Task DeleteEmployee(int employeeId)
        {
            await _apiConnector.HttpPost<int, bool>(ROUTE_DELETE, employeeId);
        }

        public async Task<Certification> GetCertification(int certId)
        {
            return await _apiConnector.HttpGet<Certification>(ROUTE_GET_CERT + $"/{certId}");
        }

        public async Task<Certification[]> GetCertifications(int employeeId)
        {
            var result = await _apiConnector.HttpGet<PagedQuery<Certification>>(ROUTE_EMPLOYEE_CERTs + $"/{employeeId}");
            return result.Records;
        }

        public async Task<Entity.Employee> GetEmployee(int employeeId)
        {
            return await _apiConnector.HttpGet<Entity.Employee>(ROUTE_GET_ID + $"/{employeeId}");
        }

        public async Task<Entity.Employee[]> GetEmployeesInOrg(int? orgId)
        {
            if(orgId == null)orgId = 0;
            var result = await _apiConnector.HttpGet<PagedQuery<Entity.Employee>>(
                    ROUTE_EMPLOYEE_ORG + $"/{orgId}", "");
            
            return result.Records;
        }

        public async Task<Entity.Employee[]> GetEmployeesInProject(int projectId, bool forCheck) =>
            (await _apiConnector.HttpGet<PagedQuery<Entity.Employee>>(
                    ROUTE_PROJECT_EMPLOYEEs + $"/{projectId}" + (forCheck?"?forcheck=true":"")))?.Records;

        public async Task SaveCertification(Certification certification) =>
            await _apiConnector.HttpPost<Certification, int>(ROUTE_SAVE_CERT, certification);

        public async Task<int> SaveEmployee(Entity.Employee employee) => 
            await _apiConnector.HttpPost<Entity.Employee, int>(ROUTE_SAVE, employee);

        public async Task<PagedQuery<Entity.Employee>> SearchEmployee(CriteriaForEmployee criteria) =>
            await _apiConnector.HttpPost<CriteriaForEmployee,PagedQuery<Entity.Employee>>(
                ROUTE_SEARCH, criteria);
    }
}