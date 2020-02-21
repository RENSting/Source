using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cnf.Api;
using Cnf.Project.Employee.Entity;
using Microsoft.Extensions.Configuration;
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

        public async Task<TReturn> HttpGet<TReturn>(string route, string queryString) where TReturn:new()
        {
            var apiResult = await _connector.GetAsync<TReturn>(route + "?" + queryString);
            if (apiResult.IsSuccess)
                return apiResult.GetData();
            else
                throw new Exception(apiResult.Message);
        }

        public async Task<TReturn> HttpPost<T, TReturn>(string route, T data) 
            where T : new() where TReturn: new()
        {
            var apiResult = await _connector.PostAsync<T, TReturn>(route, data);
            if (apiResult.IsSuccess)
                return apiResult.GetData();
            else
                throw new Exception(apiResult.Message);
        }
    }

    public class UserManger : IUserManager
    {
        const string ROUTE_AUTHENTICATE = "api/User/Authenticate";

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

        public Task<bool> ChangeCredential(int userId, string oldCredential, string newCredential)
        {
            throw new NotImplementedException();
        }

        public Task<User> GetUser(int id)
        {
            throw new NotImplementedException();
        }

        public Task<User[]> GetUsers()
        {
            throw new NotImplementedException();
        }

        public Task<bool> SaveUser(User user)
        {
            throw new NotImplementedException();
        }
    }
}
