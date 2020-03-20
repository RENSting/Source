using Cnf.CodeBase.Serialize;
using Cnf.DataAccess.SqlServer;
using Cnf.Project.Employee.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cnf.Project.Employee.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectController : EmployeeControllerBase
    {
        public ProjectController(IConfiguration configuration) : base(configuration) { }

        //
        // api/Project/Search (post CriteriaForProject)
        //

        [HttpPost("Search")]
        public async Task<ActionResult<ApiResult<PagedQuery<Entity.Project>>>> Search(CriteriaForProject criteria)
        {
            var whereClause = @"
(@name is null OR [FullName] LIKE '%' + @name + '%'
 OR [ShortName] LIKE '%' + @name + '%'
 OR [SitePlace] LIKE '%' + @name + '%'
 OR [Owner] LIKE '%' + @name + '%'
 OR [ContractCode] LIKE '%' + @name + '%')";
            if(criteria.ProjectStatus.HasValue)
            {
                whereClause += " AND [Status]=" + ((int)criteria.ProjectStatus).ToString();
            }

            var sql = DbHelper.BuildPagedSelectSql("*", "tb_project", 
                whereClause, "[FullName]", criteria.PageIndex, criteria.PageSize);
            
            SqlParameter p = new SqlParameter("@name", SqlDbType.NVarChar);
            if(!string.IsNullOrWhiteSpace(criteria.SearchName))
                p.Value = criteria.SearchName;
            else
                p.Value = DBNull.Value;
            DataSet ds = await Connector.ExecuteSqlQuerySet(sql, p);
            PagedQuery<Entity.Project> result = new PagedQuery<Entity.Project>();
            if (ds != null && ds.Tables.Count == 2)
            {
                result.Total = (int)ds.Tables[0].Rows[0][0];
                ds.Tables[1].TableName = "result";
                result.Records = await Task.Run(() => ValueHelper.WrapEntities<Entity.Project>(ds.Tables[1]));
            }
            else
            {
                result.Total = 0;
                result.Records = new Entity.Project[0];
            }
            return Success(result);
        }

        //
        // api/Project/Status/1 (mo id means ignoring status)
        //
        [HttpGet("Status/{id?}")]
        public async Task<ActionResult<ApiResult<PagedQuery<Entity.Project>>>> GetProjectsByStatus(int? id)
        {
            PagedQuery<Entity.Project> pagedQuery;
            if (id == null)
            {
                pagedQuery = await DbHelper.QueryPagedEntity<Entity.Project>(Connector, 0, int.MaxValue, nameof(Entity.Project.FullName), false);
            }
            else
            {
                pagedQuery = await DbHelper.SearchEntities<Entity.Project>(Connector,
                    new Dictionary<string, object> { [nameof(Entity.Project.Status)] = id },
                    0, int.MaxValue, nameof(Entity.Project.FullName), false);
            }
            return Success(pagedQuery);
        }

        //
        // api/Project/5
        //
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResult<Entity.Project>>> GetProjectById(int id)
        {
            return Success(await DbHelper.FindEntity<Entity.Project>(Connector, id));
        }

        //
        // POST api/Project/Save json(Project)
        //
        [HttpPost("Save")]
        public async Task<ActionResult<ApiResult<int>>> Save([FromBody] Entity.Project project)
        {
            try
            {
                if (project == null || string.IsNullOrWhiteSpace(project.FullName))
                {
                    return Error<int>("没有传递正确的项目信息");
                }

                if (project.ID <= 0)
                {
                    if (project.CreatedOn.Year <= 1900)
                        project.CreatedOn = DateTime.Now;

                    int newId = await DbHelper.InsertEntity(Connector, project);
                    return Success(newId);
                }
                else
                {
                    await DbHelper.UpdateEntity(Connector, project);
                    return Success(project.ID);
                }

            }
            catch (Exception ex)
            {
                return Error<int>(ex.Message);
            }

        }

        //
        // POST api/Project/Delete json(id)
        //
        [HttpPost("Delete")]
        public async Task<ActionResult<ApiResult<bool>>> Delete([FromBody] int id)
        {
            try
            {
                if (id <= 0)
                    return Error<bool>("超出了ID范围");

                await DbHelper.DeleteEntity<Entity.Project>(Connector, id);
                return Success(true);

            }
            catch (Exception ex)
            {
                return Error<bool>(ex.Message);
            }
        }

        //
        // POST api/Project/Verify json({EmployeeID, ProjectID, DutyID})
        // return: ApiResult.GetDate()  =   0     /// OK
        //                              =   1     /// WARNING (ApiResult.Message 包含 警告项)
        //
        [HttpPost("Verify")]
        public async Task<ActionResult<ApiResult<int>>> VerifyTransfer([FromBody] TransferInfo transferInfo)
        {
            try
            {
                int employeeId = transferInfo.EmployeeId;
                int projectId = transferInfo.ProjectId;
                int dutyId = transferInfo.DutyId;

                if (employeeId <= 0 || projectId <= 0 || dutyId <= 0)
                {
                    return Error<int>("员工ID，项目ID，职责ID都不能为无效（小于或等于0）");
                }

                bool hasWarning = false;
                System.Text.StringBuilder warningBuilder = new System.Text.StringBuilder();

                var employee = await DbHelper.FindEntity<Entity.Employee>(Connector, employeeId);
                if (employee == null || employee.ID <= 0)
                    throw new Exception("人员不在库中");

                var project = await DbHelper.FindEntity<Entity.Project>(Connector, projectId);
                if (project == null || project.ID <= 0)
                    throw new Exception("项目不在库中");

                Duty duty = await DbHelper.FindEntity<Reference>(Connector, dutyId);
                if (duty == null || duty.DutyID <= 0)
                    throw new Exception("职责不在库中");

                if (employee.InProjectID > 0)
                {
                    //人员已经在项目中，列为警告
                    var inProject = await DbHelper.FindEntity<Entity.Project>(Connector, employee.InProjectID);
                    hasWarning = true;
                    warningBuilder.Append($"人员不是自由的，他/她在项目[{inProject.ShortName}]中。");
                }

                var pageRequired = await DbHelper.SearchEntities<DutyQualification>(Connector,
                    new Dictionary<string, object> { [nameof(DutyQualification.DutyID)] = dutyId },
                    0, int.MaxValue, "", false);
                if (pageRequired.Total > 0)
                {
                    //职责有资格要求
                    var pagedCertifications = await DbHelper.SearchEntities<Certification>(Connector,
                        new Dictionary<string, object> { [nameof(Certification.EmployeeID)] = employeeId },
                        0, int.MaxValue, nameof(Certification.ExpireDate), true);
                    foreach (DutyQualification reqired in pageRequired.Records)
                    {
                        if (pagedCertifications.Total > 0)
                        {
                            //人员具备一些资格证书，需要检查是否有所需的并且没有到期
                            var matched = from certification in pagedCertifications.Records
                                          where certification.QualificationID == reqired.QualificationID
                                                && certification.ExpireDate >= DateTime.Today
                                          select certification;
                            if (matched.Count() == 0)
                            {
                                hasWarning = true;
                                Qualification qualification = await DbHelper.FindEntity<Reference>(
                                    Connector, reqired.QualificationID);
                                warningBuilder.Append($"人员不具备要求的资格[{qualification.Name}]或资格证书已过期");
                            }
                        }
                        else
                        {
                            //人员没有任何资格证书
                            hasWarning = true;
                            Qualification qualification = await DbHelper.FindEntity<Reference>(
                                Connector, reqired.QualificationID);
                            warningBuilder.Append($"人员不具备要求的资格[{qualification.Name}]");
                        }
                    }
                }

                if (hasWarning)
                {
                    return Success(1, warningBuilder.ToString());
                }
                else
                {
                    return Success(0);
                }
            }
            catch (Exception ex)
            {
                return Error<int>(ex.ToString());
            }
        }

        //
        // POST api/Project/Transfer json({EmployeeID, ProjectID, DutyID, UserID})
        //
        [HttpPost("Transfer")]
        public async Task<ActionResult<ApiResult<int>>> Transfer([FromBody] TransferInfo transferInfo)
        {
            try
            {
                int employeeId = transferInfo.EmployeeId;
                int projectId = transferInfo.ProjectId;
                int dutyId = transferInfo.DutyId;
                int userId = transferInfo.UserId;

                if (employeeId <= 0)
                {
                    return Error<int>("员工ID不能为无效（小于或等于0）");
                }
                if (projectId > 0 && dutyId <= 0)
                {
                    return Error<int>("调入项目的同时必须分配职责");
                }

                var employee = await DbHelper.FindEntity<Entity.Employee>(Connector, employeeId);
                if (employee == null || employee.ID <= 0)
                    throw new Exception("人员不在库中");

                Entity.Project project;
                Duty duty;
                if (projectId > 0)
                {
                    project = await DbHelper.FindEntity<Entity.Project>(Connector, projectId);
                    if (project == null || project.ID <= 0)
                        throw new Exception("项目不在库中");
                    duty = await DbHelper.FindEntity<Reference>(Connector, dutyId);
                    if (duty == null || duty.DutyID <= 0)
                        throw new Exception("职责不在库中");
                }
                else
                {
                    project = null;
                    duty = null;
                }

                if (employee.InProjectID > 0)
                {
                    Entity.Project outProject = await DbHelper.FindEntity<Entity.Project>(Connector, employee.InProjectID);
                    //添加调出日志
                    TransferOutLog outLog = new TransferOutLog()
                    {
                        ActiveStatus = true,
                        CreatedBy = userId,
                        CreatedOn = DateTime.Now,
                        OutDate = DateTime.Today,
                        OutDutyID = employee.InDutyID,
                        OutEmployeeID = employeeId,
                        OutProjectID = employee.InProjectID,
                        EmployeeName = employee.Name,
                        ProjectName = string.IsNullOrEmpty(outProject.ShortName) ? outProject.FullName : outProject.ShortName,
                    };
                    if (employee.InDutyID > 0)
                    {
                        outLog.DutyName = (await DbHelper.FindEntity<Reference>(Connector, employee.InDutyID)).ReferenceValue;
                    }
                    await DbHelper.InsertEntity(Connector, outLog);
                }

                if (project == null)
                {
                    //将人员设置为自由, 无需任何调入日志
                    employee.InDate = ValueHelper.DbDate_Null;
                    employee.InDutyID = ValueHelper.DBKEY_NULL;
                    employee.InProjectID = ValueHelper.DBKEY_NULL;
                    employee.ProjectName = string.Empty;
                    employee.DutyName = string.Empty;
                }
                else
                {
                    //添加调入日志
                    TransferInLog inLog = new TransferInLog()
                    {
                        ActiveStatus = true,
                        CreatedBy = userId,
                        CreatedOn = DateTime.Now,
                        InDate = DateTime.Today,
                        InDutyID = dutyId,
                        InEmployeeID = employeeId,
                        InProjectID = projectId,
                        EmployeeName = employee.Name,
                        ProjectName = string.IsNullOrEmpty(project.ShortName) ? project.FullName : project.ShortName,
                        DutyName = duty.Name
                    };
                    await DbHelper.InsertEntity(Connector, inLog);

                    //修改人员状态
                    employee.InDate = DateTime.Today;
                    employee.InDutyID = dutyId;
                    employee.InProjectID = projectId;
                    employee.ProjectName = string.IsNullOrEmpty(project.ShortName) ? project.FullName : project.ShortName;
                    employee.DutyName = duty.Name;
                }
                await DbHelper.UpdateEntity(Connector, employee);
                return Success(0);
            }
            catch (Exception ex)
            {
                return Error<int>(ex.Message);
            }
        }

        //
        // api/Project/Inlog?employeeId=&projectId=&forCheck=
        //  **** forCheck=true means just return the first one to check if any record exists
        //
        [HttpGet("Inlog")]
        public async Task<ActionResult<ApiResult<PagedQuery<TransferInLog>>>> GetInlogs(
            [FromQuery] int? employeeId, [FromQuery] int? projectId, bool? forCheck)
        {
            var pageSize = (forCheck == null || forCheck.Value == false) ? int.MaxValue : 1;

            Dictionary<string, object> criteria = new Dictionary<string, object>();
            if (employeeId.HasValue)
                criteria.Add(nameof(TransferInLog.InEmployeeID), employeeId.Value);
            if (projectId.HasValue)
                criteria.Add(nameof(TransferInLog.InProjectID), projectId.Value);

            var pagedQuery = await DbHelper.SearchEntities<TransferInLog>(
                Connector, criteria, 0, pageSize, nameof(TransferInLog.InDate), false);
            return Success(pagedQuery);
        }

        //
        // api/Project/Outlog?employeeId=&projectId=&forCheck=
        //  **** forCheck=true means just return the first one to check if any record exists
        //
        [HttpGet("Outlog")]
        public async Task<ActionResult<ApiResult<PagedQuery<TransferOutLog>>>> GetOutlogs(
            [FromQuery] int? employeeId, [FromQuery] int? projectId, bool? forCheck)
        {
            var pageSize = (forCheck == null || forCheck.Value == false) ? int.MaxValue : 1;

            Dictionary<string, object> criteria = new Dictionary<string, object>();
            if (employeeId.HasValue)
                criteria.Add(nameof(TransferOutLog.OutEmployeeID), employeeId.Value);
            if (projectId.HasValue)
                criteria.Add(nameof(TransferOutLog.OutProjectID), projectId.Value);

            var pagedQuery = await DbHelper.SearchEntities<TransferOutLog>(
                Connector, criteria, 0, pageSize, nameof(TransferOutLog.OutDate), false);
            return Success(pagedQuery);
        }

        //
        // api/Project/RequiredQualifications/5  (id=DutyID)
        //
        [HttpGet("RequiredQualifications/{id}")]
        public async Task<ActionResult<ApiResult<PagedQuery<DutyQualification>>>> RequiredQualifications([FromRoute] int id)
        {
            var requiredPaged = await DbHelper.SearchEntities<DutyQualification>(
                Connector, new Dictionary<string, object> { [nameof(DutyQualification.DutyID)] = id },
                0, int.MaxValue, nameof(DutyQualification.QualificationName), false);
            return Success(requiredPaged);
        }

        //
        // api/Project/UpdateRequiredQualifcations json(DutyQualifictionInfo)
        //
        [HttpPost("UpdateRequiredQualifcations")]
        public async Task<ActionResult<ApiResult<bool>>> UpdateRequiredQualifcations(DutyQualificationInfo info)
        {
            try
            {
                var sql = "DELETE FROM [tr_Duty_Qualification] WHERE DutyID=" + info.DutyID.ToString();
                await Connector.ExecuteSqlNonQuery(sql);
                if (info.QualifIDs?.Length > 0)
                {
                    foreach (var qualifId in info.QualifIDs)
                    {
                        await AddQualification(new { DutyID = info.DutyID, QualificationID = qualifId });
                    }
                }
                return Success(true);
            }
            catch (Exception ex)
            {
                return Error<bool>(ex.Message);
            }
        }

        //
        // POST api/Project/AddQualification json({QualificationID, DutyID})
        //
        [HttpPost("AddQualification")]
        public async Task<ActionResult<ApiResult<int>>> AddQualification([FromBody] dynamic addInfo)
        {
            try
            {
                int qualificationId = Convert.ToInt32(addInfo.QualificationID);
                int dutyId = Convert.ToInt32(addInfo.DutyID);

                Qualification qualification = await DbHelper.FindEntity<Reference>(Connector, qualificationId);
                Duty duty = await DbHelper.FindEntity<Reference>(Connector, dutyId);

                if (qualification == null || qualification.QualificationID <= 0)
                    throw new Exception("没有提供有效的资格类型ID");
                if (duty == null || duty.DutyID <= 0)
                    throw new Exception("没有提供有效的职责ID");

                var matches = await DbHelper.SearchEntities<DutyQualification>(Connector,
                    new Dictionary<string, object>
                    {
                        [nameof(DutyQualification.DutyID)] = dutyId,
                        [nameof(DutyQualification.QualificationID)] = qualificationId
                    }, 0, int.MaxValue, string.Empty, false);

                if (matches.Total <= 0)
                {
                    var record = new DutyQualification
                    {
                        DutyID = dutyId,
                        DutyName = duty.Name,
                        QualificationID = qualificationId,
                        QualificationName = qualification.Name
                    };

                    await DbHelper.InsertEntity(Connector, record);
                }

                return Success(0);
            }
            catch (Exception ex)
            {
                return Error<int>(ex.Message);
            }
        }

        //
        // POST api/Project/RemoveQualification json(int id)
        //
        [HttpPost("RemoveQualification")]
        public async Task<ActionResult<ApiResult<int>>> RemoveQualification([FromBody] int id)
        {
            await DbHelper.DeleteEntity<DutyQualification>(Connector, id);

            return Success(0);
        }
    }
}