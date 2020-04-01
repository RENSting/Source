using System;
using System.Text;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Cnf.Project.Employee.Entity;

namespace Cnf.Project.Employee.Web.Models
{
    public enum DutyCategoryEnum
    {
        [Display(Name = "项目班子")]
        LeadingGroup = 1,
        [Display(Name = "其他")]
        Others = 100,
    }

    /// <summary>
    /// 岗位职责视图模型
    /// </summary>
    public class DutyViewMode
    {
        public const char LEADING_GROUP = 'A';
        public const char OTHERS = 'Z';

        [Display(Name = "分类")]
        public DutyCategoryEnum Category { get; set; }

        [Display(Name = "岗位编号")]
        [Required(ErrorMessage = "必须指定一个岗位编号")]
        [MaxLength(40, ErrorMessage = "代号最长50个字符")]
        public string NativeCode { get; set; }

        [Display(Name = "岗位职责")]
        [Required(ErrorMessage = "必须输入名称")]
        [MaxLength(200, ErrorMessage = "名称最长200字符")]
        public string Name { get; set; }

        public int DutyId { get; set; }

        [Display(Name = "是否有效")]
        public bool IsActive { get; set; }

        public static implicit operator DutyViewMode(RefViewMode r) =>
            new DutyViewMode
            {
                Category = r.Code?.StartsWith(LEADING_GROUP) == true ?
                        DutyCategoryEnum.LeadingGroup :
                        DutyCategoryEnum.Others,
                NativeCode = string.IsNullOrEmpty(r.Code) ? "" : r.Code.Substring(1),
                Name = r.Name,
                DutyId = r.ID,
                IsActive = r.ActiveStatus,
            };

        public static implicit operator DutyViewMode(Reference r) =>
            r.Type != ReferenceTypeEnum.Duty ? throw (new Exception("wrong type")) : (RefViewMode)r;

        public static string GenerateCategoriesQuery(IEnumerable<DutyCategoryEnum> categories)
        {
            StringBuilder sb = new StringBuilder();
            if (categories != null)
            {
                foreach (var c in categories)
                {
                    sb.Append(GetPrefix(c));
                }
            }
            return sb.ToString();
        }

        public static char GetPrefix(DutyCategoryEnum category) =>
            category == DutyCategoryEnum.LeadingGroup ? LEADING_GROUP
                    : OTHERS;
    }

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
                type == ReferenceTypeEnum.Specialty ? "专业" :
                type == ReferenceTypeEnum.Duty ? "职责" :
                type == ReferenceTypeEnum.Qualification ? "资格类型" :
                        throw (new Exception("Error reference type"));

        public static ReferenceTypeEnum Parse(string type) =>
                type == "专业" ? ReferenceTypeEnum.Specialty :
                type == "职责" ? ReferenceTypeEnum.Duty :
                type == "资格类型" ? ReferenceTypeEnum.Qualification :
                        throw (new Exception("Error refenrect type name"));
    }

    public class DutyQualifViewModel
    {
        public int DutyId { get; set; }
        /// <summary>
        /// 用逗号（,）分割的资格类型ID字符串，表示选中的资格类型。
        /// </summary>
        /// <value></value>
        public string QualifIds { get; set; }
    }
}