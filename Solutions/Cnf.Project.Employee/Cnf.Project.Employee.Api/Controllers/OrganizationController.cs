using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cnf.CodeBase.Serialize;
using Cnf.DataAccess.SqlServer;
using Cnf.Project.Employee.Entity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Cnf.Project.Employee.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrganizationController : EmployeeControllerBase
    {
        public OrganizationController(IConfiguration configuration):base(configuration)
        {
            ;
        }

        //
        // POST api/Organization/Save
        //
        [HttpPost("Save")]
        public async Task<ActionResult<ApiResult<int>>> Save([FromBody]Organization organization)
        {
            try
            {
                if (organization == null || string.IsNullOrWhiteSpace(organization.Name))
                {
                    return Error<int>("没有传递要保存的组织信息");
                }

                if (organization.ID <= 0)
                {
                    if (organization.CreatedOn.Year <= 1900)
                        organization.CreatedOn = DateTime.Now;

                    int newId = await DbHelper.InsertEntity(Connector, organization);
                    return Success(newId);
                }
                else
                {
                    await DbHelper.UpdateEntity(Connector, organization);
                    return Success(organization.ID);
                }

            }
            catch (Exception ex)
            {
                return Error<int>(ex.Message);
            }
        }

        //
        // GET api/Organization/Get
        //
        [HttpGet("Get")]
        public async Task<ActionResult<ApiResult<PagedQuery<Organization>>>> Get()
        {
            var organizations = await DbHelper.QueryPagedEntity<Organization>(
                Connector, 0, int.MaxValue, "Name", false);

            return Success(organizations);
        }

        //
        // GET api/Organizatin/5
        //
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResult<Organization>>> Get(int id)
        {
            var org = await DbHelper.FindEntity<Organization>(Connector, id);
            return Success(org);
        }

        //
        // POST api/Organization/Delete   json(id)
        //
        [HttpPost("Delete")]
        public async Task<ActionResult<ApiResult<bool>>> Delete([FromBody] int id)
        {
            try
            {
                if (id <= 0)
                    return Error<bool>("超出了ID范围");

                await DbHelper.DeleteEntity<Organization>(Connector, id);
                return Success(true);

            }
            catch (Exception ex)
            {
                return Error<bool>(ex.Message);
            }
        }
    }
}