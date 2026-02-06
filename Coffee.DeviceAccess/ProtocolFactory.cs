using Coffee.DeviceAdapter;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DeviceAccess
{
    public interface IProtocolFactory
    {
        IProtocolAdapter CreateAdapter(IProtocolOptions option);
    }

    public class ProtocolFactory : IProtocolFactory
    {
        public ProtocolFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        readonly ILoggerFactory _loggerFactory;

        public IProtocolAdapter CreateAdapter(IProtocolOptions option)
        {
            if (option == null)
                throw new ArgumentNullException("option");
            if (option is ModbusSerialOptions modbusSerialOption)
            {
                return new ModbusAdapter(modbusSerialOption, _loggerFactory);
            }
            else if (option is ModbusSocketOptions modbusSocketOption)
            {
                return new ModbusAdapter(modbusSocketOption, _loggerFactory);
            }
            return null;
        }
    }
}
