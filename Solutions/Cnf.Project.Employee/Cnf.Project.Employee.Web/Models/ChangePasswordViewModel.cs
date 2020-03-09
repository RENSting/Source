using System;
using System.ComponentModel.DataAnnotations;
using Cnf.Project.Employee.Entity;

namespace Cnf.Project.Employee.Web.Models
{
    public class ChangePasswordViewModel
    {
        public int UserId{get;set;}

        [Display(Name="登录名")]
        public string Login{get;set;}

        [Display(Name="姓名")]
        public string Name{get;set;}

        [Display(Name="旧口令")]
        [DataType(DataType.Password)]
        public string OldPassword{get;set;}

        [Display(Name="新口令"), DataType(DataType.Password)]
        [Required(ErrorMessage="必须输入符合强密码规则的口令")]
        [RegularExpression("^(?=.*\\d)(?=.*[a-z])(?=.*[A-Z]).{8,12}$",
            ErrorMessage="必须符合强密码规则，8-12位，必须同时包含大小写字母和数字")]
        public string NewPassword{get;set;}

        [Display(Name="确认口令"), DataType(DataType.Password)]
        [Compare("NewPassword", ErrorMessage="确认口令和新口令不一致")]
        public string ConfirmPassword{get;set;}

        public static implicit operator ChangePasswordViewModel(User user) =>
            new ChangePasswordViewModel
            {
                UserId = user.UserID,
                Login = user.Login,
                Name = user.Name
            };
    }
}