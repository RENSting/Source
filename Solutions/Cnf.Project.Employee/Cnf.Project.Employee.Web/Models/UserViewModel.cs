using System;
using System.ComponentModel.DataAnnotations;
using Cnf.Project.Employee.Entity;
using Microsoft.AspNetCore.Mvc;

namespace Cnf.Project.Employee.Web.Models
{
    public class UserViewModel
    {
        public int UserId{get;set;}

        [Required(ErrorMessage="必须输入登录帐号")]
        [Display(Name="登录名")]
        [RegularExpression("^[a-zA-Z][a-zA-Z0-9_]{1,15}$",
            ErrorMessage="登录名必须字母开头，只能由字母数字下划线组成，长度2-16")
        ]
        [Remote("ValidateLogin", "System", AdditionalFields="UserId", ErrorMessage="登录名不能重复")]
        public string Login{get;set;}

        [Required(ErrorMessage="必须输入用户姓名")]
        [Display(Name="姓名")]
        public string Name{get;set;}

        [Display(Name="系统管理员")]
        public bool IsSysAdmin{get;set;}

        [Display(Name="人事主管")]
        public bool IsHumanResourceAdmin{get;set;}

        [Display(Name="项目主管")]
        public bool IsProjectAdmin{get;set;}

        [Display(Name="经理")]
        public bool IsManager{get;set;}

        [Display(Name="是否有效")]
        public bool ActiveStatus{get;set;}

        public static implicit operator UserViewModel(User user) =>
            new UserViewModel
            {
                ActiveStatus = user.ActiveStatus,
                IsHumanResourceAdmin = UserHelper.IsHumanResourceAdmin(user.Role),
                IsManager = UserHelper.IsManager(user.Role),
                IsProjectAdmin = UserHelper.IsProjectAdmin(user.Role),
                IsSysAdmin = UserHelper.IsSystemAdmin(user.Role),
                Login = user.Login,
                Name = user.Name,
                UserId = user.UserID
            };
    }
}