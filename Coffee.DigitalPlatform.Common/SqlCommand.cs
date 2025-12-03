using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Common
{
    public class SqlCommand
    {
        public SqlCommand(string sql, Dictionary<string, object> parameters, Dictionary<string, Type> parameterTypes = null)
        {
            Sql = sql;
            if (parameters == null)
            {
                return;
            }
            Parameters = new Dictionary<string, object>();
            ParameterTypes = new Dictionary<string, Type>();
            foreach (var parameter in parameters)
            {
                Parameters.Add(parameter.Key, parameter.Value);
                if (parameterTypes == null)
                    continue;
                if (parameterTypes.TryGetValue(parameter.Key, out var type) && type != null)
                {
                    if (parameter.Value == null)
                    {
                        if (!type.IsClass) //参数值是空值，但是指定参数类型为为空
                        {
                            throw new Exception("SQL Command指定的参数类型与值类型不一致！");
                        }
                    }
                    else
                    {
                        //参数值类型和指定参数类型不一致，抛出异常
                        if (!(parameter.Value.GetType() == type || type.IsAssignableFrom(parameter.Value.GetType())))
                        {
                            throw new Exception("SQL Command指定的参数类型与值类型不一致！");
                        }
                    }
                    ParameterTypes.Add(parameter.Key, type);
                }
                else
                {
                    if (parameter.Value != null)
                    {
                        ParameterTypes.Add(parameter.Key, parameter.Value.GetType());
                    }
                    else
                    {
                        ParameterTypes.Add(parameter.Key, typeof(object));
                    }
                }
            }
        }

        public string Sql {  get; private set; }

        public Dictionary<string, object> Parameters;

        public Dictionary<string, Type> ParameterTypes;
    }
}
