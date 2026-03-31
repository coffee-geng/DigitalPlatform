using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.Omron.Communication.Base
{
    internal class SocketSender
    {
        Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

        string _host;
        int _port;

        public SocketSender(string host, int port)
        {
            _host = host;
            _port = port;
        }

        public int ResponseTimeOut
        {
            get => socket.ReceiveTimeout;
            set => socket.ReceiveTimeout = value;
        }

        public IPAddress IP { get => (socket.LocalEndPoint as IPEndPoint).Address; }

        public void Open()
        {
            if (socket == null)
                socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

            socket.Connect(_host, _port);
        }

        public void Close() 
        {
            if (socket == null)
                throw new Exception("Socket没有创建成功！");
            socket.Close();
            socket.Dispose();
        }

        public byte[] SendAndReceive(byte[] data)
        {
            socket.Send(data);
            // 获取
            byte[] resp = new byte[1024];
            socket.Receive(resp, 0, 1024, SocketFlags.None);

            return resp;
        }
    }
}
