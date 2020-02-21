using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Cnf.CodeBase.Data;
using Cnf.CodeBase.Serialize;

namespace Cnf.DataAccess.SqlServer
{
    public static class DbHelper
    {
        #region 数据库增、删、改

        /// <summary>
        /// 在数据库中插入一条新的实体记录，实体类必须使用[DbTable]和[DbField]进行标记，返回新插入的记录的自增长ID。
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="entity"></param>
        /// <returns></returns>
        public static async Task<int> InsertEntity(DbConnector connector, object entity)
        {
            SqlParameter[] parameters;
            string sql = BuildInsertSql(entity, out parameters, out SqlParameter primaryKey);

            await connector.ExecuteSqlNonQuery(sql, parameters);
            return (int)primaryKey.Value;
        }

        /// <summary>
        /// 使用实体类更新一条数据库记录，实体类必须使用[DbTable]和[DbField]进行标记，并且关键字属性被设置为正确的ID值。
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="entity"></param>
        /// <returns>返回更新记录的关键字id值</returns>
        public static async Task<int> UpdateEntity(DbConnector connector, object entity)
        {
            SqlParameter[] parameters;
            string sql = BuildUpdateSql(entity, out parameters, out int primaryKeyValue);
            await connector.ExecuteSqlNonQuery(sql, parameters);
            return primaryKeyValue;
        }

        /// <summary>
        /// 从数据库中删除一条记录
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="tableName"></param>
        /// <param name="primaryKey"></param>
        /// <param name="value"></param>
        public static async Task DeleteEntity(DbConnector connector, string tableName, string primaryKey, int value)
        {
            string fieldName;
            SqlParameter p;
            if (primaryKey.StartsWith("@"))
            {
                p = new SqlParameter(primaryKey, value);
                fieldName = primaryKey.Substring(1);
            }
            else
            {
                p = new SqlParameter("@" + primaryKey, value);
                fieldName = primaryKey;
            }

            string sql = @$"DELETE FROM {tableName} WHERE [{fieldName}]={p.ParameterName}";
            await connector.ExecuteSqlNonQuery(sql, p);
        }

        /// <summary>
        /// 从数据库中删除一条记录
        /// </summary>
        public static async Task DeleteEntity<T>(DbConnector connector, int id) where T: new()
        {
            Type entityType = typeof(T);
            if (!(DbTableAttribute.GetCustomAttribute(entityType, typeof(DbTableAttribute)) is DbTableAttribute tableAttr))
                throw new Exception("类型" + entityType.FullName + "没有标记DbTableAttribute特性");

            string tableName = tableAttr.TableName;
            string primaryKey = string.Empty;
            foreach (PropertyInfo property in entityType.GetProperties())
            {
                if (!property.IsDefined(typeof(DbFieldAttribute), false))
                    continue;

                DbFieldAttribute fldAttr = (DbFieldAttribute)(Attribute.GetCustomAttribute(property, typeof(DbFieldAttribute)));
                if (fldAttr.IsPrimaryKey)
                {
                    primaryKey = fldAttr.FieldName;
                }
            }
            if(string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(primaryKey))
            {
                throw new Exception("类型" + entityType.FullName + "不支持数据库操纵");
            }
            await DeleteEntity(connector, tableName, primaryKey, id);
        }

        #endregion

        #region 查询数据库

        /// <summary>
        /// 自动添加需要的参数和语句，根据传入参数生成的可以用于分页查询的T-SQL语句。
        /// 为了防止SQL注入，用于筛选的SQL参数需要事先在whereClause子句中定义，如：name like '%' + @name + '%'。
        /// </summary>
        /// <param name="selectClause">SELECT 子句</param>
        /// <param name="fromClause">FROM 子句</param>
        /// <param name="whereClause">WHERE 子句，请勿直接使用拼接查询串方式，而是使用@sql参数</param>
        /// <param name="orderBy">排序列，必须设置不能为空白，如需倒序请在此参数中加入desc</param>
        /// <param name="pageIndex">当前页，从0开始</param>
        /// <param name="pageSize">每页行数</param>
        /// <returns>一个SQL，执行的话填充一个DataSet，第一个Table是记录总数，第二个Table是结果集，并且结果集中包含一个[_row_index]字段指定行号</returns>
        public static string BuildPagedSelectSql(string selectClause, string fromClause, string whereClause,
                                                 string orderBy, int pageIndex, int pageSize)
        {
            if (string.IsNullOrEmpty(orderBy))
                throw new Exception("使用本方法必须传递排序列");

            selectClause = selectClause.Trim();
            fromClause = fromClause.Trim();
            whereClause = whereClause.Trim();
            orderBy = orderBy.Trim();

            StringBuilder sqlBuilder = new StringBuilder();
            if (!selectClause.StartsWith("select ", StringComparison.OrdinalIgnoreCase))
            {
                sqlBuilder.Append("SELECT ");
            }
            sqlBuilder.Append(selectClause);
            sqlBuilder.Append(" INTO #tmp ");   //先把满足条件的记录加入到一个临时表#tmp
            if (!fromClause.StartsWith("from ", StringComparison.OrdinalIgnoreCase))
            {
                sqlBuilder.Append("FROM ");
            }
            sqlBuilder.Append(fromClause);
            sqlBuilder.Append(" ");
            if (whereClause.Length > 0)
            {
                if (!whereClause.StartsWith("where ", StringComparison.OrdinalIgnoreCase))
                {
                    sqlBuilder.Append("WHERE ");
                }
            }
            sqlBuilder.Append(whereClause);
            if (!whereClause.EndsWith(";"))
            {
                sqlBuilder.Append(";");
            }
            sqlBuilder.Append("\r\n\r\n");

            sqlBuilder.Append("SELECT COUNT(*) FROM #tmp;\r\n"); //第一个返回DataTable是总记录行数。
            sqlBuilder.Append("DECLARE @output_sql NVARCHAR(MAX);\r\n");
            sqlBuilder.Append(@"
SET @output_sql = '
    WITH source AS 
    (	SELECT ROW_NUMBER() OVER( ORDER BY " + orderBy + @") AS [_row_index], 
    	   [#tmp].* FROM #tmp
    )
    SELECT * FROM source
    WHERE [_row_index] > " + pageSize * pageIndex + @"
      AND [_row_index] <= " + pageSize * (pageIndex + 1) + @"
    ORDER BY [_row_index]';");
            sqlBuilder.Append("\r\n");
            sqlBuilder.Append("EXEC(@output_sql);\r\n");
            sqlBuilder.Append("DROP TABLE #tmp;\r\n");

            return sqlBuilder.ToString();
        }

        /// <summary>
        /// 使用关键字id从数据库中查找，返回entityType类的实例
        /// </summary>
        /// <param name="connector"></param>
        /// <param name="entityType"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static async Task<T> FindEntity<T>(DbConnector connector, int id) where T:new()
        {
            Type entityType = typeof(T);
            object[] tableAttributes = entityType.GetCustomAttributes(typeof(DbTableAttribute), false);
            if (tableAttributes == null || tableAttributes.Length != 1)
            {
                throw new Exception("使用FindEntity()方法，类" + entityType.FullName + "必须标记唯一的DbTable特性。");
            }

            DbTableAttribute tableAttr = (DbTableAttribute)tableAttributes[0];
            string tableName = tableAttr.TableName;
            string keyName = string.Empty;
            foreach (PropertyInfo propertyInfo in entityType.GetProperties())
            {
                if (!propertyInfo.IsDefined(typeof(DbFieldAttribute), false))
                    continue;

                DbFieldAttribute fldAttr = (DbFieldAttribute)(DbFieldAttribute.GetCustomAttribute(propertyInfo, typeof(DbFieldAttribute)));
                if (fldAttr.IsPrimaryKey)
                {
                    keyName = fldAttr.FieldName;
                    break;
                }
            }

            string sql = string.Format(@"SELECT * FROM {0} WHERE {1}={2};", tableName, keyName, id);

            DataTable table = await connector.ExecuteSqlQueryTable(sql);
            if (table == null || table.Rows.Count == 0)
            {
                return default(T);
            }
            else
            {
                object obj = Activator.CreateInstance(entityType);
                ValueHelper.FulfillEntity(entityType, table.Rows[0], ref obj);
                return (T)obj;
            }
        }

        /// <summary>
        /// 分页读取数据库中全部实体，不带查询条件
        /// </summary>
        /// <typeparam name="T">实体类</typeparam>
        /// <param name="connector">数据源</param>
        /// <param name="pageIndex">读取的页（从0开始）</param>
        /// <param name="pageSize">每页包含记录条数</param>
        /// <param name="sortProperty">排序方法</param>
        /// <returns></returns>
        public static async Task<PagedQuery<T>> QueryPagedEntity<T>(DbConnector connector, int pageIndex, int pageSize,
            string sortProperty, bool descending) where T : new() =>
            await SearchEntities<T>(connector, null, pageIndex, pageSize, sortProperty, descending);

        /// <summary>
        /// 使用提供的条件查询数据库（表由entityType指定），返回满足条件的记录（分页）
        /// </summary>
        /// <typeparam name="T">要查询的实体类型，标记了DbTable和DbField特性的类，并且具有空白构造函数</typeparam>
        /// <param name="connector"></param>
        /// <param name="criteria">查询条件字典，键是PropertyName，条件仅支持=比较，条件间仅支持AND运算</param>
        /// <param name="pageIndex">从0开始的返回页索引</param>
        /// <param name="pageSize">每页几行记录</param>
        /// <param name="sortProperty">用于排序的PropertyName(如为空则使用关键字)</param>
        /// <param name="descending">是否倒序排列</param>
        /// <param name="total">符合条件的记录总数</param>
        /// <returns></returns>
        public static async Task<PagedQuery<T>> SearchEntities<T>(DbConnector connector, Dictionary<string, object> criteria,
            int pageIndex, int pageSize, string sortProperty, bool descending) where T:new()
        {
            Type entityType = typeof(T);
            if (!(DbTableAttribute.GetCustomAttribute(entityType, typeof(DbTableAttribute)) is DbTableAttribute tableAttr))
                throw new Exception("类型" + entityType.FullName + "没有标记DbTableAttribute特性");

            string selectClause = "SELECT * ";
            string fromClause = "FROM " + tableAttr.TableName;
            string orderBy;

            #region 正确处理orderBy
            DbFieldAttribute sortFieldAttr = null;
            if (string.IsNullOrWhiteSpace(sortProperty))
            {
                //如果没有传递sortProperty参数, 则使用PrimaryKey
                foreach (PropertyInfo property in entityType.GetProperties())
                {
                    DbFieldAttribute fldAttr = DbFieldAttribute.GetCustomAttribute(property, typeof(DbFieldAttribute)) as DbFieldAttribute;
                    if (fldAttr != null && fldAttr.IsPrimaryKey)
                    {
                        sortFieldAttr = fldAttr;
                        break;
                    }
                }
            }
            else
            {
                PropertyInfo property = entityType.GetProperty(sortProperty);
                if (property == null)
                    throw new Exception(string.Format("类型{0}中没有找到{1}属性用于排序", entityType.FullName, sortProperty));
                sortFieldAttr = DbFieldAttribute.GetCustomAttribute(property, typeof(DbFieldAttribute)) as DbFieldAttribute;
            }
            if (sortFieldAttr == null)
                throw new Exception("类型" + entityType.FullName + "没有标记IsPrimaryKey=true的DbField特性");

            if (descending)
                orderBy = sortFieldAttr.FieldName + " DESC";
            else
                orderBy = sortFieldAttr.FieldName;
            #endregion

            StringBuilder whereBuilder = new StringBuilder();
            List<SqlParameter> sqlParameters = new List<SqlParameter>();
            if (criteria != null)
            {
                foreach (string conditionName in criteria.Keys)
                {
                    if (criteria[conditionName] == null || criteria[conditionName].ToString().Length == 0)
                        continue;

                    PropertyInfo conditionProperty = entityType.GetProperty(conditionName);
                    if (conditionProperty == null)
                        throw new Exception(string.Format("类型{0}中没有找到{1}属性用于查询条件", entityType.FullName, conditionName));
                    DbFieldAttribute conditionAttr = DbFieldAttribute.GetCustomAttribute(conditionProperty, typeof(DbFieldAttribute)) as DbFieldAttribute;
                    if (conditionAttr == null)
                        throw new Exception(string.Format("类型{0}的{1}属性没有标记DbFieldAttribute特性, 无法用于数据库查询",
                            entityType.FullName, conditionName));

                    SqlParameter parameter = GenerateSqlParameter(conditionAttr, conditionProperty.PropertyType, criteria[conditionName]);
                    sqlParameters.Add(parameter);
                    whereBuilder.AppendFormat("[{0}]=@{0} ", conditionAttr.FieldName);
                    whereBuilder.Append("AND ");
                }
            }
            if (whereBuilder.Length > 0)
                whereBuilder.Remove(whereBuilder.Length - 4, 4);

            string sql = BuildPagedSelectSql(selectClause, fromClause, whereBuilder.ToString(), orderBy, pageIndex, pageSize);

            DataSet ds = await connector.ExecuteSqlQuerySet(sql, sqlParameters.ToArray());
            PagedQuery<T> result = new PagedQuery<T>();
            if (ds != null && ds.Tables.Count == 2)
            {
                result.Total = (int)ds.Tables[0].Rows[0][0];
                ds.Tables[1].TableName = "result";
                result.Records = await Task.Run(() => ValueHelper.WrapEntities<T>(ds.Tables[1]));
            }
            else
            {
                result.Total = 0;
                result.Records = new T[0];
            }
            ds.Dispose();
            return result;
        }

        #endregion

        #region 私有静态方法

        static SqlParameter GenerateSqlParameter(DbFieldAttribute fldAttr, Type memberType, object value)
        {
            string parameterName = "@" + fldAttr.FieldName;
            if (value != null)
            {
                if (fldAttr.FieldType == SqlDbType.BigInt 
                    || fldAttr.FieldType == SqlDbType.Int
                    || fldAttr.FieldType == SqlDbType.SmallInt 
                    || fldAttr.FieldType == SqlDbType.TinyInt
                    || fldAttr.FieldType == SqlDbType.UniqueIdentifier 
                    || fldAttr.FieldType == SqlDbType.Xml
                   )
                {
                    SqlParameter p = new SqlParameter(parameterName, fldAttr.FieldType)
                    {
                        Value = value
                    };
                    return p;
                }
                else
                {
                    return new SqlParameter(parameterName, value);
                }
            }
            else
            {
                SqlDbType dbType = (fldAttr.TypeDefined)? fldAttr.FieldType: ValueHelper.MapSystemTypeToSqlDbType(memberType);
                return new SqlParameter(parameterName, dbType)
                {
                    Value = DBNull.Value
                };
            }
        }

        /// <summary>
        /// 从一个对象实例中通过检查DbTable和DbField特性来构建一个用于插入数据行的T-SQL语句。
        /// 为了防止SQL注入，该SQL语句是参数化的，并且通过out参数返回由entity对象实例中包括的值构建出来的SqlParameter集
        /// </summary>
        /// <param name="entity">准备用作插入新行的对象实例</param>
        /// <param name="parameters">参数化的每一列输入参数，包含了值</param>
        /// <param name="primaryKey">主键输出参数，用来返回新行的主键</param>
        /// <returns></returns>
        static string BuildInsertSql(object entity, out SqlParameter[] parameters, out SqlParameter primaryKey)
        {
            Type type = entity.GetType();
            object[] tableAttributes = type.GetCustomAttributes(typeof(DbTableAttribute), false);
            if (tableAttributes == null || tableAttributes.Length != 1)
            {
                throw new Exception("使用BuildInsertSql()方法，类" + type.FullName + "必须标记唯一的DbTable特性。");
            }

            DbTableAttribute tableAttr = (DbTableAttribute)tableAttributes[0];

            StringBuilder fieldBuilder = new StringBuilder(), valueBuilder = new StringBuilder();
            List<SqlParameter> parametersList = new List<SqlParameter>();
            primaryKey = null;
            foreach (MemberInfo m in type.GetMembers(BindingFlags.Instance | BindingFlags.Public))
            {
                #region loop values
                bool isDefine = m.IsDefined(typeof(DbFieldAttribute), false);
                if (!isDefine)
                    continue;

                DbFieldAttribute fldAttr = (DbFieldAttribute)(DbFieldAttribute.GetCustomAttribute(m, typeof(DbFieldAttribute)));
                if (fldAttr.IsPrimaryKey)
                {
                    primaryKey = new SqlParameter("@" + fldAttr.FieldName, SqlDbType.Int)
                    {
                        Direction = ParameterDirection.Output
                    };
                    parametersList.Add(primaryKey);
                    continue;
                }

                if ((fldAttr.Usage & DbFieldUsage.InsertParameter) != DbFieldUsage.InsertParameter)
                    continue;

                fieldBuilder.Append(",[" + fldAttr.FieldName + "]");
                valueBuilder.Append(",@" + fldAttr.FieldName);

                Type memberType;
                object memberValue = ValueHelper.GetMemberValue(m, fldAttr, entity, out memberType);
                SqlParameter parameter = GenerateSqlParameter(fldAttr, memberType, memberValue);
                parametersList.Add(parameter);
                #endregion
            }
            fieldBuilder.Remove(0, 1);
            valueBuilder.Remove(0, 1);

            string sql = @$"
INSERT INTO {tableAttr.TableName}({fieldBuilder.ToString()}) VALUES({valueBuilder.ToString()});
SET {primaryKey.ParameterName}=@@Identity;";

            parameters = parametersList.ToArray();
            return sql;
        }

        /// <summary>
        /// 从一个对象实例中通过检查DbTable和DbField特性来构建一个用于更新数据行的T-SQL语句。
        /// 为了防止SQL注入，该SQL语句是参数化的，并且通过out参数返回由entity对象实例中包括的值构建出来的SqlParameter集
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="parameters"></param>
        /// <param name="primaryKeyValue">从对象实例读取到的主键值</param>
        /// <returns></returns>
        static string BuildUpdateSql(object entity, out SqlParameter[] parameters, out int primaryKeyValue)
        {
            primaryKeyValue = 0;
            Type type = entity.GetType();
            object[] tableAttributes = type.GetCustomAttributes(typeof(DbTableAttribute), false);
            if (tableAttributes == null || tableAttributes.Length != 1)
            {
                throw new Exception("使用BuildUpdateSql()方法，类" + type.FullName + "必须标记唯一的DbTable特性。");
            }

            DbTableAttribute tableAttr = (DbTableAttribute)tableAttributes[0];

            StringBuilder columnBuilder = new StringBuilder(), whereBuilder = new StringBuilder();
            List<SqlParameter> parametersList = new List<SqlParameter>();
            foreach (MemberInfo m in type.GetMembers(BindingFlags.Instance | BindingFlags.Public))
            {
                #region loop values
                bool isDefine = m.IsDefined(typeof(DbFieldAttribute), false);
                if (!isDefine)
                    continue;

                DbFieldAttribute fldAttr = (DbFieldAttribute)(DbFieldAttribute.GetCustomAttribute(m, typeof(DbFieldAttribute)));
                if ((fldAttr.Usage & DbFieldUsage.UpdateParameter) != DbFieldUsage.UpdateParameter)
                    continue;

                object memberValue = ValueHelper.GetMemberValue(m, fldAttr, entity, out Type memberType);
                SqlParameter parameter = GenerateSqlParameter(fldAttr, memberType, memberValue);
                parametersList.Add(parameter);

                if (fldAttr.IsPrimaryKey)
                {
                    primaryKeyValue = Convert.ToInt32(memberValue);
                    whereBuilder.Append("[" + fldAttr.FieldName + "]=" + parameter.ParameterName);
                }
                else
                {
                    columnBuilder.Append(",[" + fldAttr.FieldName + "]=" + parameter.ParameterName);
                }
                #endregion
            }
            columnBuilder.Remove(0, 1);

            string sql = @$"
UPDATE {tableAttr.TableName} 
SET {columnBuilder.ToString()}
WHERE {whereBuilder.ToString()};";

            parameters = parametersList.ToArray();

            if (primaryKeyValue == 0)
                throw new Exception("关键字属性必须是不为0的整数");

            return sql;
        }

        #endregion
    }
}
