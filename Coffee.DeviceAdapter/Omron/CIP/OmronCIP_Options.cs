using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using socket = System.Net.Sockets;

namespace Coffee.DeviceAdapter
{
    public class OmronCIP_Options : ProtocolOptions, ISocketOptions
    {
        public OmronCIP_Options() : this("127.0.0.1", 44818)
        {
        }

        protected OmronCIP_Options(socket.ProtocolType protocolType = socket.ProtocolType.Tcp)
        {
            _protocolType = protocolType;
        }

        protected OmronCIP_Options(string ipAddr, int port, socket.ProtocolType protocolType = socket.ProtocolType.Tcp)
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
