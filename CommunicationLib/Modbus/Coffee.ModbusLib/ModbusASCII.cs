using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.ModbusLib
{
    public class ModbusASCII : ModbusSerial
    {
        public ModbusASCII() : this("COM1")
        {
        }

        public ModbusASCII(string portName) : this(portName, 9600, Parity.None, 8, StopBits.One)
        {
        }

        public ModbusASCII(string portName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
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
            byte lrc = LRC(pdu);
            bytesToSend.AddRange(pdu);
            bytesToSend.Add(lrc);

            //转ASCII字符   加头和尾
            string strToSend = ":" + string.Join("", bytesToSend.Select(x => x.ToString("X2")));
            List<byte> asciiToSend = Encoding.ASCII.GetBytes(strToSend).ToList();
            asciiToSend.Add(0x0D);
            asciiToSend.Add(0x0A);

            //计算接收响应时读取的字节数
            //读寄存器响应格式：从站地址 + 功能码 + 字节数 + 【寄存器值1[Hi, Lo] 寄存器值2[Hi, Lo] ...】 + LRC校验码
            //接收的字节长度 = (寄存器个数 * 2 + 4字节 [从站地址 + 功能码 + 字节数+ LRC校验码]) * 2字符 + 3字符(头尾)
            int len = (count * 2 + 4) * 2 + 3;
            if (funcArea == FunctionAreas.CoilsState || funcArea == FunctionAreas.InputCoils)
            {
                //读线圈响应格式：从站地址 + 功能码 + 字节数 + 【输出状态7-0 输出状态15-8 ...】 + LRC校验码
                //接收的字节长度 = ((线圈寄存器个数占有几个字节) + 4字节 [从站地址 + 功能码 + 字节数+ LRC校验码]) * 2字符 + 3字符(头尾)
                len = ((int)Math.Ceiling(count * 1.0 / 8) + 4) * 2 + 3;
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
            byte lrc = LRC(pdu);
            bytesToSend.AddRange(pdu);
            bytesToSend.Add(lrc);

            //写入寄存器或线圈的响应格式：从站地址 + 功能码 + 写入地址[Hi, Lo] + 写入数量[Hi, Lo] + LRC校验码
            //接收的字节长度 = 7 * 2字符 + 3字符(头尾)  =  17
            int len = 17; 
            return (bytesToSend.ToArray(), len);
        }

        protected override byte[] VerifyCode(byte[] response, int dataLength, Functions function)
        {
            //读取或写入数据成功，响应信息帧就是dataLength传入的字节数
            //读取或写入数据异常，响应信息帧固定11个字节，格式：(从站地址 + 错误功能码 + 异常码 + LRC校验码) * 2字符 + 3字符(头尾)
            if (response.Length == dataLength || response.Length == 11)
            {
                //检查响应信息帧是否完整 头：字符:  尾：CR,LF
                if (response[0] != 0x3A || response[response.Length - 2] != 0x0D || response[response.Length - 1] != 0x0A)
                {
                    throw new Exception("响应报文数据不完整");
                }

                //去头和尾
                List<byte> asciiToReceive = response.ToList().GetRange(1, response.Length - 3);
                //ASCII转为字符串
                string strToReceive = Encoding.ASCII.GetString(asciiToReceive.ToArray());
                //将字符串转变为字节数组
                byte[] bytesToReceive = Convert.FromHexString(strToReceive); //此方法仅在.NET Core环境下有效！如果允许环境是.NET Framework，请循环遍历并使用2个字符一组的方式转换

                List<byte> respForCheck = new List<byte>();
                var pdu = bytesToReceive.ToList().GetRange(0, bytesToReceive.Length - 1); //去除校验码
                respForCheck.AddRange(pdu);
                respForCheck.Add(LRC(pdu.ToArray()));

                if (!respForCheck.SequenceEqual(bytesToReceive))
                {
                    throw new Exception("数据传输异常，校验码不匹配！");
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
                        throw new Exception($"数据传输错误！未知错误功能码：{errorCode.ToString("X2")}");
                    }
                }

                if (function == Functions.Read) //当读取数据成功时，返回读取到的寄存器值或输出状态
                {
                    return bytesToReceive.ToList().GetRange(3, dataLength - 4).ToArray();
                }
                else if (function == Functions.Write || function == Functions.WriteSingle) //当写入数据成功时，返回写入值或写入数量（写多个线圈或寄存器）
                {
                    return bytesToReceive.ToList().GetRange(4, dataLength - 5).ToArray();
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
    }
}
