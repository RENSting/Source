using Cnf.Project.Employee.Entity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cnf.Project.Employee.Web
{
    public static class UserHelper
    {
        const string COOKIE_USERID = "userid";
        const string COOKIE_USERNAME = "name";
        const string COOKIE_ROLE = "role";

        static int GetRole(HttpContext context) => Convert.ToInt32(context.User.FindFirstValue(COOKIE_ROLE));

        public static int GetUserID(HttpContext context) => Convert.ToInt32(context.User.FindFirstValue(COOKIE_USERID));

        public static string GetUserName(HttpContext context) => context.User.FindFirstValue(COOKIE_USERNAME);
        public static bool IsSystemAdmin(int role) =>
            ((RoleEnum)role & RoleEnum.SystemAdmin) == RoleEnum.SystemAdmin;
        public static bool IsSystemAdmin(HttpContext context) => IsSystemAdmin(GetRole(context));

        public static bool IsHumanResourceAdmin(int role) =>
            ((RoleEnum)role & RoleEnum.HumanResourceAdmin) == RoleEnum.HumanResourceAdmin;
        public static bool IsHumanResourceAdmin(HttpContext context) => IsHumanResourceAdmin(GetRole(context));

        public static bool IsProjectAdmin(int role) =>
            ((RoleEnum)role & RoleEnum.ProjectAdmin) == RoleEnum.ProjectAdmin;
        public static bool IsProjectAdmin(HttpContext context) => IsProjectAdmin(GetRole(context));

        public static bool IsManager(int role) =>
            ((RoleEnum)role & RoleEnum.Manager) == RoleEnum.Manager;
        public static bool IsManager(HttpContext context) => IsManager(GetRole(context));

        public static async Task Signin(User user, HttpContext context)
        {
            var claims = new List<Claim>
                {
                    new Claim(UserHelper.COOKIE_USERID, user.UserID.ToString()),
                    new Claim(UserHelper.COOKIE_USERNAME, user.Name),
                    new Claim(UserHelper.COOKIE_ROLE, user.Role.ToString())
                };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var properties = new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(20),
            };

            var principal = new ClaimsPrincipal(identity);

            await context.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal,properties);

        }

        public static void Signout(HttpContext context)
        {
            context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
    }
}
