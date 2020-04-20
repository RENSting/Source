using System;
using System.ComponentModel.DataAnnotations;

namespace Cnf.Project.Employee.Web.Models
{
    public enum ProjectState
    {
        [Display(Name = "准备阶段")]
        Preparing,
        [Display(Name = "进行中")]
        Processing,
        [Display(Name = "已竣工")]
        Completed,
        [Display(Name = "已关闭")]
        Closed
    }

    public class ProjectViewState
    {
        public int ProjectId { get; set; }

        //[Display(Name = "有效性")]
        //public bool ActiveStatus { get; set; }

        [Display(Name = "项目状态")]
        public ProjectState State { get; set; }

        [Display(Name = "项目名称")]
        [Required(ErrorMessage = "必须输入项目的全称")]
        public string FullName { get; set; }

        [Display(Name = "简称"), Required(ErrorMessage = "必须为项目提供一个简称")]
        [MaxLength(50, ErrorMessage = "项目简称不能多于50个字")]
        public string ShortName { get; set; }

        [Display(Name = "工程地点"), Required(ErrorMessage = "必须输入项目工程施工所在地")]
        [MaxLength(200, ErrorMessage = "所在地名称不能超过200个字")]
        public string SitePlace { get; set; }

        [Display(Name = "甲方名称"), Required(ErrorMessage = "必须输入项目甲方的名称")]
        [MaxLength(200, ErrorMessage = "甲方名称不能超过200个字")]
        public string Owner { get; set; }

        [Display(Name = "主合同编号"), Required(ErrorMessage = "必须输入项目主合同编号")]
        [MaxLength(200, ErrorMessage = "合同编号不能超过200位")]
        public string ContractCode { get; set; }

        [Display(Name = "合同金额")]
        [RegularExpression("^[0-9]+(\\.[0-9]{1,3})?$", ErrorMessage = "必须输入正实数")]
        public double? ContractAmount { get; set; }

        [Display(Name = "开工日期"), Required(ErrorMessage = "必须输入开工日期或预计开工日期")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime BeginDate { get; set; }

        [Display(Name = "完工日期"), Required(ErrorMessage = "必须输入完工日期或预计完工日期")]
        [DataType(DataType.Date), DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        public DateTime EndDate { get; set; }

        public static implicit operator ProjectViewState(Entity.Project project) =>
            new ProjectViewState
            {
                //ActiveStatus = project.ActiveStatus,
                BeginDate = project.StartTime,
                ContractCode = project.ContractCode,
                ContractAmount = project.ContractAmount == 0 ? default(double?) : project.ContractAmount,
                EndDate = project.EndTime,
                FullName = project.FullName,
                Owner = project.Owner,
                ProjectId = project.ID,
                ShortName = project.ShortName,
                SitePlace = project.SitePlace,
                State = (ProjectState)project.Status,
            };
    }
}