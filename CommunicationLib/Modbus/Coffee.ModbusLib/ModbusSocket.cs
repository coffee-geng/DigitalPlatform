using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.ModbusLib
{
    public abstract class ModbusSocket : ModbusMaster
    {
        public string IP {  get; set; }

        public int Port { get; set; }

        public int ReceiveTimeout { get; set; } = 2000;

        public int ReceiveBufferSize { get; set; } = 4096;

        public int SendTimeout { get; set; } = 2000;

        public int SendBufferSize { get; set; } = 4096;

        protected Socket? _socket {  get; set; }

        private ProtocolType _protocolType = ProtocolType.Unspecified;

        protected ModbusSocket(ProtocolType protocolType)
        {
            _protocolType = protocolType;
            initSocket(protocolType);
        }

        private void initSocket(ProtocolType protocolType)
        {
            if (protocolType == ProtocolType.Tcp)
            {
                _socket = new Socket(SocketType.Stream, protocolType);
            }
            else if (protocolType == ProtocolType.Udp)
            {
                _socket = new Socket(SocketType.Dgram, protocolType);
            }
        }

        public override void Connect()
        {
            if (_socket == null)
            {
                initSocket(_protocolType);
            }
            if (_socket == null) //初始化Socket对象失败后抛异常
            {
                throw new NullReferenceException("Socket对象创建失败！");
            }
            _socket.SendTimeout = SendTimeout;
            _socket.SendBufferSize = SendBufferSize;
            _socket.ReceiveTimeout = ReceiveTimeout;
            _socket.ReceiveBufferSize = ReceiveBufferSize;
            _socket.Connect(IP, Port);
        }

        public override void Disconnect()
        {
            if (_socket == null)
            {
                throw new NullReferenceException("Socket对象创建失败！");
            }
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _socket.Dispose();
            _socket = null;
        }

        protected virtual async Task<byte[]> SendAndReceiveAsync(byte[] bytes, int len)
        {
            return null;
        }

        protected byte[] parseReceiveBytesToRead(byte[] bytesToReceive)
        {
            if (bytesToReceive.Length < 5)
            {
                throw new Exception("接收的报文格式错误！");
            }
            if (bytesToReceive[1] > 0x80) //响应的是异常信息
            {
                byte errorCode = bytesToReceive[2]; //异常码
                if (ModbusBase.Errors.TryGetValue(errorCode, out string errorMsg))
                {
                    throw new Exception(errorMsg);
                }
                else
                {
                    throw new Exception($"读取数据错误！未知错误功能码：{errorCode.ToString("X2")}");
                }
            }
            return bytesToReceive.ToList().GetRange(3, bytesToReceive.Length - 3).ToArray();
        }

        protected void checkReceiveBytesToWrite(byte[] bytesToReceive)
        {
            if (bytesToReceive.Length < 5)
            {
                throw new Exception("接收的报文格式错误！");
            }
            if (bytesToReceive[1] > 0x80) //响应的是异常信息
            {
                byte errorCode = bytesToReceive[2]; //异常码
                if (ModbusBase.Errors.TryGetValue(errorCode, out string errorMsg))
                {
                    throw new Exception(errorMsg);
                }
                else
                {
                    throw new Exception($"写入数据错误！未知错误功能码：{errorCode.ToString("X2")}");
                }
            }
        }
    }
}
