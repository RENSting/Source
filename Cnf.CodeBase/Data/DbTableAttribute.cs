using System;
using System.Collections.Generic;
using System.Text;

namespace Cnf.CodeBase.Data
{
    /// 用来标记一个实体类，表示该类将映射到一个数据表。
    /// 使用了该特性的实体类，可以使用DbHelper中的InsertEntity、UpdateEntity以及DeleteEntity直接完成无需存储过程的增删改操作。
    /// 
    /// 限制条件：使用该标记需要配合使用DbFieldAttribute特性，并且数据表必须具有identity特性的自增长primary key。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public class DbTableAttribute : Attribute
    {
        public DbTableAttribute(string tableName)
        {
            TableName = tableName;
        }

        #region Properties

        /// <summary>
        /// 实体类关联的数据表名称
        /// </summary>
        public string TableName { get; }

        #endregion

    }
}
