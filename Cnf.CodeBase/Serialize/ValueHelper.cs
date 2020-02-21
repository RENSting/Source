using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Reflection;
using Cnf.CodeBase.Data;

namespace Cnf.CodeBase.Serialize
{
    /// <summary>
    /// 用来辅助实现各种类型值之间转换和序列化所需的功能。
    /// </summary>
    public static class ValueHelper
    {
        public const int DBKEY_NULL = -1;
        public static DateTime DbDate_Null
        {
            get
            {
                return new DateTime(1900, 1, 1);
            }
        }

        /// <summary>
        /// 把一个系统类型映射为一个SqlDbType枚举，系统类型必须是基础类型，其中枚举将被映射成int，泛型类被映射成它的基础类型，如int?映射成int。
        /// </summary>
        public static SqlDbType MapSystemTypeToSqlDbType(Type systemType)
        {
            string name = "";

            if (systemType.IsValueType && systemType.IsGenericType)
            {
                //consider it is a Nullable value type
                Type t = systemType.GetGenericTypeDefinition();
                if (t.FullName == "System.Nullable`1")
                {
                    Type[] bts = systemType.GetGenericArguments();
                    name = bts[0].FullName;
                }
            }
            else
                name = systemType.FullName;

            switch (name)
            {
                case "System.Boolean":
                    return SqlDbType.Bit;
                case "System.SByte":
                case "System.Byte":
                case "System.Int16":
                case "System.Int32":
                case "System.Int64":
                case "System.UInt16":
                case "System.UInt32":
                case "System.UInt64":
                    return SqlDbType.Int;
                case "System.Decimal":
                case "System.Double":
                case "System.Single":
                    return SqlDbType.Float;
                case "System.String":
                case "System.Char":
                    return SqlDbType.NVarChar;
                case "System.Guid":
                    return SqlDbType.UniqueIdentifier;
                case "System.DateTime":
                    return SqlDbType.DateTime;
                default:
                    if (systemType.IsEnum == true)
                        return SqlDbType.Int;
                    else
                        return SqlDbType.NVarChar;
            }
        }


        /// <summary>
        /// 从实体对象中读取属性或字段值，用于给SqlParameter参数设置正确的值。
        /// 尤其是对IsForeignKey（负数或0）和DateTime类型的值(早于1900-01-01），设置DbNull.Value给参数
        /// </summary>
        /// <param name="member"></param>
        /// <param name="attr"></param>
        /// <param name="entity"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static object GetMemberValue(MemberInfo member, DbFieldAttribute attr, object entity, out Type dataType)
        {
            object obj;
            if (member.MemberType == MemberTypes.Field)
            {
                FieldInfo field = (FieldInfo)member;
                dataType = field.FieldType;
                obj = field.GetValue(entity);
            }
            else if (member.MemberType == MemberTypes.Property)
            {
                PropertyInfo property = (PropertyInfo)member;
                dataType = property.PropertyType;
                obj = property.GetValue(entity, null);
            }
            else { throw new Exception("必须是字段或者属性。"); }

            if (attr.IsForeignKey)
            {
                if (attr.FieldType == SqlDbType.BigInt 
                    || attr.FieldType == SqlDbType.Int
                    || attr.FieldType == SqlDbType.SmallInt 
                    || attr.FieldType == SqlDbType.TinyInt    
                    )
                {
                    if (Convert.ToInt32(obj) <= 0)
                        return null;
                }
            }

            if (dataType.ToString() == "System.DateTime")
            {
                if ((DateTime)obj <= new DateTime(1900, 1, 1))
                {
                    return null;
                }
            }

            return obj;
        }

        /// <summary>
        /// 从row中读取fieldName指定列的值，如果没有列或者列中是空值，那么按照dataType返回默认值
        /// </summary>
        /// <param name="row"></param>
        /// <param name="fieldName"></param>
        /// <param name="dataType"></param>
        /// <returns></returns>
        public static object GetTableRowValue(DataRow row, string fieldName, Type dataType)
        {
            bool isValid;

            if (row.Table.Columns.Contains(fieldName))
                if (row[fieldName] == null || row[fieldName] == DBNull.Value)
                    isValid = false;
                else
                    isValid = true;
            else
                isValid = false;

            if (!isValid)
            {
                switch (dataType.FullName)
                {
                    case "System.Boolean":
                        return false;
                    case "System.Int16":
                        return Convert.ToInt16(0);
                    case "System.Int32":
                        return Convert.ToInt32(0);
                    case "System.Byte":
                        return Convert.ToByte(0);
                    case "System.Int64":
                        return Convert.ToInt64(0);
                    case "System.Decimal":
                        return Convert.ToDecimal(0);
                    case "System.Double":
                        return Convert.ToDouble(0);
                    case "System.Single":
                        return Convert.ToSingle(0);
                    case "System.String":
                        return Convert.ToString("");
                    case "System.Char":
                        return Convert.ToChar(0);
                    case "System.Guid":
                        return Guid.Empty;
                    case "System.DateTime":
                        return new DateTime(1900, 1, 1);
                    case "System.Data.DataSet":
                    case "System.Data.DataTable":
                    default:
                        throw new Exception("unknown data type " + dataType.FullName);
                }
            }
            else
            {
                return row[fieldName];
            }
        }

        /// <summary>
        /// 把要给DataTable数据集中的行包装成一个T类型的数组
        /// </summary>
        /// <typeparam name="T">类型的DbField标记字段对应table中的列</typeparam>
        /// <param name="table"></param>
        /// <returns></returns>
        public static T[] WrapEntities<T>(DataTable table) where T : new()
        {
            List<T> entities = new List<T>();
            Type entityType = typeof(T);
            foreach (DataRow row in table.Rows)
            {
                object entity = new T();
                FulfillEntity(entityType, row, ref entity);
                entities.Add((T)entity);
            }
            return entities.ToArray();
        }

        /// <summary>
        /// 使用row中的值填充对象entity
        /// </summary>
        /// <param name="type"></param>
        /// <param name="row"></param>
        /// <param name="entity"></param>
        public static void FulfillEntity(Type type, DataRow row, ref object entity)
        {
            foreach (MemberInfo m in type.GetMembers(BindingFlags.Instance | BindingFlags.Public))
            {
                Attribute attr = DbFieldAttribute.GetCustomAttribute(m, typeof(DbFieldAttribute));
                if (attr == null)
                    continue;

                DbFieldAttribute dAttr = (DbFieldAttribute)attr;
                if ((dAttr.Usage & DbFieldUsage.MapResultTable) != DbFieldUsage.MapResultTable)
                    continue;

                try
                {
                    if (m.MemberType == MemberTypes.Field)
                    {
                        //the member is a DbField;
                        object value = ValueHelper.GetTableRowValue(row, dAttr.FieldName, ((FieldInfo)m).FieldType);
                        ((FieldInfo)m).SetValue(entity, value);
                    }
                    else if (m.MemberType == MemberTypes.Property)
                    {
                        //the member is a DbField;
                        object value = GetTableRowValue(row, dAttr.FieldName, ((PropertyInfo)m).PropertyType);
                        ((PropertyInfo)m).SetValue(entity, value, null);
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception("成员[" + m.Name + "]出错" + ex.ToString());
                }
            }
        }
    }
}
