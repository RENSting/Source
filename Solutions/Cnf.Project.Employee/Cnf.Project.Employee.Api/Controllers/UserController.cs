using Cnf.CodeBase.Serialize;
using Cnf.DataAccess.SqlServer;
using Cnf.Project.Employee.Entity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Threading.Tasks;

namespace Cnf.Project.Employee.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : EmployeeControllerBase
    {
        public UserController(IConfiguration configuration) : base(configuration)
        {
            ;
        }

        // api/User/GetUsers

        [HttpGet("GetUsers")]
        public async Task<ActionResult<ApiResult<PagedQuery<User>>>> GetUsers()
        {
            PagedQuery<User> pagedQuery = await DbHelper.QueryPagedEntity<User>(Connector, 0, int.MaxValue, "Name", false);
            return Success(pagedQuery);
        }

        // api/User/GetUserById/5

        [HttpGet("GetUserById/{userId}")]
        [HttpGet("GetUserById")]
        public async Task<ActionResult<ApiResult<User>>> GetUserById(int userId)
        {
            User user = await DbHelper.FindEntity<User>(Connector, userId);
            return Success(user);
        }

        // api/User/Save

        /// <summary>
        /// input: json(User);
        /// output: ApiResult(int UserId);
        /// </summary>
        /// <returns></returns>
        [HttpPost("Save")]
        public async Task<ActionResult<ApiResult<int>>> SaveUser()
        {
            using StreamReader sr = new StreamReader(Request.Body);
            try
            {
                string postData = await sr.ReadToEndAsync();
                User user = SerializationHelper.JsonDeserialize<User>(postData);
                if (user.UserID <= 0)
                {
                    //insert a new user
                    user.UserID = await DbHelper.InsertEntity(Connector, user);
                }
                else
                {
                    await DbHelper.UpdateEntity(Connector, user);
                }
                return Success(user.UserID);
            }
            catch (Exception ex)
            {
                return Error<int>(ex.Message);
            }
        }

        // api/User/ChangeCredential

        /// <summary>
        /// input: Json(ChangeCredential)
        /// output: ApiResult(true) or ApiResult(error)
        /// </summary>
        /// <returns></returns>
        [HttpPost("ChangeCredential")]
        public async Task<ActionResult<ApiResult<bool>>> ChangeCredential()
        {
            using StreamReader sr = new StreamReader(Request.Body);
            try
            {
                ChangeCredential changeCredential = SerializationHelper.JsonDeserialize<ChangeCredential>(
                    await sr.ReadToEndAsync());

                DataTable dataTable = await Connector.ExecuteSqlQueryTable(
                    "SELECT [Password] FROM [tb_user] WHERE [UserID]=@userId",
                    new SqlParameter("@userId", changeCredential.UserID));

                if (dataTable == null || dataTable.Rows.Count == 0)
                {
                    throw new Exception("没有找到用户。");
                }
                string oldCredential = Convert.ToString(dataTable.Rows[0]["Password"]);
                if (string.IsNullOrEmpty(oldCredential)
                    || oldCredential.Equals(changeCredential.OldCredential))
                {
                    //用户当前口令为空白，可以随意修改
                    await Connector.ExecuteSqlNonQuery(
                        "UPDATE [tb_user] SET [Password]=@password WHERE [UserID]=@userId",
                        new SqlParameter("@password", changeCredential.NewCredential),
                        new SqlParameter("@userId", changeCredential.UserID));

                    return Success(true);
                }
                else
                {
                    throw new Exception("提交的原始口令不相符");
                }
            }
            catch (Exception ex)
            {
                return Error<bool>(ex.Message);
            }

        }

        // api/User/Authenticate

        /// <summary>
        /// input: Json(Authentication)
        /// return: ApiResult(User) or ApiResult(error)
        /// </summary>
        /// <returns></returns>
        [HttpPost("Authenticate")]
        public async Task<ActionResult<ApiResult<User>>> Authenticate()
        {
            using StreamReader sr = new StreamReader(Request.Body);
            try
            {
                Authentication authentication = SerializationHelper.JsonDeserialize<Authentication>(
                    await sr.ReadToEndAsync());

                DataTable dataTable = await Connector.ExecuteSqlQueryTable(
                    "SELECT [UserID] FROM [tb_user] WHERE [Login]=@login AND [Password]=@password",
                    new SqlParameter("@login", authentication.Login),
                    new SqlParameter("@password", authentication.Credential));

                if(dataTable != null && dataTable.Rows.Count > 0)
                {
                    User user = await DbHelper.FindEntity<User>(Connector, Convert.ToInt32(dataTable.Rows[0]["UserID"]));
                    if (user != null && user.UserID > 0)
                    {
                        return Success(user);
                    }
                }
                return Success(default(User));
                //throw new Exception("错误的用户名或口令");
            }
            catch (Exception ex)
            {
                return Error<User>(ex.Message);
            }
        }
    }
}