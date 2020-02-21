using Cnf.CodeBase.Serialize;
using Cnf.DataAccess.SqlServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cnf.Project.Employee.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : EmployeeControllerBase
    {
        public EmployeeController(IConfiguration configuration):base(configuration)
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
        // api/Employee/InOrganization/2
        //
        [HttpGet("InOrganization/{id}")]
        public async Task<ActionResult<ApiResult<PagedQuery<Entity.Employee>>>> GetEmployeesInOrganization(int id)
        {
            var employees = await DbHelper.SearchEntities<Entity.Employee>(Connector,
                new Dictionary<string, object> { [nameof(Entity.Employee.OrganizationID)] = id },
                0, int.MaxValue, nameof(Entity.Employee.Name), false);

            return Success(employees);
        }

        // 
        // api/Employee/InProject/2
        //
        [HttpGet("InProject/{id}")]
        public async Task<ActionResult<ApiResult<PagedQuery<Entity.Employee>>>> GetEmployeesInProject(int id)
        {
            var employees = await DbHelper.SearchEntities<Entity.Employee>(Connector,
                new Dictionary<string, object> { [nameof(Entity.Employee.InProjectID)] = id },
                0, int.MaxValue, nameof(Entity.Employee.Name), false);

            return Success(employees);
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