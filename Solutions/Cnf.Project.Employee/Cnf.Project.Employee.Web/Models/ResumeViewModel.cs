using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Cnf.Project.Employee.Entity;

namespace Cnf.Project.Employee.Web.Models
{
    public class ResumeViewModel
    {
        public int EmployeeID{get;set;}
        public string EmployeeName{get;set;}
        [Display(Name="职务")]
        public string Title{get;set;}
        [Display(Name="单位名称")]
        public string OrganizationName{get;set;}
        [Display(Name="专业")]
        public string SpecialtyName{get;set;}
        [Display(Name="是否可用")]
        public bool ActiviteStatus{get;set;}
        [Display(Name="所在项目")]
        public string CurrentProjectName{get;set;}
        [Display(Name="承担职责")]
        public string CurrentDutyName{get;set;}

        public CertViewModel[] Certifications{get;set;}
        public TransferLog[] Logs{get;set;}
    }

    public enum TransferLogType
    {
        [Display(Name="入场")]
        In, 
        [Display(Name="离场")]
        Out,
    }

    public class TransferLog
    {
        [Display(Name="动作")]
        public TransferLogType LogType{get;set;}
        [Display(Name="日期")]
        [DisplayFormat(DataFormatString="{0:yyyy-MM-dd}")]
        public DateTime Date{get;set;}
        public string FromProject{get;set;}
        public string FromDuty{get;set;}
        public string ToProject{get;set;}
        public string ToDuty{get;set;}
        public DateTime TimeStamp{get;set;}
    }
}