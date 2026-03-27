using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using socket = System.Net.Sockets;

namespace Coffee.DeviceAdapter
{
    public class SiemensS7_Options : ProtocolOptions, ISocketOptions
    {
        public IPAddress IP { get; set; }

        public int Port { get; set; }

        public int ReceiveTimeout { get; set; } = 2000;

        public int ReceiveBufferSize { get; set; } = 4096;

        public int SendTimeout { get; set; } = 2000;

        public int SendBufferSize { get; set; } = 4096;

        public byte Rack { get; set; }

        public byte Slot { get; set; }

        private socket.ProtocolType _protocolType = socket.ProtocolType.Unspecified;
    }
}
