using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Cnf.CodeBase.Data;
using Cnf.DataAccess.SqlServer;
using Cnf.Project.Employee.Entity;
using System.Threading.Tasks;

namespace TestLab.TestCommon
{
    public class TestSqlServerConnector
    {
        readonly DbConnector _connector;
        public TestSqlServerConnector()
        {
            string conn = "data source=(local);initial catalog=d_test;UID=sa;PWD=rdc;min pool size=4";
            _connector = new DbConnector(connectString: conn);
        }

        public async void Start()
        {
            //InsertPerson();

            Person person = await GetPersonByID(2);
            Console.WriteLine(Cnf.CodeBase.Serialize.SerializationHelper.JsonSerialize(person));

            //UpdatePerson(2);

            if (await SelectPersonsAsync())
                Console.WriteLine("Query Table Method is good.");
            else
                Console.WriteLine("Query table failed.");

            if (await SelectPersonSetAsync())
                Console.WriteLine("Query DataSet 方法正确。");
            else
                Console.WriteLine("Query DataSet 方法错误。");
        }

        private async void InsertPerson()
        {
            Person person = new Person()
            {
                Name = $"ren{new Random().Next()}",
                CreatedBy = 1,
                CreatedOn = DateTime.Now,
                ActiveStatus = true
            };

            int id = await DbHelper.InsertEntity(_connector, person);

            Console.WriteLine($"Have inserted a new person {person.Name}, his id = {id}.");
        }

        async Task<Person> GetPersonByID(int id)
        {
            return await DbHelper.FindEntity<Person>(_connector, id);
        }

        async void UpdatePerson(int id)
        {
            Person person = await GetPersonByID(id);
            person.CreatedBy = 3;
            person.Name = "REN Sting";

            await DbHelper.UpdateEntity(_connector, person);

            Console.WriteLine($"Have updated person's name into {person.Name}, his id = {id}.");
        }

        private async Task<bool> SelectPersonsAsync()
        {
            string sql = "SELECT * FROM person";

            DataTable result = await _connector.ExecuteSqlQueryTable(sql);
            if (result.TableName.Equals("result")
                && result.Rows.Count > 0)
                return true;
            else
                return false;
        }

        private async Task<bool> SelectPersonSetAsync()
        {
            string sql = @"
SELECT count(1) AS [ROWS] FROM person;
SELECT * FROM person;
";

            DataSet result = await _connector.ExecuteSqlQuerySet(sql);
            int count = (int)result.Tables[0].Rows[0]["ROWS"];
            Console.WriteLine($"person 表现有记录{count.ToString()}行。");
            if (count == result.Tables[1].Rows.Count)
                return true;
            else
                return false;
        }
    }
}
