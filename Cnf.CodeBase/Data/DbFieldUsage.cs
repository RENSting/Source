using System;
using System.Collections.Generic;
using System.Text;

namespace Cnf.CodeBase.Data
{
    /// <summary>
    /// 用于定义数据访问帮助类将如何使用该成员的枚举。
    /// 该枚举值可以使用“或”操作和“与”操作连接。
    /// </summary>
    [Flags]
    public enum DbFieldUsage
    {
        /// <summary>
        /// The field/property won't be used, it's just same as that you haven't tagged the DbField attribute
        /// </summary>
        None = 0,
        /// <summary>
        /// When use DbHelper.FulfillEntity(), the field/property will be set
        /// </summary>
        MapResultTable = 1,
        /// <summary>
        /// the field/property will be used as a SQL parameter when inserting a new row in DB.
        /// DbHelper.ParseSqlParametersForInsert() refers it
        /// </summary>
        InsertParameter = 2,
        /// <summary>
        /// the field/property will be used as a SQL parameter when updating an existing row in DB.
        /// DbHelper.ParseSqlParametersForUpdate() refers it
        /// </summary>
        UpdateParameter = 4,
        /// <summary>
        /// The field/property will be used in all above situations
        /// </summary>
        All = 7,
    }
}
