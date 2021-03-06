using System;
using System.Linq;
using System.Threading.Tasks;
using Cnf.CodeBase.Serialize;
using Cnf.Project.Employee.Entity;

namespace Cnf.Project.Employee.Web.Models
{
    public interface IProjectService
    {
        Task<Entity.Project[]> GetProjectByStatus(ProjectState? state);
        Task<Entity.Project> GetProjectById(int id);
        Task SaveProject(Entity.Project project);
        Task<TransferInLog[]> GetTransferInLogs(int? employeeId, int? projectId, bool? forCheck);
        Task<TransferOutLog[]> GetTransferOutLogs(int? employeeId, int? projectId, bool? forCheck);
        Task Delete(int projectId);
        Task<DutyQualification[]> GetDutyQualifications(int dutyId);
        Task<string> VerifyTransfer(int employeeId, int projectId, int dutyId);
        Task Transfer(int employeeId, int projectId, int dutyId, DateTime transferDate, int userId);
        Task UpdateRequiredQualifcations(DutyQualifViewModel model);
        Task<PagedQuery<Entity.Project>> SearchProject(ProjectState? state, string searchName, int pageIndex, int pageSize);
    }

    public class ProjectService : IProjectService
    {
        const string ROUTE_GET_PROJECTS = "api/Project/Status"; // can add /{statusId} (mo id means ignoring status)
        const string ROUTE_GET_PROJECT_ID = "api/Project"; //need add /{id}
        const string ROUTE_SAVE_PROJECT = "api/Project/Save";
        const string ROUTE_GET_INLOGs = "api/Project/Inlog"; //employeeId=&projectId=&forCheck=
        const string ROUTE_GET_OUTLOGs = "api/Project/Outlog"; //employeeId=&projectId=&forCheck=
        const string ROUTE_DELETE_PROJECT = "api/Project/Delete";  // post json(id)
        const string ROUTE_GET_DUTYQULIFs = "api/Project/RequiredQualifications";  // need add /{dutyId}
        const string ROUTE_TRANSFER = "api/Project/Transfer";  // json({EmployeeID, ProjectID, DutyID, UserID})
        const string ROUTE_UPDATE_NEEDQUALIFs = "api/Project/UpdateRequiredQualifcations"; // json(DutyQualifictionInfo)
        const string ROUTE_SEARCH_PROJECT = "api/Project/Search";  //json(CriteriaForProject)

        readonly IApiConnector _apiConnector;

        public ProjectService(IApiConnector apiConnector) => _apiConnector = apiConnector;

        public async Task Delete(int projectId) =>
            await _apiConnector.HttpPost<int, bool>(ROUTE_DELETE_PROJECT, projectId);

        public async Task<DutyQualification[]> GetDutyQualifications(int dutyId)=>
            (await _apiConnector.HttpGet<PagedQuery<DutyQualification>>(
                    ROUTE_GET_DUTYQULIFs + $"/{dutyId}")).Records;

        public async Task<Entity.Project> GetProjectById(int id) =>
            await _apiConnector.HttpGet<Entity.Project>(ROUTE_GET_PROJECT_ID + $"/{id}");

        public async Task<Entity.Project[]> GetProjectByStatus(ProjectState? state) =>
            (await _apiConnector.HttpGet<PagedQuery<Entity.Project>>(ROUTE_GET_PROJECTS
                + (state.HasValue ? "/" + Convert.ToString((int)state.Value) : ""))).Records;

        public async Task<TransferInLog[]> GetTransferInLogs(int? employeeId, int? projectId, bool? forCheck) =>
            (await _apiConnector.HttpGet<PagedQuery<TransferInLog>>(ROUTE_GET_INLOGs,
                $"employeeId={employeeId}&projectId={projectId}&forCheck={forCheck}"))?.Records;

        public async Task<TransferOutLog[]> GetTransferOutLogs(int? employeeId, int? projectId, bool? forCheck) =>
            (await _apiConnector.HttpGet<PagedQuery<TransferOutLog>>(ROUTE_GET_OUTLOGs,
                $"employeeId={employeeId}&projectId={projectId}&forCheck={forCheck}"))?.Records;

        public async Task SaveProject(Entity.Project project) =>
            await _apiConnector.HttpPost<Entity.Project, int>(ROUTE_SAVE_PROJECT, project);

        public async Task<PagedQuery<Entity.Project>> SearchProject(ProjectState? state, string searchName, int pageIndex, int pageSize)=>
            await _apiConnector.HttpPost<CriteriaForProject, PagedQuery<Entity.Project>>(
                ROUTE_SEARCH_PROJECT, new CriteriaForProject{
                    PageIndex = pageIndex, PageSize = pageSize,
                    ProjectStatus = state.HasValue?(ProjectStatusEnum)(int)state.Value
                                    :default(ProjectStatusEnum?),
                    SearchName = searchName,
                });

        public async Task Transfer(int employeeId, int projectId, int dutyId, DateTime transferDate, int userId)
        {
            var data = new TransferInfo{
                DutyId = dutyId, EmployeeId=employeeId, ProjectId=projectId, 
                TransferDate = transferDate,UserId = userId,
            };
            await _apiConnector.HttpPost<TransferInfo, int>(ROUTE_TRANSFER, data);
        }

        public async Task UpdateRequiredQualifcations(DutyQualifViewModel model)
        {
            var data = new DutyQualificationInfo{
                DutyID = model.DutyId,
                QualifIDs = string.IsNullOrWhiteSpace(model.QualifIds)?
                    default:
                    model.QualifIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                        .Select(q=>Convert.ToInt32(q)).ToArray(),
            };
            await _apiConnector.HttpPost<DutyQualificationInfo, bool>(ROUTE_UPDATE_NEEDQUALIFs, data);
        }

        public async Task<string> VerifyTransfer(int employeeId, int projectId, int dutyId)
        {
        // return: ApiResult.GetDate()  =   0     /// OK
        //                              =   1     /// WARNING (ApiResult.Message 包含 警告项)
        //
            var result = await _apiConnector.VerifyTransfer(projectId, dutyId, employeeId);
            if(result.IsSuccess)
            {
                var state = result.GetData();
                if(state == 0)
                    return "<span style=\"font-size:85%;\" class=\"badge badge-success\">OK</span>";
                else
                    return "<span style=\"font-size:85%;\" class=\"badge badge-warning\">" + result.Message + "</span>";
            }
            else
                return "<span style=\"font-size:85%;\" class=\"badge badge-danger\">" + result.Message + "</span>";
        }
    }
}