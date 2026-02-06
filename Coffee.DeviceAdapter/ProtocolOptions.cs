using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DeviceAdapter
{
    abstract public class ProtocolOptions : IProtocolOptions
    {
        public EndianTypes EndianType { get; set; } = EndianTypes.ABCD;

        public void Config(string paramName, string paramValue, Type paramType)
        {
            if (string.IsNullOrEmpty(paramName))
                throw new ArgumentNullException(nameof(paramName));

            var propInfo = this.GetType().GetProperty(paramName, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (propInfo == null)
                return;

            if (paramName == nameof(EndianType))
            {
                if (Enum.TryParse(typeof(EndianTypes), paramValue, true, out object enumValue))
                {
                    EndianType = (EndianTypes)enumValue;
                }
                else
                {
                    throw new Exception($"配置字节序出错！");
                }
            }
            else
            {
                try
                {
                    if (paramType.IsEnum)
                    {
                        propInfo.SetValue(this, Enum.Parse(paramType, paramValue));
                    }
                    else
                    {
                        propInfo.SetValue(this, Convert.ChangeType(paramValue, paramType));
                    }
                }
                catch (Exception ex)
                {
                    throw new Exception($"配置通信协议属性{paramName}出错！");
                }
            }
        }
    }
}
