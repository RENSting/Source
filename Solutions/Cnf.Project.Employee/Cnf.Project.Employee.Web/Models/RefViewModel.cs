using System;
using System.ComponentModel.DataAnnotations;
using Cnf.Project.Employee.Entity;

namespace Cnf.Project.Employee.Web.Models
{
    public class RefViewMode : EntityViewModel
    {
        [Display(Name = "代号")]
        [Required(ErrorMessage = "必须指定一个代号")]
        [MaxLength(50, ErrorMessage = "代号最长50个字符")]
        public string Code { get; set; }

        [Display(Name = "名称")]
        [Required(ErrorMessage = "必须输入名称")]
        [MaxLength(200, ErrorMessage = "名称最长200字符")]
        public string Name { get; set; }

        public string Type { get; set; }

        public static implicit operator RefViewMode(Reference reference) =>
            new RefViewMode
            {
                Code = reference.ReferenceCode,
                Name = reference.ReferenceValue,
                ID = reference.ID,
                ActiveStatus = reference.ActiveStatus,
                Type = Translate(reference.Type)
            };

        public static string Translate(ReferenceTypeEnum type) =>
                type == ReferenceTypeEnum.Specialty? "专业":
                type == ReferenceTypeEnum.Duty? "职责":
                type == ReferenceTypeEnum.Qualification? "资格类型":
                        throw(new Exception("Error reference type"));

        public static ReferenceTypeEnum Parse(string type) => 
                type == "专业" ? ReferenceTypeEnum.Specialty :
                type == "职责" ? ReferenceTypeEnum.Duty :
                type == "资格类型"? ReferenceTypeEnum.Qualification:
                        throw(new Exception("Error refenrect type name"));
    }
}