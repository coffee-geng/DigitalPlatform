using Coffee.DeviceAdapter;
using Serilog;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;

namespace Coffee.DeviceService
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
