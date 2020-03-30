using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Server.Kestrel;
using System.Net;

namespace Cnf.Project.Employee.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    var config = new ConfigurationBuilder()
                        .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                        .Build();
                    // webBuilder.UseKestrel(options=>
                    // {
                    //     // options.Listen(IPAddress.Any, 4000);
                    //     options.Listen(IPAddress.Any, 5000);
                    //     // options.Listen(IPAddress.Any, 5001, options=>
                    //     // {
                    //     //     options.UseHttps();
                    //     // });
                    // });
                    webBuilder.UseConfiguration(config).UseStartup<Startup>();
                });
    }
}
