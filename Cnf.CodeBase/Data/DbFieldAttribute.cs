using System;
using System.Data;
using System.Collections.Generic;
using System.Text;

namespace Cnf.CodeBase.Data
{
    /// <summary>
    /// 用于标记一个类的字段或属性的特性，说明该字段或属性可用于和数据库中的字段建立映射。
    /// 缺省情况下，映射覆盖全部用途，即Usage=DbFieldUsage.All。
    /// 要改变用途，可设置Usage命名参数。
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public sealed class DbFieldAttribute : Attribute
    {
        SqlDbType? _fieldType = null;
        int? _size = null;

        public DbFieldAttribute(string fieldName)
        {
            FieldName = fieldName;
            Usage = DbFieldUsage.All;
        }

        #region Properties

        /// <summary>
        /// The column name of the field in database
        /// </summary>
        public string FieldName { get; }

        public bool TypeDefined
        { get { if (_fieldType.HasValue) return true; else return false; } }

        public bool SizeDefined
        { get { if (_size.HasValue) return true; else return false; } }

        // The type of the field, if NULL, use the DataType of the member to map a SqlDbType
        public SqlDbType FieldType
        {
            get { return _fieldType.Value; }
            set { _fieldType = value; }
        }

        /// <summary>
        /// 读取和设置本字段是否为实体在数据库表中的主键（某些自动化操作只支持自增长整型标识的主键）
        /// </summary>
        public bool IsPrimaryKey { get; set; }

        /// <summary>
        /// 如果本字段是外键，那么在转化值得时候会做NULL处理（如果值小于等于0就设置DBNull，只支持Int型外键）
        /// </summary>
        public bool IsForeignKey { get; set; }

        /// <summary>
        /// 读取和设置字段的长度，主要用于字符型数据。
        /// </summary>
        public int Size
        {
            get { return _size.Value; }
            set { _size = value; }
        }

        /// <summary>
        /// 读取或者设置该字段的用途，三种可用位或操作设置的Flags。分别为：
        /// 1. 用于从DataTable中填充实体；2. 用于Insert新数据；3.用于Update旧数据。
        /// 默认为全部
        /// </summary>
        public DbFieldUsage Usage { get; set; }

        #endregion
    }
}
