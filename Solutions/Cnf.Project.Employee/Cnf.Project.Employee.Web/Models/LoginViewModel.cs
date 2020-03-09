using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Cnf.Project.Employee.Web.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage="必须输入登录名")]
        [Display(Name ="登录名")]
        public string UserName { get; set; }

        [DataType(DataType.Password)]
        //[Required]
        [Display(Name ="口令")]
        public string Password { get; set; }

        public bool HasChecked { get; set; }
    }
}
