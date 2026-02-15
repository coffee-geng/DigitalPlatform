using Dapper;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Entities.Converter
{
    public class JsonTypeHandler<T> : SqlMapper.TypeHandler<T>
    {
        public override T Parse(object value)
        {
            // 如果数据库返回的是DBNull
            if (value == null || value is DBNull)
                return default(T);

            var jsonString = value.ToString();

            // 使用Newtonsoft.Json
            return JsonConvert.DeserializeObject<T>(jsonString);

            // 或者使用System.Text.Json
            // return JsonSerializer.Deserialize<T>(jsonString);
        }

        public override void SetValue(IDbDataParameter parameter, T value)
        {
            parameter.Value = value == null
                ? (object)DBNull.Value
                : JsonConvert.SerializeObject(value);
        }
    }
}
