using Cnf.CodeBase.Serialize;
using Cnf.DataAccess.SqlServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace Cnf.Project.Employee.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : EmployeeControllerBase
    {
        public EmployeeController(IConfiguration configuration) : base(configuration)
        {
            ;
        }

        //
        // api/Employee/5
        //
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResult<Entity.Employee>>> GetEmployeeById(int id)
        {
            var employee = await DbHelper.FindEntity<Entity.Employee>(Connector, id);
            return Success(employee);
        }

        // 
        // api/Employee/InOrganization/2?forCheck=
        //  **** forCheck=true means just return the first one to check if any record exists

        [HttpGet("InOrganization/{id}")]
        public async Task<ActionResult<ApiResult<PagedQuery<Entity.Employee>>>> GetEmployeesInOrganization(
            int id, bool? forCheck)
        {
            var pageSize = (forCheck == null || forCheck.Value == false) ? int.MaxValue : 1;

            var employees = await DbHelper.SearchEntities<Entity.Employee>(Connector,
                new Dictionary<string, object> { [nameof(Entity.Employee.OrganizationID)] = id },
                0, pageSize, nameof(Entity.Employee.Name), false);

            return Success(employees);
        }

        // 
        // api/Employee/InProject/2?forCheck=
        //  **** forCheck=true means just return the first one to check if any record exists
        //
        [HttpGet("InProject/{id}")]
        public async Task<ActionResult<ApiResult<PagedQuery<Entity.Employee>>>> GetEmployeesInProject(
            int id, bool? forCheck)
        {
            var pageSize = (forCheck == null || forCheck.Value == false) ? int.MaxValue : 1;

            var employees = await DbHelper.SearchEntities<Entity.Employee>(Connector,
                new Dictionary<string, object> { [nameof(Entity.Employee.InProjectID)] = id },
                0, pageSize, nameof(Entity.Employee.Name), false);

            return Success(employees);
        }

        [HttpPost("Search")]
        public async Task<ActionResult<ApiResult<PagedQuery<Entity.Employee>>>> Search(
            [FromBody] Entity.CriteriaForEmployee criteria)
        {
            var whereClause = "(@name is null or [Name] LIKE '%' + @name + '%')";
            if(criteria.SelectedProj.HasValue)
            {
                if(criteria.SelectedProj.Value <=0)
                    whereClause += " AND ([InProjectID] is null OR [InProjectID]<=0)";
                else
                    whereClause += " AND [InProjectID]=" + criteria.SelectedProj.Value.ToString();
            }
            if(criteria.SelectedOrg.HasValue)
                whereClause += " AND [OrganizationID]=" + criteria.SelectedOrg.Value.ToString();
            if(criteria.SelectedSpec.HasValue)
                whereClause += " AND [SpecialtyID]=" + criteria.SelectedSpec.Value.ToString();
            if(criteria.ActiveOnly)
                whereClause += " AND [ActiveStatus]=1";

            var sql = DbHelper.BuildPagedSelectSql("*", "tb_employee", 
                whereClause, "Name", criteria.PageIndex, criteria.PageSize);
            
            SqlParameter p = new SqlParameter("@name", SqlDbType.NVarChar);
            if(!string.IsNullOrWhiteSpace(criteria.SearchName))
                p.Value = criteria.SearchName;
            else
                p.Value = DBNull.Value;
            DataSet ds = await Connector.ExecuteSqlQuerySet(sql, p);
            PagedQuery<Entity.Employee> result = new PagedQuery<Entity.Employee>();
            if (ds != null && ds.Tables.Count == 2)
            {
                result.Total = (int)ds.Tables[0].Rows[0][0];
                ds.Tables[1].TableName = "result";
                result.Records = await Task.Run(() => ValueHelper.WrapEntities<Entity.Employee>(ds.Tables[1]));
            }
            else
            {
                result.Total = 0;
                result.Records = new Entity.Employee[0];
            }
            return Success(result);
        }

        //
        // POST api/Employee/Save
        //

        [HttpPost("Save")]
        public async Task<ActionResult<ApiResult<int>>> Save([FromBody] Entity.Employee employee)
        {
            try
            {
                if (employee == null || string.IsNullOrWhiteSpace(employee.Name) || string.IsNullOrWhiteSpace(employee.IdNumber))
                {
                    return Error<int>("没有传递正确的员工信息");
                }

                if (employee.ID <= 0)
                {
                    if (employee.CreatedOn.Year <= 1900)
                        employee.CreatedOn = DateTime.Now;

                    int newId = await DbHelper.InsertEntity(Connector, employee);
                    return Success(newId);
                }
                else
                {
                    await DbHelper.UpdateEntity(Connector, employee);
                    return Success(employee.ID);
                }

            }
            catch (Exception ex)
            {
                return Error<int>(ex.Message);
            }

        }

        //
        // POST api/Employee/Delete   json(id)
        //
        [HttpPost("Delete")]
        public async Task<ActionResult<ApiResult<bool>>> Delete([FromBody] int id)
        {
            try
            {
                if (id <= 0)
                    return Error<bool>("超出了ID范围");

                await DbHelper.DeleteEntity<Entity.Employee>(Connector, id);
                return Success(true);

            }
            catch (Exception ex)
            {
                return Error<bool>(ex.Message);
            }
        }
    }
}