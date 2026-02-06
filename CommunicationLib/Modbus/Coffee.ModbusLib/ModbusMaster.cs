using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.ModbusLib
{
    public abstract class ModbusMaster
    {
        internal ConcurrentQueue<RequestTransaction> requestTransactionQueue = new ConcurrentQueue<RequestTransaction>();

        public virtual void Connect()
        {
        }

        public virtual void Disconnect()
        {
        }

        /// <summary>
        /// 读取数据。
        /// </summary>
        /// <param name="slaveId">从站地址</param>
        /// <param name="funcArea">读取的功能区域</param>
        /// <param name="startAddress">读取的起始地址</param>
        /// <param name="count">读取的寄存器个数（8个线圈一个字节，1个寄存器2个字节）</param>
        /// <returns>返回读取的结果</returns>
        public virtual byte[] Read(byte slaveId, FunctionAreas funcArea, ushort startAddress, ushort count)
        {
            return null;
        }

        /// <summary>
        /// 异步读取数据。读取的结果在传入的回调函数中返回。
        /// </summary>
        /// <param name="slaveId">从站地址</param>
        /// <param name="funcArea">读取的功能区域</param>
        /// <param name="startAddress">读取的起始地址</param>
        /// <param name="count">读取的寄存器个数（8个线圈一个字节，1个寄存器2个字节）</param>
        /// <param name="transactionId">异步调用操作的标识符，用于回调接收结果时进行筛选</param>
        /// <param name="callback">回调函数，用户接受读取请求后返回的响应数据</param>
        /// <returns>返回当前任务</returns>
        public virtual Task ReadAsync(byte slaveId, FunctionAreas funcArea, ushort startAddress, ushort count, int transactionId, Action<ReadWriteModbusCallbackResult> callback)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 写入数据。
        /// </summary>
        /// <param name="slaveId">从站地址</param>
        /// <param name="funcArea">写入的功能区域</param>
        /// <param name="startAddress">写入的起始地址</param>
        /// <param name="count">写入的寄存器个数（8个线圈一个字节，1个寄存器2个字节）</param>
        /// <param name="data">写入的数据</param>
        public virtual void Write(byte slaveId, FunctionAreas funcArea, ushort startAddress, ushort count, byte[] data)
        {
        }

        /// <summary>
        /// 异步写入数据。写入的结果在传入的回调函数中返回。
        /// </summary>
        /// <param name="slaveId">从站地址</param>
        /// <param name="funcArea">写入的功能区域</param>
        /// <param name="startAddress">写入的起始地址</param>
        /// <param name="count">写入的寄存器个数（8个线圈一个字节，1个寄存器2个字节）</param>
        /// <param name="data">写入的数据</param>
        /// <param name="transcationId">异步调用操作的标识符，用于回调接收结果时进行筛选</param>
        /// <param name="callback">回调函数，用户接受写入请求后返回的响应数据</param>
        /// <returns>返回当前任务</returns>
        public virtual Task WriteAsync(byte slaveId, FunctionAreas funcArea, ushort startAddress, ushort count, byte[] data, int transcationId, Action<ReadWriteModbusCallbackResult> callback)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 读取数据。
        /// </summary>
        /// <param name="slaveId">从站地址</param>
        /// <param name="variableAddress">字符串格式的读取数据的起始地址</param>
        /// <param name="count">读取数据的寄存器个数</param>
        /// <param name="isZeroBase">是否从零开始计算</param>
        /// <returns>以字节数组格式返回读取的数据</returns>
        public byte[] Read(byte slaveId, string variableAddress, ushort count, bool isZeroBase = true)
        {
            (FunctionAreas, int) addr = ParseViriableAddress(variableAddress, isZeroBase);
            return Read(slaveId, addr.Item1, (ushort)addr.Item2, count);
        }

        /// <summary>
        /// 写入数据。写入失败时，由抛出的异常信息判别出错原因。
        /// </summary>
        /// <param name="slaveId">从站地址</param>
        /// <param name="variableAddress">字符串格式的写入数据的起始地址</param>
        /// <param name="count">写入数据的寄存器个数</param>
        /// <param name="data">写入数据</param>
        /// <param name="isZeroBase">是否从零开始计算</param>
        public void Write(byte slaveId, string variableAddress, ushort count, byte[] data, bool isZeroBase = true)
        {
            (FunctionAreas, int) addr = ParseViriableAddress(variableAddress, isZeroBase);
            Write(slaveId, addr.Item1, (ushort)addr.Item2, count, data);
        }

        public virtual Task ReadAsync(byte slaveId, string variableAddress, ushort count, int transactionId, Action<ReadWriteModbusCallbackResult> callback)
        {
            return Task.CompletedTask;
        }

        public virtual Task WriteAsync(byte slaveId, string variableAddress, ushort count, byte[] data, int transcationId, Action<ReadWriteModbusCallbackResult> callback)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// 主站发送读取或写入数据的请求，并返回从站处理后的响应。
        /// </summary>
        /// <param name="bytes">字节数组格式的请求</param>
        /// <param name="len">读取或写入的寄存器个数</param>
        /// <returns>返回响应字节数组</returns>
        protected virtual byte[] SendAndReceive(byte[] bytes, int len)
        {
            return null;
        }

        /// <summary>
        /// 解析字符串格式的请求地址。格式是功能码+请求地址
        /// </summary>
        /// <param name="variableAddress">字符串格式的请求地址</param>
        /// <param name="isZeroBase">是否从零开始计算</param>
        /// <returns>返回功能码和整数格式地址的元组</returns>
        protected (FunctionAreas, int) ParseViriableAddress(string variableAddress, bool isZeroBase = true)
        {
            if (string.IsNullOrWhiteSpace(variableAddress) || variableAddress.Length < 2)
            {
                throw new ArgumentNullException("操作数地址格式不正确，无法识别！");
            }
            FunctionAreas? funcArea = null;
            char area = variableAddress[0];
            switch (area)
            {
                case '0':
                    funcArea = FunctionAreas.CoilsState;
                    break;
                case '1':
                    funcArea = FunctionAreas.InputCoils;
                    break;
                case '3':
                    funcArea = FunctionAreas.InputRegister;
                    break;
                case '4':
                    funcArea = FunctionAreas.HoldingRegister;
                    break;
            }
            if (!funcArea.HasValue)
            {
                throw new ArgumentNullException("操作数地址格式不正确，无法识别！");
            }

            if (!int.TryParse(variableAddress.Substring(1), out int addr))
            {
                throw new ArgumentNullException("操作数地址格式不正确，无法识别！");
            }
            if (isZeroBase)
            {
                addr -= 1;
            }
            return (funcArea.Value, addr);
        }

        /// <summary>
        /// 组装出一个发送读取数据操作的请求字节数组。
        /// </summary>
        /// <param name="slaveId">从站地址</param>
        /// <param name="funcCode">功能码</param>
        /// <param name="startAddress">读取数据的起始地址</param>
        /// <param name="count">读取数据的寄存器个数</param>
        /// <returns></returns>
        protected byte[] GetPDUToReadRequest(byte slaveId, byte funcCode, ushort startAddress, ushort count)
        {
            return new byte[]
            {
                slaveId,
                funcCode,
                (byte)(startAddress / 256), //hi
                (byte)(startAddress % 256), //low
                (byte)(count / 256), //hi 读取寄存器个数
                (byte)(count % 256), //low
            };
        }

        /// <summary>
        /// 组装出一个发送写入数据操作的请求字节数组。
        /// </summary>
        /// <param name="slaveId">从站地址</param>
        /// <param name="funcCode">功能码</param>
        /// <param name="startAddress">写入数据的起始地址</param>
        /// <param name="count">写入数据的寄存器个数</param>
        /// <param name="data">写入数据的字节数，根据写入线圈还是寄存器，字节数的计算方法不一样</param>
        /// <returns></returns>
        protected byte[] GetPDUToWriteRequest(byte slaveId, byte funcCode, ushort startAddress, int count, byte[] data)
        {
            //写入字节数，如果写入线圈，则字节数 = Math.Ceil(寄存器个数/8)
            //如果写入寄存器，则字节数 = 寄存器个数 * 2
            //因为不知道是写入线圈还是寄存器，所以字节数由调用方法传入
            List<byte> pduBytes = new List<byte>()
            {
                slaveId,
                funcCode,
                (byte)(startAddress / 256), //hi
                (byte)(startAddress % 256), //low
                (byte)(count / 256), //hi 写入寄存器个数
                (byte)(count % 256), //low
                (byte)(data.Length) //写入字节数
            };
            pduBytes.AddRange(data);
            return pduBytes.ToArray();
        }

        /// <summary>
        /// 将字节数组转换为值类型。
        /// </summary>
        /// <typeparam name="T">指定值类型的泛型</typeparam>
        /// <param name="bytes">转换前的字节数组</param>
        /// <param name="endianType">转换的字节序</param>
        /// <returns>转换后的值类型数组</returns>
        public static T[] ConvertDataFromBytes<T>(byte[] bytes, EndianTypes endianType)
        {
            if (bytes == null || bytes.Length == 0)
            {
                throw new ArgumentNullException("没有需要进行类型转换的原数据！");
            }
            List<T> data = new List<T>();
            if (typeof(T) == typeof(bool))
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    for (int j = 0; j < 8; i++)
                    {
                        //对某个字节的每一数据位进行与操作，即可知道当前这个位数据是否有值
                        dynamic bit = (bytes[i] & (1 << j)) > 0;
                        data.Add(bit);
                    }
                }
            }
            else
            {
                int byteSize = Marshal.SizeOf(typeof(T)); //得到类型的字节数
                if (bytes.Length % byteSize > 0)
                {
                    throw new ArgumentException("需要进行类型转换的原数据是非法格式！");
                }
                //通过反射，得到类BitConverter提供的指定T返回值的数据类型转换方法
                MethodInfo[] methods = typeof(BitConverter).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                MethodInfo method = methods.FirstOrDefault(m => m.ReturnType == typeof(T) && m.GetParameters().Count() == 2);
                if (method == null)
                {
                    throw new Exception("数据类型转换出错！未找到匹配的数据转换方法！");
                }

                for (int i = 0; i < bytes.Length; i += byteSize)
                {
                    var dataVal1 = bytes.ToList().GetRange(i, byteSize); //转换前的某一个数据
                    //字节数组转换为值类型时，先进行字节序转换，再进行大小端转换
                    var dataVal2 = SwitchEndianType(dataVal1, endianType); //转换字节序
                    //Modbus是大端排序，BitConverter的转换方法根据操作系统环境不同，可能是大端或小端排序
                    //如果是小端排序，要进行数据反转
                    if (BitConverter.IsLittleEndian)
                    {
                        dataVal2 = dataVal2.Reverse().ToArray();
                    }
                    //进行数据类型转换
                    try
                    {
                        T value = (T)method.Invoke(null, new object[] { dataVal2, 0 });
                        data.Add(value);
                    }
                    catch
                    {
                        throw new Exception("数据类型转换出错！未找到匹配的数据转换方法！");
                    }
                }
            }
            return data.ToArray();
        }

        /// <summary>
        /// 将值类型转换为字节数组。
        /// </summary>
        /// <typeparam name="T">指定值类型的泛型</typeparam>
        /// <param name="data">转换前的值类型数组</param>
        /// <param name="endianType">转换的字节序</param>
        /// <returns>转换后的字节数组</returns>
        public static byte[] ConvertBytesFromData<T>(T[] data, EndianTypes endianType)
        {
            List<byte> bytes = new List<byte>();
            if (typeof(T) == typeof(bool))
            {
                byte start = 0x00; //传入的布尔值数组可能转换成多个字节，变量定义的是转换后的某个字节状态
                for (int i = 0; i < data.Length; i++)
                {
                    var value = data[i];
                    byte bit = (byte)(bool.Parse(value.ToString()) ? 1 : 0);
                    //通过对数据位的或运算，将布尔值数组转换为字节。必须先左移N位，以便对第N位数据位进行或操作。
                    //因为每8个位是一个字节，如果布尔值数组超过8，则会转换成多个字节，所以当对第8位进行或操作后，要重置，以便将剩下来的布尔值转换生成到下一个字节。
                    bit = (byte)(bit << (i % 8));
                    start |= bit;

                    if ((i % 8) == 7)
                    {
                        bytes.Add(start);
                        start = 0x00;
                    }
                }
                //传入的布尔值数组的数据位个数在转换成整数个字节后，还有剩余的数据位
                if (data.Length % 8 > 0)
                {
                    bytes.Add(start);
                }
            }
            else
            {
                foreach (dynamic value in data)
                {
                    byte[] valueBytes = BitConverter.GetBytes(value);
                    //值类型转换为字节数组时，先进行大小端转换，再进行字节序转换
                    if (BitConverter.IsLittleEndian)
                    {
                        valueBytes.Reverse();
                    }
                    valueBytes = SwitchEndianType(valueBytes.ToList(), endianType);
                    bytes.AddRange(valueBytes);
                }
            }
            return bytes.ToArray();
        }

        /// <summary>
        /// 进行字节序转换。
        /// </summary>
        /// <param name="data">转换前字节数组</param>
        /// <param name="endianType">指定字节序</param>
        /// <returns>转换后字节数组</returns>
        private static byte[] SwitchEndianType(List<byte> data, EndianTypes endianType)
        {
            if (data == null || data.Count == 0)
            {
                throw new ArgumentNullException("没有进行字节序转换的原数据！");
            }
            if (data.Count <= 2)
            {
                return data.ToArray();
            }
            if (data.Count != 4 && data.Count != 8)
            {
                throw new ArgumentException("进行字节序转换的原数据是非法格式！");
            }
            switch (endianType)
            {
                case EndianTypes.ABCD:
                case EndianTypes.ABCDEFGH:
                    return data.ToArray();
                case EndianTypes.DCBA:
                case EndianTypes.HGFEDCBA:
                    data.Reverse();
                    return data.ToArray();

                case EndianTypes.CDAB:
                    if (data.Count == 4)
                        return new byte[] { data[2], data[3], data[0], data[1] };
                    else
                        throw new ArgumentException("进行字节序转换的原数据是非法格式！");
                case EndianTypes.BADC:
                    if (data.Count == 4)
                        return new byte[] { data[1], data[0], data[3], data[2] };
                    else
                        throw new ArgumentException("进行字节序转换的原数据是非法格式！");
                case EndianTypes.GHEFCDAB:
                    if (data.Count == 8)
                        return new byte[] { data[6], data[7], data[4], data[5], data[2], data[3], data[0], data[1] };
                    else
                        throw new ArgumentException("进行字节序转换的原数据是非法格式！");
                case EndianTypes.BADCFEHG:
                    if (data.Count == 8)
                        return new byte[] { data[1], data[0], data[3], data[2], data[5], data[4], data[7], data[6] };
                    else
                        throw new ArgumentException("进行字节序转换的原数据是非法格式！");
            }
            return data.ToArray();
        }

        protected static FunctionCodes? GetFunctionCode(Functions func, FunctionAreas funcArea)
        {
            FunctionCodes? funcCode = null;
            switch (funcArea)
            {
                case FunctionAreas.CoilsState:
                    if (func == Functions.Read)
                        funcCode = FunctionCodes.ReadCoilsState;
                    else if (func == Functions.Write)
                        funcCode = FunctionCodes.WriteCoilsState;
                    else if (func == Functions.WriteSingle)
                        funcCode = FunctionCodes.WriteSingleCoilsState;
                    break;
                case FunctionAreas.InputCoils:
                    if (func == Functions.Read)
                        funcCode = FunctionCodes.ReadInputCoils;
                    break;
                case FunctionAreas.InputRegister:
                    if (func == Functions.Read)
                        funcCode = FunctionCodes.ReadInputRegister;
                    break;
                case FunctionAreas.HoldingRegister:
                    if (func == Functions.Read)
                        funcCode = FunctionCodes.ReadHoldingRegister;
                    else if (func == Functions.Write)
                        funcCode = FunctionCodes.WriteHoldingRegister;
                    else if (func == Functions.WriteSingle)
                        funcCode = FunctionCodes.WriteSingleHoldingRegister;
                    break;
            }
            return funcCode;
        }

        #region TransactionID自增

        private static readonly object _lockObj = new object();
        int _transactionId = 0;

        protected int CreateTransactionId()
        {
            lock (_lockObj)
            {
                _transactionId++;
                _transactionId = _transactionId % 65536; //保证transactionId是2个字节
                return _transactionId;
            }
        }
        #endregion
    }
}
