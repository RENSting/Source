namespace Cnf.Project.Employee.Entity
{
    /// <summary>
    /// 为搜索人员而定义的条件模型（API参数）
    /// </summary>
    public class CriteriaForEmployee : SearchCriteria
    {
        /// <summary>
        /// 查找人员所在的项目
        /// null: 忽略； 0：仅自由人员（未分配项目）；n：项目ID=n的项目中的人员
        /// </summary>
        /// <value></value>
        public int? SelectedProj { get; set; }
        /// <summary>
        /// 查找人员所属的单位
        /// </summary>
        /// <value></value>
        public int? SelectedOrg { get; set; }
        /// <summary>
        /// 查找人员所属的专业
        /// </summary>
        /// <value></value>
        public int? SelectedSpec { get; set; }
    }
}