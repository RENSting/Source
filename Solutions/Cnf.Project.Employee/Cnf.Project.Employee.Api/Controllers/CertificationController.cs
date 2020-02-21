using Cnf.CodeBase.Serialize;
using Cnf.DataAccess.SqlServer;
using Cnf.Project.Employee.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Cnf.Project.Employee.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CertificationController : EmployeeControllerBase
    {
        public CertificationController(IConfiguration configuration) : base(configuration)
        {
            ;
        }

        //
        // api/Certification/Employee/5
        //
        [HttpGet("Employee/{id}")]
        public async Task<ActionResult<ApiResult<PagedQuery<Certification>>>> GetCertificationsOfEmployee(int id)
        {
            var result = await DbHelper.SearchEntities<Certification>(Connector,
                new Dictionary<string, object> { [nameof(Certification.EmployeeID)] = id },
                0, int.MaxValue, nameof(Certification.ExpireDate), true);
            return Success(result);
        }

        //
        // api/Certification/5
        //
        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResult<Certification>>> GetCertificationById(int id)
        {
            var certification = await DbHelper.FindEntity<Certification>(Connector, id);
            return Success(certification);
        }

        //
        // POST api/Certification/Save json(Certification)
        //
        [HttpPost("Save")]
        public async Task<ActionResult<ApiResult<int>>> Save([FromBody] Certification certification)
        {
            try
            {
                if (certification == null || string.IsNullOrWhiteSpace(certification.Name) 
                    || certification.EmployeeID<= 0 || certification.QualificationID <= 0)
                {
                    return Error<int>("没有传递正确的资格证信息");
                }

                if (certification.ExpireDate.Year <= 1900)
                    certification.ExpireDate = DateTime.MaxValue;

                if (certification.ID <= 0)
                {
                    if (certification.CreatedOn.Year <= 1900)
                        certification.CreatedOn = DateTime.Now;

                    int newId = await DbHelper.InsertEntity(Connector, certification);
                    return Success(newId);
                }
                else
                {
                    await DbHelper.UpdateEntity(Connector, certification);
                    return Success(certification.ID);
                }

            }
            catch (Exception ex)
            {
                return Error<int>(ex.Message);
            }

        }

        //
        // POST api/Certification/Delete json(id)
        //
        [HttpPost("Delete")]
        public async Task<ActionResult<ApiResult<bool>>> Delete([FromBody] int id)
        {
            try
            {
                if (id <= 0)
                    return Error<bool>("超出了ID范围");

                await DbHelper.DeleteEntity<Certification>(Connector, id);
                return Success(true);

            }
            catch (Exception ex)
            {
                return Error<bool>(ex.Message);
            }
        }

    }
}