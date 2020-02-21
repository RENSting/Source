using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cnf.Project.Employee.Entity;

namespace Cnf.Project.Employee.Web.Models
{
    public interface IApiConnector
    {
        Task<TReturn> HttpGet<TReturn>(string route, string queryString)
            where TReturn:new();

        Task<TReturn> HttpPost<T, TReturn>(string route, T data)
            where T: new() where TReturn:new();
    }

    public interface IUserManager
    {
        /// <summary>
        /// 验证用户身份，返回符合身份的User对象实例，验证失败返回null
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="credential"></param>
        /// <returns></returns>
        Task<User> Authenticate(string userName, string credential);

        /// <summary>
        /// 返回系统中的所有User
        /// </summary>
        /// <returns></returns>
        Task<User[]> GetUsers();

        /// <summary>
        /// 根据UserID=id返回User对象实例。没有找到返回null
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<User> GetUser(int id);

        /// <summary>
        /// 将User实例user保存到持久化存储
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        Task<bool> SaveUser(User user);

        /// <summary>
        /// 修改User的登录口令
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="oldCredential"></param>
        /// <param name="newCredential"></param>
        /// <returns></returns>
        Task<bool> ChangeCredential(int userId, string oldCredential, string newCredential);
    }
}
