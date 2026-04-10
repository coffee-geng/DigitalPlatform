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
            else if (option is SiemensS7_Options siemensS7_option)
            {
                return new SiemensS7Adapter(siemensS7_option, _loggerFactory);
            }
            else if (option is OmronFins_Options fins_option)
            {
                return new OmronFinsAdapter(fins_option, _loggerFactory);
            }
            else if (option is OmronFinsTcp_Options finstcp_option)
            {
                return new OmronFinsAdapter(finstcp_option, _loggerFactory);
            }
            else if (option is OmronCIP_Options cip_option)
            {
                return new OmronCIPAdapter(cip_option, _loggerFactory);
            }
            else if (option is Mitsubishi_MC3E_Options mc3e_option)
            {
                return new Mitsubishi_MC3E_Adapter(mc3e_option, _loggerFactory);
            }
            return null;
        }
    }
}
