using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.ModbusLib
{
    public class ModbusUDP : ModbusSocket
    {
        public ModbusUDP() : this("127.0.0.1")
        {
        }

        public ModbusUDP(string ip) : this(ip, 502)
        {
        }

        public ModbusUDP(string ip, int port) : base(ProtocolType.Udp)
        {
            this.IP = ip;
            this.Port = port;
        }

        public override byte[] Read(byte slaveId, FunctionAreas funcArea, ushort startAddress, ushort count)
        {
            byte[] bytesToSend = createSendBytesToRead(slaveId, funcArea, startAddress, count);

            byte[] bytesToReceive = SendAndReceive(bytesToSend, -1); //TCP协议不需要传入len
            return parseReceiveBytesToRead(bytesToReceive);
        }

        public override void Write(byte slaveId, FunctionAreas funcArea, ushort startAddress, ushort count, byte[] data)
        {
            byte[] bytesToSend = createSendBytesToWrite(slaveId, funcArea, startAddress, count, data);

            byte[] bytesToReceive = SendAndReceive(bytesToSend, -1);
            checkReceiveBytesToWrite(bytesToReceive);
        }

        public override Task ReadAsync(byte slaveId, FunctionAreas funcArea, ushort startAddress, ushort count, int transactionId, Action<ReadWriteModbusCallbackResult> callback)
        {
            byte[] bytesToSend = createSendBytesToRead(slaveId, funcArea, startAddress, count);

            var task = SendAndReceiveAsync(bytesToSend, -1);
            task.ContinueWith(t =>
            {
                if (callback == null)
                {
                    return;
                }
                Exception error = null;
                byte[] result = null;
                if (t.IsCanceled)
                {
                    error = new Exception("读取数据的任务被取消了！");
                }
                else if (t.Exception != null)
                {
                    error = t.Exception;
                }
                else if (t.IsFaulted)
                {
                    error = new Exception("读取数据的任务有异常！");
                }
                else if (t.IsCompletedSuccessfully)
                {
                    try
                    {
                        result = parseReceiveBytesToRead(t.Result);
                    }
                    catch (Exception e)
                    {
                        error = e;
                    }
                }

                callback.Invoke(new ReadWriteModbusCallbackResult()
                {
                    IsCompleted = t.IsCompletedSuccessfully && error != null,
                    Error = error,
                    ResultData = result
                });
            });
            return task;
        }

        public override Task WriteAsync(byte slaveId, FunctionAreas funcArea, ushort startAddress, ushort count, byte[] data, int transcationId, Action<ReadWriteModbusCallbackResult> callback)
        {
            byte[] bytesToSend = createSendBytesToWrite(slaveId, funcArea, startAddress, count, data);

            var task = SendAndReceiveAsync(bytesToSend, -1);
            task.ContinueWith(t =>
            {
                if (callback == null)
                {
                    return;
                }
                Exception error = null;
                byte[] result = null;
                if (t.IsCanceled)
                {
                    error = new Exception("写入数据的任务被取消了！");
                }
                else if (t.Exception != null)
                {
                    error = t.Exception;
                }
                else if (t.IsFaulted)
                {
                    error = new Exception("写入数据的任务有异常！");
                }
                else if (t.IsCompletedSuccessfully)
                {
                    try
                    {
                        checkReceiveBytesToWrite(t.Result);
                    }
                    catch (Exception e)
                    {
                        error = e;
                    }
                }

                callback.Invoke(new ReadWriteModbusCallbackResult()
                {
                    IsCompleted = t.IsCompletedSuccessfully && error != null,
                    Error = error,
                    ResultData = result
                });
            });
            return task;
        }

        private byte[] createSendBytesToRead(byte slaveId, FunctionAreas funcArea, ushort startAddress, ushort count)
        {
            //0x06表示UDP读取请求报文的PDU总数是6个字节长度
            List<byte> bytesToSend = new List<byte>()
            {
                0x00, 0x00, //协议标识，固定为0x0000
                0x00, 0x06 //表示后续报文的字节长度，2字节
            };
            var funcCode = GetFunctionCode(Functions.Read, funcArea);
            if (!funcCode.HasValue)
            {
                throw new Exception("没有找到匹配的功能码！");
            }
            bytesToSend.AddRange(GetPDUToReadRequest(slaveId, (byte)funcCode.Value, startAddress, count));
            return bytesToSend.ToArray();
        }

        private byte[] createSendBytesToWrite(byte slaveId, FunctionAreas funcArea, ushort startAddress, ushort count, byte[] data)
        {
            List<byte> bytesToSend = new List<byte>()
            {
                0x00, 0x00
            };
            var funcCode = GetFunctionCode(Functions.Read, funcArea);
            if (!funcCode.HasValue)
            {
                throw new Exception("没有找到匹配的功能码！");
            }
            //计算出UDP写入请求报文的PDU总数
            byte[] pdu = GetPDUToWriteRequest(slaveId, (byte)funcCode, startAddress, count, data);
            bytesToSend.Add((byte)(pdu.Length / 256));
            bytesToSend.Add((byte)(pdu.Length % 256));
            bytesToSend.AddRange(pdu);

            //添加写入的数据作为报文体
            bytesToSend.AddRange(data);
            return bytesToSend.ToArray();
        }

        protected override byte[] SendAndReceive(byte[] bytes, int len)
        {
            if (_socket == null || !_socket.Connected)
            {
                throw new NullReferenceException("Socket对象创建失败！");
            }
            _socket.Send(bytes, 0, bytes.Length, SocketFlags.None);

            byte[] respBytes = new byte[4]; //Socket响应报文头总是4个字节：2字节协议ID（一般为0x00）+ 2字节响应数据长度
            int count = _socket.Receive(respBytes, 0, 4, SocketFlags.None);

            if (count != 4)
            {
                throw new Exception("数据报文格式错误，不能正确识别！");
            }

            len = respBytes[2] * 256 + respBytes[3]; //2字节响应数据长度
            respBytes = new byte[len];
            count = _socket.Receive(respBytes, 0, len, SocketFlags.None);

            if (count <= 0)
            {
                throw new Exception("没有读取到响应数据，可能是网络断线了！");
            }

            return respBytes;
        }

        protected override async Task<byte[]> SendAndReceiveAsync(byte[] bytes, int len)
        {
            if (_socket == null || !_socket.Connected)
            {
                throw new NullReferenceException("Socket对象创建失败！");
            }
            await _socket.SendAsync(bytes, SocketFlags.None);

            byte[] respBytes = new byte[4]; //Socket响应报文头总是4个字节：2字节协议ID（一般为0x00）+ 2字节响应数据长度
            int count = await _socket.ReceiveAsync(respBytes, SocketFlags.None);

            if (count != 4)
            {
                throw new Exception("数据报文格式错误，不能正确识别！");
            }

            len = respBytes[2] * 256 + respBytes[3]; //2字节响应数据长度
            respBytes = new byte[len];
            count = await _socket.ReceiveAsync(respBytes, SocketFlags.None);

            if (count <= 0)
            {
                throw new Exception("没有读取到响应数据，可能是网络断线了！");
            }

            return respBytes;
        }
    }
}
