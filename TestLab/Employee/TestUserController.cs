using Cnf.Api;
using Cnf.CodeBase.Secure;
using Cnf.CodeBase.Serialize;
using Cnf.Project.Employee.Entity;
using System;
using System.Threading.Tasks;

namespace TestLab.Employee
{
    class TestUserController
    {
        readonly WebConnector _connector;

        public TestUserController()
        {
            _connector = new WebConnector("https://localhost:5001");

        }

        internal async Task Start()
        {
            try
            {
                //CreateNewUser();
                //CreateFirstUser();
                await CreateNewUserAsync();

                //PrintSysAdmin();

                //CheckCredential();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                throw;
            }

        }

        //private async void PrintSysAdmin()
        //{
        //    User user = (await _connector.Get<User>("api/User/GetUserById?userId=1")).GetData();
        //    if(user != null && user.UserID > 0)
        //    {
        //        Console.WriteLine($"System Administrator: " +
        //            $"Login={user.Login}, Name={user.Name}");
        //    }
        //    else
        //    {
        //        Console.WriteLine("Not found sys admin");
        //    }
        //}

        //private async void CheckCredential()
        //{
        //    Authentication authentication = new Authentication()
        //    {
        //        Login = "admin",
        //        Credential = CryptoHelper.CreateCredential("wrong172")
        //    };

        //    ApiResult<User> apiResult = await _connector.Post<Authentication, User>("api/User/Authenticate", authentication);
        //    if(apiResult.IsSuccess)
        //    {
        //        Console.WriteLine("验证用户登录密码的功能有bug，没有提示错误");
        //    }
        //    else
        //    {
        //        Console.WriteLine("验证口令错误的功能实现正确");
        //    }

        //    authentication.Credential = CryptoHelper.CreateCredential("sting163");
        //    apiResult = await _connector.Post<Authentication, User>("api/User/Authenticate", authentication);
        //    if (apiResult.IsSuccess)
        //    {
        //        Console.WriteLine("验证口令错误的功能实现正确");
        //    }
        //    else
        //    {
        //        Console.WriteLine("验证用户登录密码的功能有bug，没有通过正确口令");
        //    }
        //}

        //private async void CreateFirstUser()
        //{
        //    PagedQuery<User> pagedQuery = (await _connector.Get<PagedQuery<User>>("api/User/GetUsers")).GetData();
        //    if(pagedQuery.Total == 0)
        //    {
        //        User admin = new User()
        //        {
        //            ActiveStatus = true,
        //            Login = "admin",
        //            Name = "系统管理员",
        //            Role = (int)RoleEnum.SystemAdmin
        //        };

        //        ApiResult<int> apiResult = await _connector.Post<User, int>("api/User/SaveUser", admin);
        //        int userId = apiResult.GetData();

        //        //change credential
        //        ChangeCredential changeCredential = new ChangeCredential()
        //        {
        //            UserID = userId,
        //            OldCredential = string.Empty,
        //            NewCredential = CryptoHelper.CreateCredential("sting163")
        //        };

        //        bool success = (await _connector.Post<ChangeCredential, bool>("api/User/ChangeCredential", changeCredential)).GetData();
        //        if (success)
        //            Console.WriteLine(" system administrator has been created.");
        //    }
        //}

        void CreateNewUser()
        {
            Random r = new Random();
            User newUser = new User()
            {
                ActiveStatus = true,
                Login = "u" + r.Next(10000),
                Name = "CH" + r.Next(10000),
                Role = (int)RoleEnum.HumanResourceAdmin
            };

            ApiResult<int> apiResult = _connector.Post<User, int>("api/User/SaveUser", newUser);
            if (apiResult.IsSuccess)
            {
                Console.WriteLine("Create a new user: id=" + apiResult.GetData().ToString());
                //change credential
                ChangeCredential changeCredential = new ChangeCredential()
                {
                    UserID = apiResult.GetData(),
                    OldCredential = string.Empty,
                    NewCredential = CryptoHelper.CreateCredential("P@55w0rd")
                };

                ApiResult<bool> setPwdResult = _connector.Post<ChangeCredential, bool>("api/User/ChangeCredential", changeCredential);
                if (setPwdResult.IsSuccess && setPwdResult.GetData())
                {
                    Console.WriteLine("the password for this user has been set.");
                }
                else
                {
                    Console.WriteLine("set password failed.");
                }
            }
            else
            {
                Console.WriteLine("Create new user failed, err=" + apiResult.Message);
            }
        }

        async Task CreateNewUserAsync()
        {
            Random r = new Random();
            User newUser = new User()
            {
                ActiveStatus = true,
                Login = "u" + r.Next(10000),
                Name = "CH" + r.Next(10000),
                Role = (int)RoleEnum.HumanResourceAdmin
            };

            ApiResult<int> apiResult = await _connector.PostAsync<User, int>("api/User/SaveUser", newUser);
            if (apiResult.IsSuccess)
            {
                Console.WriteLine("Create a new user: id=" + apiResult.GetData().ToString());
                //change credential
                ChangeCredential changeCredential = new ChangeCredential()
                {
                    UserID = apiResult.GetData(),
                    OldCredential = string.Empty,
                    NewCredential = CryptoHelper.CreateCredential("P@55w0rd")
                };

                ApiResult<bool> setPwdResult = await _connector.PostAsync<ChangeCredential, bool>("api/User/ChangeCredential", changeCredential);
                if (setPwdResult.IsSuccess && setPwdResult.GetData())
                {
                    Console.WriteLine("the password for this user has been set.");
                }
                else
                {
                    Console.WriteLine("set password failed.");
                }
            }
            else
            {
                Console.WriteLine("Create new user failed, err=" + apiResult.Message);
            }
        }
    }
}
