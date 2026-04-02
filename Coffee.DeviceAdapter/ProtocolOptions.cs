using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.CompilerServices;
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
                    else if (paramType == typeof(IPAddress))
                    {
                        propInfo.SetValue(this, IPAddress.Parse(paramValue));
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

    public class EndianModeExtension
    {
        public static EndianMode GetEndianModeByProtocol(string protocol)
        {
            var options = Assembly.GetAssembly(typeof(EndianModeExtension)).GetTypes().Where(t => t.IsSubclassOf(typeof(ProtocolOptions)));
            if (options == null) return EndianMode.BigLittleEndian;

            var option = options.Where(opt => string.Equals(opt.FullName, $"Coffee.DeviceAdapter.{protocol}_Options")).FirstOrDefault();
            if (option == null) return EndianMode.BigLittleEndian;

            EndianModeAttribute attribute = option.GetCustomAttribute<EndianModeAttribute>();
            return attribute?.Mode ?? EndianMode.BigLittleEndian;
        }
    }
}
