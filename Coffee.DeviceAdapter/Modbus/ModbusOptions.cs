using Coffee.ModbusLib;
using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using socket = System.Net.Sockets;

namespace Coffee.DeviceAdapter
{
    public abstract class ModbusSocketOptions : ProtocolOptions, ISocketOptions
    {
        protected ModbusSocketOptions(socket.ProtocolType protocolType)
        {
            _protocolType = protocolType;
        }

        public string IP { get; set; }

        public int Port { get; set; }

        public int ReceiveTimeout { get; set; } = 2000;

        public int ReceiveBufferSize { get; set; } = 4096;

        public int SendTimeout { get; set; } = 2000;

        public int SendBufferSize { get; set; } = 4096;


        private socket.ProtocolType _protocolType = socket.ProtocolType.Unspecified;
    }

    public abstract class ModbusSerialOptions : ProtocolOptions, ISerialPortOptions
    {
        public string PortName { get; set; } = "COM1";

        public int BaudRate { get; set; } = 9600;

        public Parity Parity { get; set; } = Parity.None;

        public int DataBits { get; set; } = 8;

        public StopBits StopBits { get; set; } = StopBits.One;

        public int ReadTimeout { get; set; } = 2000;

        public int ReadBufferSize { get; set; } = 4096;

        public int WriteTimeout { get; set; } = 2000;

        public int WriteBufferSize { get; set; } = 4096;
    }

    public class ModbusRTU_Options : ModbusSerialOptions
    {

    }

    public class ModbusTCP_Options : ModbusSocketOptions
    {
        public ModbusTCP_Options() : this("127.0.0.1")
        {
        }

        public ModbusTCP_Options(string ip) : this(ip, 502)
        {
        }

        public ModbusTCP_Options(string ip, int port) : base(socket.ProtocolType.Tcp)
        {
            this.IP = ip;
            this.Port = port;
        }
    }

    public class ModbusUDP_Options : ModbusSocketOptions
    {
        public ModbusUDP_Options() : this("127.0.0.1")
        {
        }

        public ModbusUDP_Options(string ip) : this(ip, 502)
        {
        }

        public ModbusUDP_Options(string ip, int port) : base(socket.ProtocolType.Udp)
        {
            this.IP = ip;
            this.Port = port;
        }
    }
}
