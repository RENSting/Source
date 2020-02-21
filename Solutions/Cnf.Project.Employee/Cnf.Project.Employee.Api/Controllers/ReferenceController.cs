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
    public class ReferenceController : EmployeeControllerBase
    {
        public ReferenceController(IConfiguration configuration)
            : base(configuration)
        {
            ;
        }

        async Task<PagedQuery<Reference>> GetRerencesByType(ReferenceTypeEnum referenceType)
        {
            return await DbHelper.SearchEntities<Reference>(Connector,
                    new Dictionary<string, object>()
                    { [nameof(Reference.ReferenceType)] = referenceType.ToString() },
                    0, int.MaxValue, nameof(Reference.ReferenceValue), false);
        }

        //
        // api/Reference/GetSpecialties
        //

        [HttpGet("GetSpecialties")]
        public async Task<ActionResult<ApiResult<PagedQuery<Reference>>>> GetSpecialties()
        {
            try
            {
                return Success(await GetRerencesByType(ReferenceTypeEnum.Specialty));
            }
            catch (Exception ex)
            {
                return Error<PagedQuery<Reference>>(ex.Message);
            }
        }

        //
        // api/Reference/GetQualifications
        //

        [HttpGet("GetQualifications")]
        public async Task<ActionResult<ApiResult<PagedQuery<Reference>>>> GetQualifications()
        {
            try
            {
                return Success(await GetRerencesByType(ReferenceTypeEnum.Qualification));
            }
            catch (Exception ex)
            {
                return Error<PagedQuery<Reference>>(ex.Message);
            }
        }

        //
        // api/Reference/GetDuties
        //

        [HttpGet("GetDuties")]
        public async Task<ActionResult<ApiResult<PagedQuery<Reference>>>> GetDuties()
        {
            try
            {
                return Success(await GetRerencesByType(ReferenceTypeEnum.Duty));
            }
            catch (Exception ex)
            {
                return Error<PagedQuery<Reference>>(ex.Message);
            }
        }

        //
        // POST api/Reference/Save
        //

        [HttpPost("Save")]
        public async Task<ActionResult<ApiResult<int>>> Save([FromBody] Reference reference)
        {
            try
            {
                if (reference == null || string.IsNullOrWhiteSpace(reference.ReferenceValue) || string.IsNullOrWhiteSpace(reference.ReferenceType))
                {
                    return Error<int>("没有传递正确的参照项信息");
                }

                if (reference.ID <= 0)
                {
                    if (reference.CreatedOn.Year <= 1900)
                        reference.CreatedOn = DateTime.Now;

                    int newId = await DbHelper.InsertEntity(Connector, reference);
                    return Success(newId);
                }
                else
                {
                    await DbHelper.UpdateEntity(Connector, reference);
                    return Success(reference.ID);
                }

            }
            catch (Exception ex)
            {
                return Error<int>(ex.Message);
            }

        }

        //
        // POST api/Reference/Delete   json(id)
        //
        [HttpPost("Delete")]
        public async Task<ActionResult<ApiResult<bool>>> Delete([FromBody] int id)
        {
            try
            {
                if (id <= 0)
                    return Error<bool>("超出了ID范围");

                await DbHelper.DeleteEntity<Reference>(Connector, id);
                return Success(true);

            }
            catch (Exception ex)
            {
                return Error<bool>(ex.Message);
            }
        }

        //
        // GET api/Reference/Get
        //
        [HttpGet("Get")]
        public async Task<ActionResult<ApiResult<PagedQuery<Reference>>>> Get()
        {
            var reference = await DbHelper.QueryPagedEntity<Reference>(
                Connector, 0, int.MaxValue, nameof(Reference.ReferenceType) + "," + nameof(Reference.ReferenceValue), false);

            return Success(reference);
        }

        //
        // GET api/Reference/5
        //
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResult<Reference>>> Get(int id)
        {
            var reference = await DbHelper.FindEntity<Reference>(Connector, id);
            return Success(reference);
        }
    }
}