using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Cnf.Project.Employee.Entity;

namespace Cnf.Project.Employee.Web.Models
{
    /// <summary>
    /// 员工是否满足项目承担职责的状态
    /// </summary>
    public enum QualifyStatus
    {
        /// <summary>
        /// 职责要求的资格证书一个也没有
        /// </summary>
        [Display(Name="无资格")]
        None,
        /// <summary>
        /// 职责要求的资格证书有部分满足
        /// </summary>
        [Display(Name="部分满足")]
        Partially,
        /// <summary>
        /// 职责要求的资格证书完全满足
        /// </summary>
        [Display(Name="完全满足")]
        Completely
    }

    public class EmployeeQualifState
    {
        public int EmployeeId{get;set;}
        public int DutyId{get;set;}
        public QualifyStatus State{get;set;}
        public Certification[] Certs{get;set;}
    }

    public class RecruitViewModel
    {
        /// <summary>
        /// 要加入的项目ID
        /// </summary>
        /// <value></value>
        public int ProjectId{get;set;}

        /// <summary>
        /// 要加入的项目的名称
        /// </summary>
        /// <value></value>
        public string ProjectName{get;set;}

        [Display(Name="仅自由人员")]
        public bool JustFreeOnly{get;set;}

        [Display(Name="所属单位")]
        public int? SelectedOrgId{get;set;}

        [Display(Name="专业")]
        public int? SelectedSpecId{get;set;}

        [Display(Name="模糊搜索")]
        public string SearchName{get;set;}

        public int PageIndex{get;set;}
        
        public int PageSize{get;set;}

        public int Total{get;set;}

        public ProjEmployeeViewModel[] Candidates{get;set;}

        public int GetPageSize() => PageSize<= 0?20:PageSize;

        public int CalcPageCount() =>
            Total==0?1:(Total%GetPageSize()==0?Total/GetPageSize():Total/GetPageSize()+1);

        /// <summary>
        /// 获得可用于API的查询条件
        /// </summary>
        /// <returns></returns>
        public CriteriaForEmployee GetCriteria() =>
            new CriteriaForEmployee{
                PageIndex = PageIndex, PageSize=GetPageSize(), SearchName = SearchName,
                SelectedOrg = SelectedOrgId, 
                SelectedSpec = SelectedSpecId,
                SelectedProj = JustFreeOnly?0:default(int?),
            };
    }

    public class TransferViewModel
    {
        public int ProjectId{get;set;}

        public string ProjectName{get;set;}

        public int DutyId{get;set;}

        public int EmployeeId{get;set;}

        public string EmployeeName{get;set;}
        
        public string SpecialityName{get;set;}
    }

    /// <summary>
    /// 项目人员的视图模型，不同于EmployeeViewModel，这个模型类完全表示人员在项目中的状态，而非用于维护人员基本信息。
    /// </summary>
    public class ProjEmployeeViewModel
    {
        /// <summary>
        /// 此属性用来在列表中选择人员（仅用于Recruit）
        /// </summary>
        /// <value></value>
        public bool Selected{get;set;}

        /// <summary>
        /// 此属性用于指定人员准备调入的岗位（仅用于Recruit）
        /// </summary>
        /// <value></value>
        public int RecruitDutyId{get;set;}

        public int EmployeeId{get;set;}

        [Display(Name="姓名")]
        public string EmployeeName{get;set;}

        public int SpecialityId{get;set;}

        [Display(Name="专业")]
        public string SpecialityName{get;set;}

        public int ProjectId{get;set;}

        [Display(Name="所在项目")]
        public string ProjectName{get;set;}

        public int DutyId{get;set;}

        [Display(Name="承担职责")]
        public string DutyName{get;set;}

        [Display(Name="加入时间")]
        [DisplayFormat(DataFormatString="{0:yyyy-MM-dd}")]
        public DateTime EnListDate{get;set;}

        public static EmployeeQualifState EvalQualif(int employeeId, int dutyId, 
            DutyQualification[] dutyQualifs, Certification[] certifications, DateTime checkDate)
        {
            var matchedCerts = new List<Certification>();
            var needQulifs = from dq in dutyQualifs
                            where dq.DutyID == dutyId
                            select dq;
            int total=needQulifs.Count(), notMatch = 0;
            foreach(var dq in needQulifs)
            {
                var qulifiedCerts = from c in certifications
                                where c.QualificationID == dq.QualificationID
                                    && c.ActiveStatus == true
                                    && c.ExpireDate >= checkDate
                                select c;
                if(qulifiedCerts.Count() <= 0) 
                    notMatch++;
                else
                    matchedCerts.AddRange(qulifiedCerts);
            }
            return new EmployeeQualifState{
                EmployeeId = employeeId, DutyId = dutyId,
                Certs = matchedCerts.ToArray(),
                State = total==0?QualifyStatus.Completely:
                        notMatch==0?QualifyStatus.Completely:
                        notMatch==total?QualifyStatus.None:QualifyStatus.Partially,
            };
        }

        public static ProjEmployeeViewModel Create(Entity.Employee employee, Reference[] specialities) =>
            new ProjEmployeeViewModel{
                DutyId = employee.InDutyID, 
                DutyName = employee.DutyName,
                EmployeeId = employee.ID,
                EmployeeName = employee.Name,
                EnListDate = employee.InDate,
                ProjectId = employee.InProjectID,
                ProjectName = employee.ProjectName,
                SpecialityId = employee.SpecialtyID,
                SpecialityName = (from s in specialities 
                                    where s.ID==employee.SpecialtyID
                                    select s.ReferenceValue).FirstOrDefault(),
                
            };
    }
}