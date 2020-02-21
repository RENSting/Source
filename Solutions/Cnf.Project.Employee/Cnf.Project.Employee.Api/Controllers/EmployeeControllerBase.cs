using Cnf.CodeBase.Serialize;
using Cnf.DataAccess.SqlServer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Cnf.Project.Employee.Api.Controllers
{
    public abstract class EmployeeControllerBase : ControllerBase
    {
        protected IConfiguration Configuration { get; }

        protected DbConnector Connector { get; }

        protected ActionResult<ApiResult<T>> Success<T>(T result) where T:new()
        {
            return Success(result, null);
        }

        protected ActionResult<ApiResult<T>> Success<T>(T result, string message) where T:new()
        {
            var apiResult = new ApiResult<T>(result)
            {
                Message = message
            };
            return apiResult;
        }

        protected ActionResult<ApiResult<T>> Error<T>(string error) where T: new()
        {
            ApiResult<T> apiResult = new ApiResult<T>(CodeBase.Serialize.ApiResult<T>.EXCEPTION, error);
            return apiResult;
        }

        public EmployeeControllerBase(IConfiguration configuration)
        {
            Configuration = configuration;

            Connector = new DbConnector(configuration.GetConnectionString("PrjEmployees"));
        }
    }
}
