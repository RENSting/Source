using System;
using System.Threading.Tasks;

namespace TestLab
{
    class Program
    {
        static async Task Main(string[] args)
        {            
            Console.WriteLine("Start Test...");

            //TaskScheduler scheduler = TaskScheduler.Current
            //Employee.TestUserController t1 = new Employee.TestUserController();
            //await t1.Start();

            //Employee.TestOrganizationController t2 = new Employee.TestOrganizationController();
            //await t2.Start();

            Employee.TestReferenceController t3 = new Employee.TestReferenceController();
            await t3.Start();
                 
            Console.WriteLine($"Tests end at {DateTime.Now.ToShortTimeString()}");
        }

        static void TestCommon()
        {
            TestCommon.TestCryptor t1 = new TestCommon.TestCryptor();
            t1.Start();

            TestCommon.TestSqlServerConnector t2 = new TestCommon.TestSqlServerConnector();
            t2.Start();

            TestCommon.TestSerialize t3 = new TestCommon.TestSerialize();
            t3.Start();

            //TestCommon.TestApi t4 = new TestCommon.TestApi();
            //t4.Start();

        }
    }
}
