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
            else if (option is SiemensS7_Options siemensS7_Option)
            {
                return new SiemensS7Adapter(siemensS7_Option, _loggerFactory);
            }
            else if (option is OmronFins_Options fins_Option)
            {
                return new OmronFinsAdapter(fins_Option, _loggerFactory);
            }
            else if (option is OmronFinsTcp_Options finstcp_Option)
            {
                return new OmronFinsAdapter(finstcp_Option, _loggerFactory);
            }
            else if (option is OmronCIP_Options cip_Option)
            {
                return new OmronCIPAdapter(cip_Option, _loggerFactory);
            }
            return null;
        }
    }
}
