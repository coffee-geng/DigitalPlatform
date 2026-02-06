using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.ModbusLib
{
    public class ModbusRTU : ModbusSerial
    {
        public ModbusRTU() : this("COM1")
        {
        }

        public ModbusRTU(string portName) : this(portName, 9600, Parity.None, 8, StopBits.One)
        {
        }

        public ModbusRTU(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits) : base()
        {
            this.PortName = portName;
            this.BaudRate = baudRate;
            this.Parity = parity;
            this.DataBits = dataBits;
            this.StopBits = stopBits;
        }

        public override byte[] Read(byte slaveId, FunctionAreas funcArea, ushort startAddress, ushort count)
        {
            var pair = createSendBytesToRead(slaveId, funcArea, startAddress, count);
            byte[] bytesToSend = pair.Item1;
            int len = pair.Item2;

            byte[] bytesToReceive = SendAndReceive(bytesToSend, len);
            return VerifyCode(bytesToReceive, len, Functions.Read);
        }

        public override Task ReadAsync(byte slaveId, FunctionAreas funcArea, ushort startAddress, ushort count, int transactionId, Action<ReadWriteModbusCallbackResult> callback)
        {
            var pair = createSendBytesToRead(slaveId, funcArea, startAddress, count);
            byte[] bytesToSend = pair.Item1;
            int len = pair.Item2;

            return SendAndReceiveAsync(bytesToSend, len, Functions.Read, callback);
        }

        public override void Write(byte slaveId, FunctionAreas funcArea, ushort startAddress, ushort count, byte[] data)
        {
            var pair = createSendBytesToWrite(slaveId, funcArea, startAddress, count, data);
            byte[] bytesToSend = pair.Item1;
            int len = pair.Item2;

            byte[] bytesToReceive = SendAndReceive(bytesToSend, len);
            VerifyCode(bytesToReceive, len, Functions.Read);
        }

        public override Task WriteAsync(byte slaveId, FunctionAreas funcArea, ushort startAddress, ushort count, byte[] data, int transcationId, Action<ReadWriteModbusCallbackResult> callback)
        {
            var pair = createSendBytesToWrite(slaveId, funcArea, startAddress, count, data);
            byte[] bytesToSend = pair.Item1;
            int len = pair.Item2;

            return SendAndReceiveAsync(bytesToSend, len, Functions.Write, callback);
        }

        private (byte[], int) createSendBytesToRead(byte slaveId, FunctionAreas funcArea, ushort startAddress, ushort count)
        {
            List<byte> bytesToSend = new List<byte>();
            var funcCode = GetFunctionCode(Functions.Read, funcArea);
            if (!funcCode.HasValue)
            {
                throw new Exception("没有找到匹配的功能码！");
            }
            byte[] pdu = GetPDUToReadRequest(slaveId, (byte)funcCode.Value, startAddress, count);
            byte[] crc = CRC16(pdu.ToList()).Reverse().ToArray();
            bytesToSend.AddRange(pdu);
            bytesToSend.AddRange(crc);

            //计算接收响应时读取的字节数
            int len = count * 2 + 5; //读寄存器响应格式：从站地址 + 功能码 + 字节数 + 【寄存器值1[Hi, Lo] 寄存器值2[Hi, Lo] ...】 + CRC16校验码（2字节）
            if (funcArea == FunctionAreas.CoilsState || funcArea == FunctionAreas.InputCoils)
            {
                len = (int)Math.Ceiling(count * 1.0 / 8) + 5; //读线圈响应格式：从站地址 + 功能码 + 字节数 + 【输出状态7-0 输出状态15-8 ...】 + CRC16校验码（2字节）
            }
            return (bytesToSend.ToArray(), len);
        }

        private (byte[], int) createSendBytesToWrite(byte slaveId, FunctionAreas funcArea, ushort startAddress, ushort count, byte[] data)
        {
            List<byte> bytesToSend = new List<byte>();
            var funcCode = GetFunctionCode(Functions.Write, funcArea);
            if (!funcCode.HasValue)
            {
                throw new Exception("没有找到匹配的功能码！");
            }
            byte[] pdu = GetPDUToWriteRequest(slaveId, (byte)funcCode.Value, startAddress, count, data);
            byte[] crc = CRC16(pdu.ToList()).Reverse().ToArray();
            bytesToSend.AddRange(pdu);
            bytesToSend.AddRange(crc);

            int len = 8; //写多寄存器或多线圈的响应消息帧都是固定8个字节，格式为：从站地址 + 功能码 + 写入地址[Hi, Lo] + 写入数量[Hi, Lo] + CRC16校验码（2字节）
            return (bytesToSend.ToArray(), len);
        }

        protected override byte[] VerifyCode(byte[] response, int dataLength, Functions function)
        {
            //读取或写入数据成功，响应信息帧就是dataLength传入的字节数
            //读取或写入数据异常，响应信息帧固定5个字节，格式：从站地址 + 错误功能码 + 异常码 + CRC16校验码（2字节）
            if (response.Length == dataLength || response.Length == 5)
            {
                List<byte> respForCheck = new List<byte>();
                var pdu = response.ToList().GetRange(0, response.Length - 2); //去除校验码
                respForCheck.AddRange(pdu);
                respForCheck.AddRange(CRC16(pdu).Reverse());

                if (!respForCheck.SequenceEqual(response))
                {
                    throw new Exception("数据传输异常，校验码不匹配！");
                }

                if (response[1] > 0x80) //响应的是异常信息
                {
                    byte errorCode = response[2]; //异常码
                    if (ModbusBase.Errors.TryGetValue(errorCode, out string errorMsg))
                    {
                        throw new Exception(errorMsg);
                    }
                    else
                    {
                        throw new Exception($"数据传输错误！未知错误功能码：{errorCode.ToString("X2")}");
                    }
                }

                if (function == Functions.Read) //当读取数据成功时，返回读取到的寄存器值或输出状态
                {
                    return response.ToList().GetRange(3, dataLength - 5).ToArray();
                }
                else if (function == Functions.Write || function == Functions.WriteSingle) //当写入数据成功时，返回写入值或写入数量（写多个线圈或寄存器）
                {
                    return response.ToList().GetRange(4, dataLength - 6).ToArray();
                }
                else
                {
                    throw new Exception($"功能码{function.ToString("X2")}不支持！");
                }
            }
            else
            {
                throw new Exception("通信响应异常！");
            }
        }

        private byte[] parseReceiveBytesToRead(byte[] bytesToReceive)
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
    }
}
