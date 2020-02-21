namespace Cnf.CodeBase.Serialize
{
    /// <summary>
    /// 分页查询的返回结果，包括总记录数，当前页记录数组
    /// </summary>
    /// <typeparam name="T">数组元素数据类型，必须具有空白构造函数</typeparam>
    public class PagedQuery<T> where T:new()
    {
        /// <summary>
        /// 满足条件的总记录数
        /// </summary>
        public int Total { get; set; }
        /// <summary>
        /// 当前页包含的记录
        /// </summary>
        public T[] Records { get; set; }
    }
}
