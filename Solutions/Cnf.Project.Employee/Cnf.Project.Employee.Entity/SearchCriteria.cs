namespace Cnf.Project.Employee.Entity
{
    public class SearchCriteria
    {
        /// <summary>
        /// 用于模糊查询的名称，具体用于哪些字段需要具体实现来体现
        /// </summary>
        public string SearchName { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
    }
}