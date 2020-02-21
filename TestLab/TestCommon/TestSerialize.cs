using System;
using System.Collections.Generic;
using System.Text;
using Cnf.CodeBase.Serialize;
using Cnf.Project.Employee.Entity;

namespace TestLab.TestCommon
{
    class TestSerialize
    {
        User _user;
        readonly Person _person = new Person() { Name = "REN Sting", ActiveStatus = false, CreatedBy = 1, CreatedOn = DateTime.Now };

        public TestSerialize()
        {
            _user = new User()
            {
                UserID = 1,
                Name = "REN Sting",
                Login = "rcc",
                Role = 5,
            };
        }

        public void Start()
        {
            Specialty specialty = new Specialty("201", "焊接", 2);
            Console.WriteLine(SerializationHelper.JsonSerialize(specialty));

            string json = TestJsonSerialize();

            TestJsonDeserialize(json);

            json = SerializationHelper.JsonSerialize(_person);
            Console.WriteLine(json);

            Person person = SerializationHelper.JsonDeserialize<Person>(json);
            Console.WriteLine(person.Name + person.CreatedOn.ToString());
        }

        public void TestJsonDeserialize(string json)
        {
            Console.WriteLine("测试 SerializationHelper.JsonDeSerialize() 方法. 结果：");
            User user = SerializationHelper.JsonDeserialize<User>(json);
            Console.WriteLine($"" +
                $"UserID={user.UserID},\n" +
                $"Login={user.Login},\n" +
                $"Name={user.Name},\n" +
                $"Role={user.Role},\n" +
                $"IsAdmin={user.IsSystemAdmin}");
        }

        public string TestJsonSerialize()
        {
            Console.WriteLine("测试 SerializationHelper.JsonSerialize() 方法. 结果：");
            string json = SerializationHelper.JsonSerialize(_user);
            Console.WriteLine(json);
            return json;
        }
    }
}
