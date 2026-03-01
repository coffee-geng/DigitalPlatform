using Coffee.DigitalPlatform.Common;
using Coffee.ModbusLib;
using Microsoft.Extensions.Logging;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DeviceAdapter
{
    public class ModbusAdapter : IProtocolAdapter
    {
        private readonly ModbusMaster _modbusClient;

        private readonly IProtocolOptions _modbusOptions;

        private readonly ILogger<ModbusAdapter> _logger;

        private readonly CounterInterlocked _conter = new CounterInterlocked(1);

        public ModbusAdapter(IProtocolOptions modbusOptions, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<ModbusAdapter>();

            _modbusOptions = modbusOptions ?? throw new ArgumentNullException(nameof(modbusOptions));
            if (!(_modbusOptions is ModbusSerialOptions || _modbusOptions is ModbusSocketOptions))
            {
                throw new ArgumentException("Invalid Modbus options provided.", nameof(modbusOptions));
            }

            if (modbusOptions is ModbusTCP_Options tcpOptions)
            {
                _modbusClient = new ModbusTCP(tcpOptions.IP, tcpOptions.Port);
            }
            else if (modbusOptions is ModbusUDP_Options udpOptions)
            {
                _modbusClient = new ModbusUDP(udpOptions.IP, udpOptions.Port);
            }
            else if (modbusOptions is ModbusSerialOptions serialOptions)
            {
                _modbusClient = new ModbusRTU(serialOptions.PortName, serialOptions.BaudRate, serialOptions.Parity, serialOptions.DataBits, serialOptions.StopBits);
            }
            else
            {
                throw new ArgumentException("Unsupported Modbus option type.", nameof(modbusOptions));
            }
        }

        public void Dispose()
        {
            Disconnect();
        }

        #region IProtocolAdapter接口
        public ProtocolType ProtocolType
        {
            get
            {
                if (_modbusOptions is null)
                    return Coffee.DeviceAdapter.ProtocolType.Unknown;
                if (_modbusOptions is ModbusTCP_Options)
                    return Coffee.DeviceAdapter.ProtocolType.ModbusTCP;
                else if (_modbusOptions is ModbusUDP_Options)
                    return Coffee.DeviceAdapter.ProtocolType.ModbusUDP;
                else if (_modbusOptions is ModbusSerialOptions)
                    return Coffee.DeviceAdapter.ProtocolType.ModbusRTU;
                else
                    return Coffee.DeviceAdapter.ProtocolType.Unknown; 
            }
        }

        public bool IsConnected
        {
            get 
            {
                //尝试一次连接设备的操作，如果有异常，则返回不能连接
                try
                {
                    var result = _modbusClient.Read(1, FunctionAreas.HoldingRegister, 1, 1);
                    return true;
                }
                catch (Exception ex)
                {
                    return false;
                }
            }
        }

        public bool Connect()
        {
            try
            {
                if (_modbusOptions is ModbusSerialOptions serialOptions)
                {
                    _logger.LogInformation($"Connecting to Modbus device at SerialPort: {serialOptions.PortName}");
                }
                else if (_modbusOptions is ModbusSocketOptions socketOptions)
                {
                    _logger.LogInformation($"Connecting to Modbus device at Socket: {socketOptions.IP} : {socketOptions.Port}");
                }
                _modbusClient.Connect();
                return true;
            }
            catch(Exception ex)
            {
                if (_modbusOptions is ModbusSerialOptions serialOptions)
                {
                    _logger.LogInformation($"Failed to connect to Modbus device at SerialPort: {serialOptions.PortName}");
                }
                else if (_modbusOptions is ModbusSocketOptions socketOptions)
                {
                    _logger.LogInformation($"Failed to connect to Modbus device at Socket: {socketOptions.IP} : {socketOptions.Port}");
                }
                return false;
            }
        }

        public void Disconnect()
        {
            try
            {
                _modbusClient.Disconnect();
            }
            catch (Exception ex)
            {
                if (_modbusOptions is ModbusSerialOptions serialOptions)
                {
                    _logger.LogError($"Failed to disconnect to Modbus device at SerialPort: {serialOptions.PortName}");
                }
                else if (_modbusOptions is ModbusSocketOptions socketOptions)
                {
                    _logger.LogError($"Failed to disconnect to Modbus device at Socket: {socketOptions.IP} : {socketOptions.Port}");
                }
            }
        }

        public ProtocolData Read(string address, DataType dataType, ushort length = 1, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            var modbusData = new ModbusData()
            {
                DataType = dataType,
                DataLength = length,
                ProtocolType = ProtocolType
            };
            try
            {

                byte[] result = _modbusClient.Read(slaveId, address, length);
                modbusData.Bytes = result;

                try
                {
                    //反射调用泛型方法：public static T[] ConvertDataFromBytes<T>(byte[] bytes, EndianTypes endianType)
                    var method = typeof(ModbusMaster).GetMethod("ConvertDataFromBytes");
                    var genericType1 = DataTypeHelper.GetTypeFromDataType(dataType);
                    var genericMethod = method.MakeGenericMethod(genericType1);
                    modbusData.Value = genericMethod.Invoke(null, new object[] { result, endianTypes });
                }
                catch(Exception ex1)
                {
                    throw new Exception($"将返回值转换成指定类型{Enum.GetName(typeof(DataType), dataType)}时发生错误！");
                }
                modbusData.Success = true;
                return modbusData;
            }
            catch (Exception ex)
            {
                modbusData.Success = false;
                modbusData.ErrorMessage = new Exception($"读取点位地址{address}的数据时发生错误！", ex).Message;
                return modbusData;
            }
        }

        public ReadResponseParameter Read(ReadRequestParameter requestParam, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            if (requestParam == null)
                throw new ArgumentNullException("读取点位信息的请求参数不能为空！");
            var result = Read(requestParam.Address, requestParam.DataType, requestParam.Length, slaveId, endianTypes);
            var response = new ReadResponseParameter(result);
            return response;
        }

        public bool Write(string address, DataType dataType, object value, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            try
            {
                int count = 0;
                byte[] bytesToWrite = new byte[0];
                var genericType1 = DataTypeHelper.GetTypeFromDataType(dataType);
                try
                {

                    //反射调用泛型方法：public static T[] ConvertToDataArray<T>(object value)
                    var method1 = typeof(DataTypeHelper).GetMethod("ConvertToDataArray");
                    var genericMethod1 = method1.MakeGenericMethod(genericType1);
                    dynamic dataArray = genericMethod1.Invoke(null, new object[] { value });
                    count = dataArray.Length;

                    //反射调用泛型方法：public static byte[] ConvertBytesFromData<T>(T[] data, EndianTypes endianType)
                    var method2 = typeof(ModbusMaster).GetMethod("ConvertBytesFromData");
                    var genericMethod2 = method2.MakeGenericMethod(genericType1);
                    bytesToWrite = (byte[])genericMethod2.Invoke(null, new object[] {dataArray , endianTypes });
                }
                catch (Exception ex1)
                {
                    throw new Exception($"写入数据不符合指定类型{Enum.GetName(typeof(DataType), dataType)}！");
                }

                _modbusClient.Write(slaveId, address, (ushort)count, bytesToWrite);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"写入点位地址{address}时发生错误！", ex);
            }
        }

        public WriteResponseParameter Write(WriteRequestParameter requestParam, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            if (requestParam == null)
                throw new ArgumentNullException("写入点位信息的请求参数不能为空！");
            var response = new WriteResponseParameter()
            {
                ProtocolType = ProtocolType,
                DataType = requestParam.DataType,
                Value = requestParam.Value
            };
            try
            {
                var result = Write(requestParam.Address, requestParam.DataType, requestParam.Value, slaveId, endianTypes);
                response.Success = result;
            }
            catch(Exception ex)
            {
                response.ErrorMessage = ex.Message;
            }
            return response;
        }

        public BatchReadResponseParameter BatchRead(IEnumerable<ReadRequestParameter> requestParam, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            if (requestParam == null || !requestParam.Any())
                throw new ArgumentNullException("读取点位信息的请求参数不能为空！");
            var response = new BatchReadResponseParameter();
            if (requestParam.Count() == 1)
            {
                var resp = Read(requestParam.First(), slaveId, endianTypes);
                response.Success = resp.Success;
                response.ErrorMessage = resp.ErrorMessage;
                response.Results.Add(resp);
                return response;
            }
            else
            {
                List<string> varAddrWithErrorList = new List<string>();
                foreach(var requestItem in requestParam)
                {
                    var resp = Read(requestItem, slaveId, endianTypes);
                    response.Results.Add(resp);
                    if (!resp.Success)
                    {
                        varAddrWithErrorList.Add(requestItem.Address);
                    }
                }
                if (varAddrWithErrorList.Any())
                {
                    response.Success = false;
                    response.ErrorMessage = new Exception($"读取点位地址{string.Join(",", varAddrWithErrorList)}的数据时发生错误！").Message;
                }
                else
                {
                    response.Success = true;
                }
            }
            return response;
        }

        public BatchWriteResponseParameter BatchWrite(IEnumerable<WriteRequestParameter> requestParam, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            if (requestParam == null || !requestParam.Any())
                throw new ArgumentNullException("写入点位信息的请求参数不能为空！");
            var response = new BatchWriteResponseParameter();
            if (requestParam.Count() == 1)
            {
                var resp = Write(requestParam.First(), slaveId, endianTypes);
                response.Success = resp.Success;
                response.ErrorMessage = resp.ErrorMessage;
                response.Results.Add(resp);
                return response;
            }
            else
            {
                List<string> varAddrWithErrorList = new List<string>();
                foreach (var requestItem in requestParam)
                {
                    var resp = Write(requestItem, slaveId, endianTypes);
                    response.Results.Add(resp);
                    if (!resp.Success)
                    {
                        varAddrWithErrorList.Add(requestItem.Address);
                    }
                }
                if (varAddrWithErrorList.Any())
                {
                    response.Success = false;
                    response.ErrorMessage = new Exception($"写入点位地址{string.Join(",", varAddrWithErrorList)}的数据时发生错误！").Message;
                }
                else
                {
                    response.Success = true;
                }
            }
            return response;
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                if (_modbusOptions is ModbusSerialOptions serialOptions)
                {
                    _logger.LogInformation($"Connecting to Modbus device at SerialPort: {serialOptions.PortName}");
                }
                else if (_modbusOptions is ModbusSocketOptions socketOptions)
                {
                    _logger.LogInformation($"Connecting to Modbus device at Socket: {socketOptions.IP} : {socketOptions.Port}");
                }
                return await Task.Run(() =>
                {
                    _modbusClient.Connect();
                    return true;
                });
                
            }
            catch (Exception ex)
            {
                if (_modbusOptions is ModbusSerialOptions serialOptions)
                {
                    _logger.LogError($"Failed to connect to Modbus device at SerialPort: {serialOptions.PortName}");
                }
                else if (_modbusOptions is ModbusSocketOptions socketOptions)
                {
                    _logger.LogError($"Failed to connect to Modbus device at Socket: {socketOptions.IP} : {socketOptions.Port}");
                }
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                if (_modbusOptions is ModbusSerialOptions serialOptions)
                {
                    _logger.LogInformation($"Disconnecting to Modbus device at SerialPort: {serialOptions.PortName}");
                }
                else if (_modbusOptions is ModbusSocketOptions socketOptions)
                {
                    _logger.LogInformation($"Disconnecting to Modbus device at Socket: {socketOptions.IP} : {socketOptions.Port}");
                }
                await Task.Run(() =>
                {
                    _modbusClient.Connect();
                });
            }
            catch (Exception ex)
            {
                if (_modbusOptions is ModbusSerialOptions serialOptions)
                {
                    _logger.LogError($"Failed to disconnect to Modbus device at SerialPort: {serialOptions.PortName}");
                }
                else if (_modbusOptions is ModbusSocketOptions socketOptions)
                {
                    _logger.LogError($"Failed to disconnect to Modbus device at Socket: {socketOptions.IP} : {socketOptions.Port}");
                }
            }
        }

        public async Task<ProtocolData> ReadAsync(string address, DataType dataType, ushort length = 1, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            var tcs = new TaskCompletionSource<ProtocolData>();

            var modbusData = new ModbusData()
            {
                DataType = dataType,
                DataLength = length,
                ProtocolType = ProtocolType
            };
            try
            {
                ushort registerCount = GetRegisterCount(dataType, length);
                await _modbusClient.ReadAsync(slaveId, address, registerCount, _conter.GetAndIncrement(), r =>
                {
                    if (r.IsCompleted && r.Error == null)
                    {
                        try
                        {
                            //反射调用泛型方法：public static T[] ConvertDataFromBytes<T>(byte[] bytes, EndianTypes endianType)
                            var method = typeof(ModbusMaster).GetMethod("ConvertDataFromBytes");
                            var genericType1 = DataTypeHelper.GetTypeFromDataType(dataType);
                            var genericMethod = method.MakeGenericMethod(genericType1);
                            modbusData.Value = genericMethod.Invoke(null, new object[] { r.ResultData, endianTypes });

                            modbusData.Success = true;
                            modbusData.Bytes = r.ResultData;
                        }
                        catch (Exception ex1)
                        {
                            var innerExp = new Exception($"将返回值转换成指定类型{Enum.GetName(typeof(DataType), dataType)}时发生错误！");
                            modbusData.Success = false;
                            modbusData.ErrorMessage = new Exception($"读取点位地址{address}的数据时发生错误！", ex1).Message;
                        }
                    }
                    else
                    {
                        modbusData.Success = false;
                        modbusData.ErrorMessage = new Exception($"读取点位地址{address}的数据时发生错误！").Message;
                    }

                    tcs.SetResult(modbusData);
                });
                return tcs.Task.Result;
            }
            catch (Exception ex)
            {
                modbusData.Success = false;
                modbusData.ErrorMessage = new Exception($"读取点位地址{address}的数据时发生错误！", ex).Message;
                return modbusData;
            }
        }

        public async Task<ReadResponseParameter> ReadAsync(ReadRequestParameter requestParam, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            if (requestParam == null)
                throw new ArgumentNullException("读取点位信息的请求参数不能为空！");
            var result = await ReadAsync(requestParam.Address, requestParam.DataType, requestParam.Length, slaveId, endianTypes);
            var response = new ReadResponseParameter(result);
            return response;
        }

        public async Task<bool> WriteAsync(string address, DataType dataType, object value, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            var tcs = new TaskCompletionSource<bool>();

            try
            {
                ushort count = 0; //计算出写入数据占用的寄存器个数
                byte[] bytesToWrite = new byte[0];
                var genericType1 = DataTypeHelper.GetTypeFromDataType(dataType);
                try
                {
                    //反射调用泛型方法：public static T[] ConvertToDataArray<T>(object value)
                    var method1 = typeof(DataTypeHelper).GetMethod("ConvertToDataArray");
                    var genericMethod1 = method1.MakeGenericMethod(genericType1);
                    dynamic dataArray = genericMethod1.Invoke(null, new object[] { value });
                    //count = dataArray.Length;
                    count = GetRegisterCount(dataType, dataArray);

                    //反射调用泛型方法：public static byte[] ConvertBytesFromData<T>(T[] data, EndianTypes endianType)
                    var method2 = typeof(ModbusMaster).GetMethod("ConvertBytesFromData");
                    var genericMethod2 = method2.MakeGenericMethod(genericType1);
                    bytesToWrite = (byte[])genericMethod2.Invoke(null, new object[] { dataArray, endianTypes });
                }
                catch (Exception ex1)
                {
                    throw new Exception($"写入数据不符合指定类型{Enum.GetName(typeof(DataType), dataType)}！");
                }

                await _modbusClient.WriteAsync(slaveId, address, (ushort)count, bytesToWrite, _conter.GetAndIncrement(), r =>
                {
                    if (r.IsCompleted && r.Error == null)
                    {
                        tcs.SetResult(true);
                    }
                    else
                    {
                        tcs.SetResult(false);
                        tcs.SetException(new Exception($"写入点位地址{address}时发生错误！"));
                    }
                });
                return tcs.Task.Result;
            }
            catch (Exception ex)
            {
                throw new Exception($"写入点位地址{address}时发生错误！", ex);
            }
        }

        public async Task<WriteResponseParameter> WriteAsync(WriteRequestParameter requestParam, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            if (requestParam == null)
                throw new ArgumentNullException("写入点位信息的请求参数不能为空！");
            var response = new WriteResponseParameter()
            {
                ProtocolType = ProtocolType,
                DataType = requestParam.DataType,
                Value = requestParam.Value
            };
            try
            {
                var result = await WriteAsync(requestParam.Address, requestParam.DataType, requestParam.Value, slaveId, endianTypes);
            }
            catch (Exception ex)
            {
                response.ErrorMessage = ex.Message;
            }
            return response;
        }

        public async Task<BatchReadResponseParameter> BatchReadAsync(IEnumerable<ReadRequestParameter> requestParam, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            if (requestParam == null || !requestParam.Any())
                throw new ArgumentNullException("读取点位信息的请求参数不能为空！");
            var response = new BatchReadResponseParameter();
            if (requestParam.Count() == 1)
            {
                var resp = await ReadAsync(requestParam.First(), slaveId, endianTypes);
                response.Success = resp.Success;
                response.ErrorMessage = resp.ErrorMessage;
                response.Results.Add(resp);
                return response;
            }
            else
            {
                List<string> varAddrWithErrorList = new List<string>();
                foreach (var requestItem in requestParam)
                {
                    var resp = await ReadAsync(requestItem, slaveId, endianTypes);
                    response.Results.Add(resp);
                    if (!resp.Success)
                    {
                        varAddrWithErrorList.Add(requestItem.Address);
                    }
                }
                if (varAddrWithErrorList.Any())
                {
                    response.Success = false;
                    response.ErrorMessage = new Exception($"读取点位地址{string.Join(",", varAddrWithErrorList)}的数据时发生错误！").Message;
                }
                else
                {
                    response.Success = true;
                }
            }
            return response;
        }

        public async Task<BatchWriteResponseParameter> BatchWriteAsync(IEnumerable<WriteRequestParameter> requestParam, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            if (requestParam == null || !requestParam.Any())
                throw new ArgumentNullException("写入点位信息的请求参数不能为空！");
            var response = new BatchWriteResponseParameter();
            if (requestParam.Count() == 1)
            {
                var resp = await WriteAsync(requestParam.First(), slaveId, endianTypes);
                response.Success = resp.Success;
                response.ErrorMessage = resp.ErrorMessage;
                response.Results.Add(resp);
                return response;
            }
            else
            {
                List<string> varAddrWithErrorList = new List<string>();
                foreach (var requestItem in requestParam)
                {
                    var resp = await WriteAsync(requestItem, slaveId, endianTypes);
                    response.Results.Add(resp);
                    if (!resp.Success)
                    {
                        varAddrWithErrorList.Add(requestItem.Address);
                    }
                }
                if (varAddrWithErrorList.Any())
                {
                    response.Success = false;
                    response.ErrorMessage = new Exception($"写入点位地址{string.Join(",", varAddrWithErrorList)}的数据时发生错误！").Message;
                }
                else
                {
                    response.Success = true;
                }
            }
            return response;
        }
        #endregion

        /// <summary>
        /// 根据数据类型和数据个数，计算出此类型在Modbus中占据的寄存器个数。
        /// </summary>
        /// <param name="dataType">数据类型</param>
        /// <param name="length">数据个数，如果是数据类型是字符串，则数据个数就是这个字符串的字符数；如果类型是字节数组，则其就是数组中的元素个数</param>
        /// <param name="encoding">当数据类型是字符串，则其指定字符串的编码方式。默认是ASCII</param>
        /// <returns></returns>
        public static ushort GetRegisterCount(DataType dataType, ushort length)
        {
            int count = 0; //寄存器个数
            switch (dataType)
            {
                case DataType.Byte:
                    count = (int)Math.Ceiling(length * 1.0 / 2);
                    break;
                case DataType.Int16:
                case DataType.UInt16:
                    count = length;
                    break;
                case DataType.Int32:
                case DataType.UInt32:
                case DataType.Float:
                    count = length * 2;
                    break;
                case DataType.Double:
                    count = length * 4;
                    break;
                case DataType.Bit:
                    count = length; //一个bit占用一个线圈寄存器
                    break;
                default:
                    throw new Exception($"数据类型{Enum.GetName(typeof(DataType), dataType)}不受支持，无法计算寄存器个数！");
            }
            if (count > ushort.MaxValue)
            {
                throw new Exception($"根据数据类型{Enum.GetName(typeof(DataType), dataType)}和数据个数{length}计算出的寄存器个数{count}超过了short类型的最大值！");
            }
            return (ushort)count;
        }

        public static ushort GetRegisterCount(DataType dataType, object data, Encoding encoding = null)
        {
            var genericType1 = DataTypeHelper.GetTypeFromDataType(dataType);
            if (genericType1 == typeof(string))
            {
                if (data is string str)
                {
                    return GetRegisterCountByString(str, encoding);
                }
                else
                {
                    throw new Exception($"参数{nameof(data)}提供的数据类型与参数{nameof(dataType)}指定的类型{Enum.GetName(typeof(DataType), dataType)}不匹配！");
                }
            }
            else if (genericType1 == typeof(byte[]))
            {
                if (data is byte[] bytes)
                {
                    return GetRegisterCountByPrimitiveType<byte>(bytes);
                }
                else
                {
                    throw new Exception($"参数{nameof(data)}提供的数据类型与参数{nameof(dataType)}指定的类型{Enum.GetName(typeof(DataType), dataType)}不匹配！");
                }
            }
            else
            {
                if (data != null)
                {
                    if (data.GetType() == genericType1 || (DataTypeHelper.TryGetIEnumerableElementType(data, out Type elementType) && elementType == genericType1))
                    {
                        //反射调用泛型方法：private static short GetRegisterCountByPrimitiveType<T>(T[] dataArray)
                        var method2 = typeof(ModbusAdapter).GetMethod("GetRegisterCountByPrimitiveType", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
                        var genericMethod2 = method2.MakeGenericMethod(genericType1);

                        ushort count = 0;
                        if (data.GetType() == genericType1) //如果data是单个数据对象
                        {
                            var array = Array.CreateInstance(genericType1, 1);
                            array.SetValue(data, 0);
                            count = (ushort)genericMethod2.Invoke(null, new object[] { array });
                        }
                        else //集合对象
                        {
                            count = (ushort)genericMethod2.Invoke(null, new object[] { data });
                        }
                        return count;
                    }
                    else
                    {
                        throw new Exception($"参数{nameof(data)}提供的数据类型与参数{nameof(dataType)}指定的类型{Enum.GetName(typeof(DataType), dataType)}不匹配！");
                    }
                }
                else
                {
                    return 0;
                }
            }
        }

        private static ushort GetRegisterCountByPrimitiveType<T>(T[] dataArray)
        {
            if (dataArray == null)
                return 0;
            int byteCount = Marshal.SizeOf<T>() * dataArray.Length;
            int count = (int)Math.Ceiling((double)byteCount / 2); //寄存器数
            if (count > ushort.MaxValue)
            {
                throw new Exception($"根据数据类型{typeof(T).Name}和数据个数{dataArray.Length}计算出的寄存器个数{count}超过了short类型的最大值！");
            }
            return (ushort)count;
        }

        private static ushort GetRegisterCountByString(string str, Encoding encoding = null)
        {
            int byteCount = 0;
            //字符串类型占用的寄存器个数需要根据字符串的字符数及其编码方式来定，这里的length就是字符的个数
            if (encoding == null || encoding == Encoding.ASCII)
            {
                byteCount = str.Length;
            }
            else if (encoding == Encoding.Unicode || encoding == Encoding.BigEndianUnicode) //UTF16
            {
                byteCount = str.Length * 2;
            }
            else if (encoding == Encoding.UTF8) //UTF-8是变长编码，一个字符可能占用1到4个字节
            {
                byteCount = encoding.GetByteCount(str);
            }
            int count = (int)Math.Ceiling((double)byteCount / 2); //寄存器数
            if (count > ushort.MaxValue)
            {
                throw new Exception($"根据当前字符串计算出的寄存器个数超过了short类型的最大值！");
            }
            return (ushort)count;
        }
    }
}
