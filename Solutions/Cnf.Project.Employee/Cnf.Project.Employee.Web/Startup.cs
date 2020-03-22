using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Authentication.Cookies;
using Cnf.Api;
using Cnf.Project.Employee.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;

namespace Cnf.Project.Employee.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<WebConnectorSettings>(Configuration.GetSection("WebConnectorSettings"));

            services.AddScoped<IApiConnector, WebApiConnector>();
            services.AddScoped<IUserManager, UserManger>();
            services.AddScoped<ISysAdminService, SysAdminService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IProjectService, ProjectService>();
            services.AddScoped<IAnalysisService, AnalysisService>();

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Home/Login";
                    //options.AccessDeniedPath = "/Home/Denied";
                    options.Cookie.HttpOnly = true;
                });

            services.AddControllersWithViews()
                .AddRazorRuntimeCompilation();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.Use(async (context, next) =>
          {
              //System.Text.StringBuilder builder = new System.Text.StringBuilder();
              //builder.Append($"PathBase={context.Request.PathBase}  ");
              //builder.Append($"Path={context.Request.Path} ");
              //builder.Append($"Hotst={context.Request.Host} ");
              //builder.Append($"Protocol={context.Request.Protocol} ");
              //builder.Append($"QueryString={context.Request.QueryString} ");
              //builder.Append($"Scheme={context.Request.Scheme} ");

              //await context.Response.WriteAsync(builder.ToString());

              string path = context.Request.Path.Value.ToLower();
              if (path == "/" || path.StartsWith("/home"))
              {
                  await next.Invoke();
              }
              else if (path.StartsWith("/system"))
              {
                  if (UserHelper.IsSystemAdmin(context))
                  {
                      await next.Invoke();
                  }
                  else
                  {
                      context.Response.Redirect("/Home/Denied");
                  }
              }
              else if (path.StartsWith("/employee"))
              {
                  if (UserHelper.IsHumanResourceAdmin(context))
                  {
                      await next.Invoke();
                  }
              }
              else if (path.StartsWith("/project"))
              {
                  if (path.StartsWith("/project/getqualifystate")
                        || path.StartsWith("/project/getdutyqualifications")
                        || path.StartsWith("/project/getcertsofemployee")
                        || path.StartsWith("/project/verifytransfer")
                        || path.StartsWith("/project/searchprojects"))
                  {
                      //ajax api, won't authenticate them
                      await next.Invoke();
                  }
                  else
                  {
                      if (UserHelper.IsProjectAdmin(context))
                      {
                          await next.Invoke();
                      }
                  }
              }
              else if (path.StartsWith("/dashboard"))
              {
                  if (UserHelper.IsManager(context))
                  {
                      await next.Invoke();
                  }
              }

          });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
