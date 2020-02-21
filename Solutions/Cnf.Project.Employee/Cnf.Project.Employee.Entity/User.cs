using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using Cnf.CodeBase.Data;
using Newtonsoft.Json;

namespace Cnf.Project.Employee.Entity
{
    [Flags]
    public enum RoleEnum
    {
        SystemAdmin = 0b0001,
        HumanResourceAdmin = 0b0010,
        ProjectAdmin = 0b0100,
        Manager = 0b1000,
    }

    [DbTable("tb_user")]
    public class User
    {
        [DbField("UserID", FieldType =SqlDbType.Int, IsPrimaryKey =true)]
        public int UserID { get; set; }

        [DbField("Login", FieldType = SqlDbType.NVarChar)]
        public string Login { get; set; }

        [DbField("Name", FieldType =SqlDbType.NVarChar)]
        public string Name { get; set; }

        [DbField("Role", FieldType =SqlDbType.Int)]
        public int Role { get; set; }

        [DbField("ActiveStatus", FieldType = SqlDbType.Bit)]
        public bool ActiveStatus { get; set; }
    }

    /// <summary>
    /// 修改用户登录的Credential。
    /// Credential是自加密的字符串，而非明文
    /// </summary>
    public class ChangeCredential
    {
        public int UserID { get; set; }

        public string OldCredential { get; set; }

        public string NewCredential { get; set; }
    }

    /// <summary>
    /// 用于验证用户身份的对象
    /// </summary>
    public class Authentication
    {
        public string Login { get; set; }

        public string Credential { get; set; }
    }
}
