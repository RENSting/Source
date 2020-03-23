using System;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Cnf.Project.Employee.Entity;

namespace Cnf.Project.Employee.Web.Models
{
    public class CertViewModel
    {
        public int CertId { get; set; }

        [Display(Name = "证书名称"), Required(ErrorMessage = "必须输入证书名称")]
        public string CertName { get; set; }
        public int EmployeeId { get; set; }

        [Display(Name = "姓名")]
        public string EmployeeName { get; set; }
        public int QualifID { get; set; }
        public string QualifName { get; set; }

        [Display(Name = "发证机关"), Required(ErrorMessage = "必须输入发证机关")]
        [MaxLength(200, ErrorMessage="发证机关不能超过200个字")]
        public string AuthorityUnit { get; set; }

        [Display(Name="颁证日期")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        [DataType(DataType.Date)]
        public DateTime CertifyingDate { get; set; }

        [Display(Name="有效期到")]
        [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}")]
        [DataType(DataType.Date)]
        public DateTime ExpireDate { get; set; }

        [Display(Name="是否有效")]
        public bool ActiveStatus { get; set; }

        /// <summary>
        /// 正确初始化一个证书视图模型类实例。
        /// </summary>
        /// <param name="cert">证书实体</param>
        /// <param name="employee">人员实体</param>
        /// <param name="references">全部资格类型集合</param>
        /// <returns></returns>
        public static CertViewModel Create(Certification cert, Entity.Employee employee, Reference[] qulifications) =>
            new CertViewModel
            {
                ActiveStatus = cert.ActiveStatus,
                AuthorityUnit = cert.AuthorityUnit,
                CertId = cert.ID,
                CertifyingDate = cert.CertifyingDate,
                CertName = cert.Name,
                EmployeeId = cert.EmployeeID,
                EmployeeName = employee.Name,
                ExpireDate = cert.ExpireDate,
                QualifID = cert.QualificationID,
                QualifName = (qulifications.Select(q =>
                                new
                                {
                                    Id = q.ID,
                                    Name = $"{q.ReferenceValue}({q.ReferenceCode})"
                                })
                                .First(q => q.Id == cert.QualificationID)
                                ).Name
            };
    }

    public class EmployeeCertsViewModel
    {
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public CertViewModel[] Certs { get; set; }

        public int? NewCertQualifId { get; set; }
        public SelectList QualifList { get; set; }

        public string NewCertName { get; set; }
        public string NewCertAuthUnit { get; set; }

        [DataType(DataType.Date)]
        public DateTime NewCertIssueDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime NewCertExpireDate { get; set; }

        public bool NewCertActive { get; set; }

        public EmployeeCertsViewModel() { }

        /// <summary>
        /// 正确初始化一个人员证书清单视图模型类实例
        /// </summary>
        /// <param name="employee">人员实体</param>
        /// <param name="certifications">该人员拥有的全部证书实体集合</param>
        /// <param name="references">所有资格类型参照项实体集合</param>
        /// <returns></returns>
        public static EmployeeCertsViewModel Create(Entity.Employee employee,
            Certification[] certifications, Reference[] qulifications) =>
            new EmployeeCertsViewModel
            {
                EmployeeId = employee.ID,
                EmployeeName = employee.Name,
                Certs = certifications?.Select(c => CertViewModel.Create(c, employee, qulifications))
                            .ToArray(),
                QualifList = new SelectList(qulifications, nameof(Reference.ID), nameof(Reference.ReferenceValue)),
                NewCertQualifId = qulifications?.First()?.ID,
                NewCertIssueDate = DateTime.Today,
                NewCertExpireDate = DateTime.Today.AddYears(3),
                NewCertActive = true,
            };
    }
}