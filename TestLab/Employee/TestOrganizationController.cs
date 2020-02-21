using Cnf.Api;
using Cnf.CodeBase.Serialize;
using Cnf.Project.Employee.Entity;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TestLab.Employee
{
    class TestOrganizationController
    {
        readonly WebConnector _connector;

        public TestOrganizationController()
        {
            _connector = new WebConnector("https://localhost:5001");

        }

        internal async Task Start()
        {
            InsertNewOrg();

            await Delete(1);
        }

        async Task Delete(int id)
        {
            ApiResult<bool> apiResult = await _connector.PostAsync<int, bool>("api/Organization/Delete", id);
            if(apiResult.IsSuccess)
            {
                Console.WriteLine("成功删除了ID=" + id.ToString() + "的组织");
            }
            else
            {
                Console.WriteLine(apiResult.Message);
            }
        }

        async void InsertNewOrg()
        {
            Organization organization = new Organization()
            {
                ActiveStatus = true,
                CreatedBy = 1,
                Name = "O" + new Random().Next(1000)
            };

            ApiResult<int> apiResult = await _connector.PostAsync<Organization, int>("api/Organization/Save", organization);

            if (apiResult.IsSuccess)
            {
                Console.WriteLine("organization id:" + apiResult.GetData().ToString());
            }
            else
            {
                Console.WriteLine("organization save failed, " + apiResult.Message);
            }

        }
    }
}
