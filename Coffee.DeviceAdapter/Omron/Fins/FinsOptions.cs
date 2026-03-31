using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using socket = System.Net.Sockets;

namespace Coffee.DeviceAdapter
{
    public abstract class FinsBaseOptions : ProtocolOptions
    {
        //节点地址
        //在网络层面唯一标识一个PLC设备。当多台设备（如多个PLC、电脑）连接到同一个网络中时，每个设备都需要一个唯一的节点地址。
        public byte NodeAddress { get; set; } = 0x00;

        //单元地址
        //在PLC内部唯一标识一个特定的通信单元。一个PLC可能有多个通信端口（如CPU自带的串口、扩展的通信板、以太网模块等），每个端口都有一个单元号。
        public byte UnitAddress { get; set; } = 0x00;
    }

    public class FinsOptions : FinsBaseOptions, ISerialPortOptions
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

    public class FinsTcpOptions : FinsBaseOptions, ISocketOptions
    {
        protected FinsTcpOptions(socket.ProtocolType protocolType)
        {
            _protocolType = protocolType;
        }

        protected FinsTcpOptions(socket.ProtocolType protocolType, string ipAddr, int port)
        {
            _protocolType = protocolType;
            IP = IPAddress.Parse(ipAddr);
            Port = port;
        }

        public IPAddress IP { get; set; }

        public int Port { get; set; }

        public int ReceiveTimeout { get; set; } = 2000;

        public int ReceiveBufferSize { get; set; } = 4096;

        public int SendTimeout { get; set; } = 2000;

        public int SendBufferSize { get; set; } = 4096;


        private socket.ProtocolType _protocolType = socket.ProtocolType.Unspecified;
    }
}
