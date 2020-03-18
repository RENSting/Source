using System;
using System.Linq;
using System.Threading.Tasks;
using Cnf.Api;
using Cnf.CodeBase.Serialize;
using Cnf.Project.Employee.Entity;
using Microsoft.Extensions.Options;

namespace Cnf.Project.Employee.Web.Models
{
    public class WebApiConnector : IApiConnector
    {
        private readonly WebConnector _connector;

        public WebApiConnector(IOptionsSnapshot<WebConnectorSettings> options)
        {
            _connector = new WebConnector(
                options.Value.Applications.First(app => app.Name == WebConnector.DEFAULT).BaseUrl);
        }

        public async Task<TReturn> HttpGet<TReturn>(string route, string queryString="") where TReturn:new()
        {
            string fullUrl = route + 
                (string.IsNullOrWhiteSpace(queryString)?"":
                    ("?"+queryString));
            var apiResult = await _connector.GetAsync<TReturn>(fullUrl);
            if (apiResult.IsSuccess)
                return apiResult.GetData();
            else
                throw new Exception(apiResult.Message);
        }

        public async Task<TReturn> HttpPost<T, TReturn>(string route, T data=default(T)) 
            where TReturn: new()
        {
            var apiResult = await _connector.PostAsync<T, TReturn>(route, data);
            if (apiResult.IsSuccess)
                return apiResult.GetData();
            else
                throw new Exception(apiResult.Message);
        }

        // POST api/Project/Verify json({EmployeeID, ProjectID, DutyID})

        public async Task<ApiResult<int>> VerifyTransfer(int projectId, int dutyId, int employeeId)
        {
            var data = new TransferInfo{
                EmployeeId = employeeId, DutyId = dutyId, ProjectId = projectId,
            };
            
            return await _connector.PostAsync<TransferInfo, int>("api/Project/Verify", data);
        }
    }

    public class UserManger : IUserManager
    {
        const string ROUTE_AUTHENTICATE = "api/User/Authenticate";
        const string ROUTE_GET_USERS = "api/User/GetUsers";
        const string ROUTE_GET_USER_ID = "api/User/GetUserById"; // add /{userId}
        const string ROUTE_CHANGE_CREDENTIAL = "api/User/ChangeCredential";
        const string ROUTE_SAVE_USER = "api/User/Save";
        const string ROUTE_GET_USER_LOGIN = "api/User/GetUserByLogin";
        const string ROUTE_DELETE_USER = "api/User/Delete";

        readonly IApiConnector _connector;

        public UserManger(IApiConnector connector)
        {
            _connector = connector;
        }

        public Task<User> Authenticate(string userName, string credential)
        {
            var data = new Authentication()
            {
                Login = userName,
                Credential = credential
            };

            return _connector.HttpPost<Authentication, User>(ROUTE_AUTHENTICATE, data);
        }

        public async Task<bool> ChangeCredential(ChangeCredential credential)
        {
            return await _connector.HttpPost<ChangeCredential, bool>(ROUTE_CHANGE_CREDENTIAL, credential);
        }

        public async Task<User> GetUser(int id)
        {
            return await _connector.HttpGet<User>(ROUTE_GET_USER_ID + $"/{id}");
        }

        public async Task<User> GetUserByLogin(string login)
        {
            return await _connector.HttpPost<string, User>(ROUTE_GET_USER_LOGIN, login);
        }

        public async Task<User[]> GetUsers()
        {
            PagedQuery<User> apiResult = await _connector.HttpGet<PagedQuery<User>>(ROUTE_GET_USERS);
            return apiResult.Records;
        }

        public async Task<bool> SaveUser(User user)
        {
            int id = await _connector.HttpPost<User, int>(ROUTE_SAVE_USER, user);
            if(id > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> DeleteUser(int userId)
        {
            try
            {
                await _connector.HttpPost<int, bool>(ROUTE_DELETE_USER, userId);
                return true;    
            }
            catch
            {
                return false;
            }
            
        }
    }
}
