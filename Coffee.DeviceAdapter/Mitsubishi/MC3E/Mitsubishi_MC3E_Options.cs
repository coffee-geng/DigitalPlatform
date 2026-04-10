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
    [EndianMode(EndianMode.BigLittleEndian)]
    public class Mitsubishi_MC3E_Options : ProtocolOptions, ISocketOptions
    {
        public IPAddress IP { get; set; }

        public int Port { get; set; }

        public int ReceiveTimeout { get; set; } = 2000;

        public int ReceiveBufferSize { get; set; } = 4096;

        public int SendTimeout { get; set; } = 2000;

        public int SendBufferSize { get; set; } = 4096;

        public byte Rack { get; set; }
        public byte Slot { get; set; }

        //XY地址是否使用八进制，默认true为八进制
        //FX3U系列PLC的XY地址是八进制，Q系列PLC的XY地址是十六进制
        public bool IsOctal { get; set; } = true;

        private socket.ProtocolType _protocolType = socket.ProtocolType.Unspecified;
    }
}
