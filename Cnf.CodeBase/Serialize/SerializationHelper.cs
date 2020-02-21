using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Cnf.CodeBase.Serialize
{
    public static class SerializationHelper
    {
        public static string JsonSerialize(object data)
        {
            if (data == null)
                return string.Empty;

            return JsonConvert.SerializeObject(data);
        }

        public static T JsonDeserialize<T>(string json)
        {
            if (string.IsNullOrEmpty(json))
                return default;

            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}
