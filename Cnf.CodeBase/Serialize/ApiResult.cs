using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Cnf.CodeBase.Serialize
{
    /// <summary>
    /// 用于在API调用时封装返回结果。模板类T是封装的DataJson类型。
    /// GET,POST both return this object
    /// </summary>
    public class ApiResult<T> where T:new()
    {
        public const int SUCCESS = 0;
        public const int EXCEPTION = -1;

        [JsonIgnore]
        public bool IsSuccess => ReturnCode == SUCCESS;

        /// <summary>
        /// 0: success, other: failed
        /// </summary>
        public int ReturnCode { get; set; }

        public string Message { get; set; }

        /// <summary>
        /// 设置或返回Json序列化的T对象实例
        /// </summary>
        public string DataJson { get; set; }

        public ApiResult() { }

        /// <summary>
        /// 错误返回，设置返回码和消息
        /// </summary>
        /// <param name="returnCode"></param>
        /// <param name="message"></param>
        public ApiResult(int returnCode, string message)
        {
            if (returnCode == SUCCESS)
                throw new ArgumentException("此构造函数仅用于调用出现错误的时候", nameof(returnCode));

            ReturnCode = returnCode;
            Message = message;
        }

        /// <summary>
        /// 成功返回
        /// </summary>
        /// <param name="data"></param>
        public ApiResult(T data)
        {
            ReturnCode = SUCCESS;
            Message = string.Empty;
            DataJson = SerializationHelper.JsonSerialize(data);
        }

        /// <summary>
        /// 读取封装的对象
        /// </summary>
        /// <returns></returns>
        public T GetData()
        {
            if (ReturnCode == SUCCESS)
            {
                if (string.IsNullOrEmpty(DataJson))
                    return default;

                return SerializationHelper.JsonDeserialize<T>(DataJson);
            }
            else
            {
                throw new Exception(Message);
            }
        }
    }
}
