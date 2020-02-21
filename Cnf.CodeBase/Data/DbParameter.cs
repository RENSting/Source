using System;
using System.Data;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Cnf.CodeBase.Data
{
    /// <summary>
    /// 用来传递参数化SQL查询所需要的参数。
    /// </summary>
    /// <remarks>
    /// 该参数包括了可以用于显示的友好名称，列名，类型，值等
    /// </remarks>
    public class DbParameter
    {
        /// <summary>
        /// 参数名称，由于并非仅用于T-SQL，因此参数名不包括前缀(T-SQL前缀是@)
        /// </summary>
        public readonly string ParameterName;
        public readonly SqlDbType DataType;
        public readonly string ValueString;

        public readonly int? Size = null;
        public readonly ParameterDirection Direction = ParameterDirection.Input;

        /// <summary>
        /// 友好参数名称（@[ParameterName]）
        /// </summary>
        public string Label
        {
            get
            {
                return $"@{ParameterName}";
            }
        }

        /// <summary>
        /// 可读完整参数（@[ParameterName]=[ValueString]）
        /// </summary>
        public string DisplayText
        {
            get
            {
                return $"@{ParameterName}={ValueString}";
            }
        }

        #region ctrs

        /// <summary>
        /// 直接使用值初始化参数，无需指明类型和长度，方向只能是Input
        /// </summary>
        public DbParameter(string parameterName, object parameterValue)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
                throw new Exception("没有为参数指定名称");

            if (parameterValue == null || parameterValue == DBNull.Value)
            {
                throw new Exception("null值参数必须使用具体指明类型的构造函数");
            }

            if (parameterName.StartsWith("@"))
                ParameterName = parameterName.Substring(1);
            else
                ParameterName = parameterName;

            DataType = Serialize.ValueHelper.MapSystemTypeToSqlDbType(parameterValue.GetType());
            ValueString = Convert.ToString(parameterValue);
            switch (DataType)
            {
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.UniqueIdentifier:
                case SqlDbType.VarChar:
                case SqlDbType.Xml:
                    Size = ValueString.Length;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 需要指明类型，但无需指明长度，如int。需要请直接使用字符串表示值
        /// </summary>
        public DbParameter(string parameterName, SqlDbType dbType, string valueString)
        {
            if (string.IsNullOrWhiteSpace(parameterName))
                throw new Exception("没有为参数指定名称");

            if (parameterName.StartsWith("@"))
                ParameterName = parameterName.Substring(1);
            else
                ParameterName = parameterName;

            DataType = dbType;
            ValueString = valueString;
            switch (DataType)
            {
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                case SqlDbType.Text:
                case SqlDbType.UniqueIdentifier:
                case SqlDbType.VarChar:
                case SqlDbType.Xml:
                    Size = ValueString.Length;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 需要同时指明数据类型和长度，如nvarchar类型。
        /// 无论使用何种类型，valueString都请直接使用字符串表示值。
        /// </summary>
        public DbParameter(string sqlParameterName, SqlDbType dbType, int size, string valueString) :
            this(sqlParameterName, dbType, valueString)
        {
            Size = size;
        }

        /// <summary>
        /// 需要指明类型，但无需指明长度，如int。请直接使用字符串表示值，同时也需要指明方向。
        /// </summary>
        public DbParameter(string parameterName, SqlDbType dbType, ParameterDirection direction, string valueString) :
            this(parameterName, dbType, valueString)
        {
            Direction = direction;
        }

        /// <summary>
        /// 需要同时指明数据类型和长度，如nvarchar。请直接使用字符串表示值，同时也需要指明方向。
        /// </summary>
        public DbParameter(string sqlParameterName, SqlDbType dbType, int size, ParameterDirection direction, string valueString) :
            this(sqlParameterName, dbType, size, valueString)
        {
            Direction = direction;
        }

        #endregion

        /// <summary>
        /// use properties to build the Xml Element, but not append it into doc, the caller will do it
        /// </summary>
        void BuildElement(XmlDocument doc, XmlElement e)
        {
            XmlElement node = doc.CreateElement("ParameterName");
            node.InnerText = ParameterName;
            e.AppendChild(node);
            node = doc.CreateElement("DataType");
            node.InnerText = ((int)DataType).ToString();
            e.AppendChild(node);
            node = doc.CreateElement("ValueString");
            node.InnerText = ValueString;
            e.AppendChild(node);
            node = doc.CreateElement("Size");
            node.InnerText = Convert.ToString(Size);
            e.AppendChild(node);
            node = doc.CreateElement("Direction");
            node.InnerText = Convert.ToString((int)Direction);
            e.AppendChild(node);
        }

        /// <summary>
        /// According XmlElement, create an instance
        /// </summary>
        /// <returns></returns>
        static DbParameter Create(XmlElement e)
        {
            XmlNodeList nodes = e.ChildNodes;
            string parameterName = nodes[0].InnerText;
            SqlDbType dataType = (SqlDbType)Convert.ToInt32(nodes[1].InnerText);
            string valueString = nodes[2].InnerText;
            string sizeString = nodes[3].InnerText;
            int? size;
            if (string.IsNullOrEmpty(sizeString))
            {
                size = null;
            }
            else
            {
                size = Convert.ToInt32(sizeString);
            }
            ParameterDirection direction = (ParameterDirection)Convert.ToInt32(nodes[4].InnerText);
            DbParameter p;
            if (size == null)
                p = new DbParameter(parameterName, dataType, direction, valueString);
            else
                p = new DbParameter(parameterName, dataType, size.Value, direction, valueString);

            return p;
        }

        /// <summary>
        /// XML序列化一个DbParameter数组
        /// </summary>
        /// <param name="parameters"></param>
        /// <returns></returns>
        public static string XmlSerialize(DbParameter[] parameters)
        {
            XmlDocument doc = new XmlDocument();
            XmlElement root = doc.CreateElement("collect");
            doc.AppendChild(root);
            foreach (DbParameter ssp in parameters)
            {
                XmlElement e = doc.CreateElement("param");
                ssp.BuildElement(doc, e);
                root.AppendChild(e);
            }
            return doc.OuterXml;
        }

        /// <summary>
        /// 反序列化一个XML，返回DbParameter数组
        /// </summary>
        /// <param name="parameterArrayXml"></param>
        /// <returns></returns>
        public static DbParameter[] XmlDeserialize(string parameterArrayXml)
        {
            if (string.IsNullOrEmpty(parameterArrayXml))
            {
                return new DbParameter[0];
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(parameterArrayXml);
            List<DbParameter> list = new List<DbParameter>();
            foreach (XmlElement e in doc.DocumentElement.ChildNodes)
            {
                DbParameter param = DbParameter.Create(e);
                list.Add(param);
            }
            return list.ToArray();
        }
    }
}
