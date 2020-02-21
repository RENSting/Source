using Cnf.Api;
using Cnf.CodeBase.Serialize;
using Cnf.Project.Employee.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TestLab.TestCommon
{
    //class TestApi
    //{
    //    readonly WebConnector _connector;

    //    public TestApi()
    //    {
    //        _connector = new WebConnector("https://localhost:5001");
    //    }

    //    internal async void Start()
    //    {
    //        PagedQuery<User> pagedQuery = await GetUsers();
    //        Console.WriteLine($"用户包括{pagedQuery.Total.ToString()}个");

    //        User user = await GetUser();
    //        Console.WriteLine($"用户UserID=1的记录为：{SerializationHelper.JsonSerialize(user)}");
    //    }

    //    async Task<PagedQuery<User>> GetUsers()
    //    {
    //        ApiResult<PagedQuery<User>> apiResult = await _connector.Get<PagedQuery<User>>("api/User/GetUsers");

    //        if (apiResult.IsSuccess)
    //        {
    //            return apiResult.GetData();
    //        }
    //        else
    //        {
    //            throw new Exception(apiResult.Message);
    //        }
    //    }

    //    async Task<User> GetUser()
    //    {
    //        ApiResult<User> apiResult = await _connector.Get<User>("api/User/GetUserById?userId=1");
    //        if(apiResult.IsSuccess )
    //        {
    //            return apiResult.GetData();
    //        }
    //        else
    //        {
    //            throw new Exception(apiResult.Message);
    //        }
    //    }
    //}
}
