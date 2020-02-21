using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Cnf.DataAccess.SqlServer
{
    /// <summary>
    /// 封装了SQL Server的SqlConnection
    /// </summary>
    public class DbConnector: IDisposable
    {
        readonly SqlConnection _connection = null;

        public DbConnector(string connectString)
        {
            _connection = new SqlConnection(connectString);
        }

        /// <summary>
        /// 安全地打开一个链接
        /// </summary>
        void OpenConnection()
        {
            if (_connection.State == ConnectionState.Closed)
            {
                _connection.Open();
            }
        }

        /// <summary>
        /// 安全地关闭一个链接
        /// </summary>
        void CloseConnection()
        {
            if (_connection.State != ConnectionState.Closed)
            {
                _connection.Close();
            }
        }

        /// <summary>
        /// 执行一段不用来SELECT数据集的T-SQL脚本，返回被影响的行数。
        /// 设置了5分钟的超时。
        /// </summary>
        /// <remarks>
        /// 本方法将提交的T-SQL脚本（参数sql）作为一个独立事务执行，不支持与外部其他脚本块组合成一个整体事务。
        /// 为防止SQL注入漏洞，调用者应当确保sql是安全的，参数都通过parameters传入。
        /// 如果参数类型是VarChar或NVarChar，空字符串被当成DBNull，如果确实希望传入String.Empty,那么请使用Char、NChar指定长度字符串。
        /// </remarks>
        /// <returns>执行后影响的行数</returns>
        public async Task<int> ExecuteSqlNonQuery(string sql, params SqlParameter[] parameters)
        {
            try
            {
                int affectRows = 0;
                SqlCommand command = _connection.CreateCommand();
                command.CommandTimeout = 300;
                command.CommandType = CommandType.Text;
                command.CommandText = sql;
                if (parameters != null && parameters.Length > 0)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        if (parameters[i].SqlDbType.Equals(SqlDbType.VarChar)
                            || parameters[i].SqlDbType.Equals(SqlDbType.NVarChar))
                        {
                            if (parameters[i].Value.Equals(string.Empty))
                            {
                                parameters[i].Value = DBNull.Value;
                            }
                        }
                        command.Parameters.Add(parameters[i]);
                    }
                }

                OpenConnection();
                affectRows = await command.ExecuteNonQueryAsync();
                command.Parameters.Clear();
                command.Dispose();
                return affectRows;
            }
            catch (SqlException e)
            {
                throw e;
            }
            finally
            {
                this.CloseConnection();
            }

        }

        /// <summary>
        /// 执行一段SQL查询脚本，返回一个DataTable（返回单一记录集），设置了5分钟超时。
        /// </summary>
        /// <remarks>
        /// 本方法建议sql参数不包括更新、插入和删除操作，而是仅仅进行查询操作。
        /// 为防止SQL注入，调用者应确保sql是安全的，所有参数都通过parameters传入。
        /// </remarks>
        public async Task<DataTable> ExecuteSqlQueryTable(string sql, params SqlParameter[] parameters)
        {
            SqlDataAdapter sqlDA = new SqlDataAdapter(sql, _connection);
            sqlDA.SelectCommand.CommandType = CommandType.Text;
            sqlDA.SelectCommand.CommandTimeout = 300;

            if (parameters != null && parameters.Length > 0)
            {
                for (int i = 0; i < parameters.Length; i++)
                    sqlDA.SelectCommand.Parameters.Add(parameters[i]);
            }

            DataTable dt = new DataTable("result");

            await Task.Run(() => sqlDA.Fill(dt));

            sqlDA.SelectCommand.Parameters.Clear();
            sqlDA.Dispose();
            return dt;
        }

        /// <summary>
        /// 执行一段SQL查询脚本，返回一个DataSet（返回一组记录集），设置5分钟超时。
        /// </summary>
        /// <remarks>
        /// 本方法建议sql参数不包括更新、插入和删除操作，而是仅仅进行查询操作。
        /// 为防止SQL注入，调用者应确保sql是安全的，所有参数都通过parameters传入。
        /// </remarks>
        public async Task<DataSet> ExecuteSqlQuerySet(string sql, params SqlParameter[] parameters)
        {
            SqlDataAdapter sqlDA = new SqlDataAdapter(sql, _connection);
            sqlDA.SelectCommand.CommandType = CommandType.Text;
            sqlDA.SelectCommand.CommandTimeout = 300;

            if (parameters != null && parameters.Length > 0)
            {
                for (int i = 0; i < parameters.Length; i++)
                    sqlDA.SelectCommand.Parameters.Add(parameters[i]);
            }

            DataSet ds = new DataSet();
            await Task.Run(() => sqlDA.Fill(ds));

            sqlDA.SelectCommand.Parameters.Clear();
            sqlDA.Dispose();
            return ds;
        }

        public void Dispose()
        {
            if (_connection != null)
                _connection.Dispose();
        }
    }
}
