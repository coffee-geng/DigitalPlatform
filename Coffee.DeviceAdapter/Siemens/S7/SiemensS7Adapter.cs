using Coffee.DeviceAdapter;
using Coffee.DigitalPlatform.Common;
using Coffee.ModbusLib;
using Coffee.Siemens.Communication;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DeviceAdapter
{
    public class SiemensS7Adapter : IProtocolAdapter
    {
        private readonly S7Client _s7Client;

        private readonly SiemensS7_Options _s7Options;

        private readonly ILogger<SiemensS7Adapter> _logger;

        private readonly CounterInterlocked _conter = new CounterInterlocked(1);

        public SiemensS7Adapter(SiemensS7_Options s7Options, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<SiemensS7Adapter>();

            _s7Options = s7Options ?? throw new ArgumentNullException(nameof(s7Options));
            _s7Client = new S7Client();            
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
                if (_s7Options is null)
                    return Coffee.DeviceAdapter.ProtocolType.Unknown;
                if (_s7Options is SiemensS7_Options)
                    return Coffee.DeviceAdapter.ProtocolType.SiemensS7;
                else
                    return Coffee.DeviceAdapter.ProtocolType.Unknown;
            }
        }

        public bool IsConnected
        {
            get
            {
                //尝试一次连接设备的操作，如果有异常，则返回不能连接
                //工业通信中推荐使用主动的心跳机制来确保通信链路真正健康，在 PLC 和上位机之间建立一个定期变化的信号（默认使用DB1.DBW0）
                try
                {
                    var result = _s7Client.Read("DB1.DBW0");
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
                string ip = _s7Options.IP.ToString();
                //int port = _s7Options.Port; 端口默认是102，不用传参
                byte rack = _s7Options.Rack;
                byte slot = _s7Options.Slot;
                int timeout = _s7Options.ReceiveTimeout;
                if (timeout > 0)
                    _s7Client.Connect(ip, rack, slot, timeout);
                else
                    _s7Client.Connect(ip, rack, slot);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Failed to connect to Siemens S7 device at Socket: {_s7Options.IP.ToString()} : 102");
                return false;
            }
        }

        public void Disconnect()
        {
            try
            {
                _s7Client.Disconnect();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to disconnect to Siemens S7 device at Socket: {_s7Options.IP.ToString()} : 102");
            }
        }

        public ProtocolData Read(string address, DataType dataType, ushort length = 1, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            var s7Data = new SiemensS7Data()
            {
                DataType = dataType,
                DataLength = length,
                ProtocolType = ProtocolType
            };
            try
            {
                byte[] result = _s7Client.Read(address, length);
                s7Data.Bytes = result;

                try
                {
                    //反射调用泛型方法：public List<T> BytesToData<T>(byte[] bytes, int count = 0)
                    var method = typeof(S7Client).GetMethod("BytesToData");
                    var genericType1 = DataTypeHelper.GetTypeFromDataType(dataType);
                    var genericMethod = method.MakeGenericMethod(genericType1);
                    s7Data.Value = genericMethod.Invoke(null, new object[] { result, length });
                }
                catch (Exception ex1)
                {
                    throw new Exception($"将返回值转换成指定类型{Enum.GetName(typeof(DataType), dataType)}时发生错误！");
                }
                s7Data.Success = true;
                return s7Data;
            }
            catch (Exception ex)
            {
                s7Data.Success = false;
                s7Data.ErrorMessage = new Exception($"读取点位地址{address}的数据时发生错误！", ex).Message;
                return s7Data;
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

                    //反射调用泛型方法：public byte[] DataToBytes<T>(params T[] values)
                    var method2 = typeof(S7Client).GetMethod("DataToBytes");
                    var genericMethod2 = method2.MakeGenericMethod(genericType1);
                    bytesToWrite = (byte[])genericMethod2.Invoke(_s7Client, new object[] { dataArray });
                }
                catch (Exception ex1)
                {
                    throw new Exception($"写入数据不符合指定类型{Enum.GetName(typeof(DataType), dataType)}！");
                }

                _s7Client.Write(address, bytesToWrite);
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
            catch (Exception ex)
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
                //西门子S7协议原生支持多区块多地址读写
                Dictionary<string, Exception> varAddrWithErrorList = new Dictionary<string, Exception>();
                IList<DataParameter> dataParameters = new List<DataParameter>();
                Dictionary<DataParameter, ReadRequestParameter> requestDataParamDict = new Dictionary<DataParameter, ReadRequestParameter>();

                foreach (var requestItem in requestParam)
                {
                    try
                    {
                        DataParameter dataParam = _s7Client.ParseRequestAddress(requestItem.Address, requestItem.Length, S7_Functions.ReadVariable);
                        dataParameters.Add(dataParam);
                        requestDataParamDict.Add(dataParam, requestItem);
                    }
                    catch (Exception ex)
                    {
                        varAddrWithErrorList.Add(requestItem.Address, ex);
                    }
                }
                if (varAddrWithErrorList.Any())
                {
                    response.Success = false;
                    response.ErrorMessage = new Exception($"点位地址{string.Join(",", varAddrWithErrorList)}格式不正确，无法读取！").Message;
                    return response;
                }

                try
                {
                    _s7Client.Read(dataParameters.ToArray());

                    foreach (var dataParam in dataParameters)
                    {
                        var requestPara = requestDataParamDict[dataParam];
                        var s7Data = new SiemensS7Data()
                        {
                            DataType = requestPara.DataType,
                            DataLength = requestPara.Length,
                            ProtocolType = ProtocolType
                        };
                        s7Data.Bytes = dataParam.DataBytes;

                        try
                        {
                            //反射调用泛型方法：public List<T> BytesToData<T>(byte[] bytes, int count = 0)
                            var method = typeof(S7Client).GetMethod("BytesToData");
                            var genericType1 = DataTypeHelper.GetTypeFromDataType(s7Data.DataType);
                            var genericMethod = method.MakeGenericMethod(genericType1);
                            s7Data.Value = genericMethod.Invoke(_s7Client, new object[] { s7Data.Bytes, s7Data.DataLength });
                        }
                        catch (Exception ex1)
                        {
                            varAddrWithErrorList.Add(requestPara.Address, ex1);
                        }
                        var responseParam = new ReadResponseParameter(s7Data);
                        response.Results.Add(responseParam);
                    }
                    if (varAddrWithErrorList.Any())
                    {
                        response.Success = false;
                        response.ErrorMessage = new Exception($"将返回值转换成指定类型时发生错误！").Message;
                    }
                    else
                    {
                        response.Success = true;
                    }
                }
                catch (Exception ex)
                {
                    response.Success = false;
                    response.ErrorMessage = new Exception($"读取点位地址{string.Join(",", requestParam.Select(p => p.Address))}的数据时发生错误！").Message;
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
                //西门子S7协议原生支持多区块多地址读写
                Dictionary<string, Exception> varAddrWithErrorDict = new Dictionary<string, Exception>();
                IList<DataParameter> dataParameters = new List<DataParameter>();
                Dictionary<DataParameter, WriteRequestParameter> requestDataParamDict = new Dictionary<DataParameter, WriteRequestParameter>();

                foreach (var requestItem in requestParam)
                {
                    int count = 0;
                    byte[] bytesToWrite = new byte[0];
                    var genericType1 = DataTypeHelper.GetTypeFromDataType(requestItem.DataType);
                    try
                    {
                        //反射调用泛型方法：public static T[] ConvertToDataArray<T>(object value)
                        var method1 = typeof(DataTypeHelper).GetMethod("ConvertToDataArray");
                        var genericMethod1 = method1.MakeGenericMethod(genericType1);
                        dynamic dataArray = genericMethod1.Invoke(null, new object[] { requestItem.Value });
                        count = dataArray.Length;

                        //反射调用泛型方法：public byte[] DataToBytes<T>(params T[] values)
                        var method2 = typeof(S7Client).GetMethod("DataToBytes");
                        var genericMethod2 = method2.MakeGenericMethod(genericType1);
                        bytesToWrite = (byte[])genericMethod2.Invoke(_s7Client, new object[] { dataArray });
                    }
                    catch (Exception ex1)
                    {
                        varAddrWithErrorDict.Add(requestItem.Address, new Exception($"写入数据不符合指定类型{Enum.GetName(typeof(DataType), requestItem.DataType)}！"));
                        continue;
                    }

                    try
                    {
                        //系统仅支持写入一个Bit的数据，但可以写入多个Byte/Word/DWord的数据。
                        //因为解析写入地址前，不知道写入数据类型，默认是写Bit数据，所以传入参数1
                        DataParameter dataParam = _s7Client.ParseRequestAddress(requestItem.Address, 1, S7_Functions.WriteVariable);

                        if (dataParam.ParameterVarType == S7_ParameterVarType.BYTE)
                        {
                            dataParam.Count = bytesToWrite.Length;
                        }
                        else if (dataParam.ParameterVarType == S7_ParameterVarType.WORD)
                        {
                            if (bytesToWrite.Length % 2 > 0)
                            {
                                throw new Exception("写入数据格式有误，提供的字节数不符号'一个WORD类型占用2个字节'的要求！");
                            }
                            dataParam.Count = bytesToWrite.Length / 2;
                        }
                        else if (dataParam.ParameterVarType == S7_ParameterVarType.DWORD)
                        {
                            if (bytesToWrite.Length % 4 > 0)
                            {
                                throw new Exception("写入数据格式有误，提供的字节数不符号'一个DWORD类型占用4个字节'的要求！");
                            }
                            dataParam.Count = bytesToWrite.Length / 4;
                        }
                        dataParam.DataBytes = bytesToWrite;

                        dataParameters.Add(dataParam);
                        requestDataParamDict.Add(dataParam, requestItem);
                    }
                    catch (Exception ex)
                    {
                        varAddrWithErrorDict.Add(requestItem.Address, ex);
                    }
                }

                try
                {
                    _s7Client.Write(dataParameters.ToArray());
                }
                catch (Exception ex)
                {
                    foreach(var requestItem in requestParam)
                    {
                        if (!varAddrWithErrorDict.ContainsKey(requestItem.Address))
                            varAddrWithErrorDict.Add(requestItem.Address, ex);
                        else
                            varAddrWithErrorDict[requestItem.Address] = ex;
                    }
                }

                if (varAddrWithErrorDict.Any())
                {
                    response.Success = false;
                    response.ErrorMessage = new Exception($"写入点位地址{string.Join(",", varAddrWithErrorDict.Keys.ToList())}的数据时发生错误！").Message;
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
                _logger.LogInformation($"Connecting to Siemens S7 device at Socket: {_s7Options.IP.ToString()} : 102");
                return await Task.Run(() =>
                {
                    string ip = _s7Options.IP.ToString();
                    //int port = _s7Options.Port; 端口默认是102，不用传参
                    byte rack = _s7Options.Rack;
                    byte slot = _s7Options.Slot;
                    int timeout = _s7Options.ReceiveTimeout;
                    if (timeout > 0)
                        _s7Client.Connect(ip, rack, slot, timeout);
                    else
                        _s7Client.Connect(ip, rack, slot);
                    return true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to disconnect to Siemens S7 device at Socket: {_s7Options.IP.ToString()} : 102");
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                _logger.LogInformation($"Disconnecting to Siemens S7 device at Socket: {_s7Options.IP.ToString()} : 102");
                await Task.Run(() =>
                {
                    _s7Client.Disconnect();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to disconnect to Siemens S7 device at Socket: {_s7Options.IP.ToString()} : 102");
            }
        }

        //由于Siemens S7协议级别的异步访问方式在本项目中还没有实现，目前只是简单封装了异步任务的方式，以保持系统所有的协议都能问异步进行访问。
        public async Task<ProtocolData> ReadAsync(string address, DataType dataType, ushort length = 1, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            var s7Data = new SiemensS7Data()
            {
                DataType = dataType,
                DataLength = length,
                ProtocolType = ProtocolType
            };
            try
            {
                await Task.Run(() =>
                {
                    var result = Read(address, dataType, length, slaveId, endianTypes);
                    if (result != null)
                    {
                        s7Data.Success = result.Success;
                        s7Data.ErrorMessage = result.ErrorMessage;
                        s7Data.Bytes = result.Bytes;
                        s7Data.Value = result.Value;
                    }
                    else
                    {
                        s7Data.Success = false;
                    }
                });
            }
            catch (Exception ex)
            {
                s7Data.Success = false;
                s7Data.ErrorMessage = ex.ToString();
            }
            return s7Data;
        }

        public async Task<ReadResponseParameter> ReadAsync(ReadRequestParameter requestParameter, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            var s7Data = new SiemensS7Data()
            {
                DataType = requestParameter.DataType,
                DataLength = requestParameter.Length,
                ProtocolType = ProtocolType
            };
            var response = new ReadResponseParameter(s7Data);
            try
            {
                await Task.Run(() =>
                {
                    var result = Read(requestParameter, slaveId, endianTypes);
                    if (result != null)
                    {
                        response = result;
                    }
                    else
                    {
                        s7Data.Success = false;
                    }
                });
            }
            catch (Exception ex)
            {
                s7Data.Success = false;
                s7Data.ErrorMessage = ex.Message;
            }
            return response;
        }

        public async Task<bool> WriteAsync(string address, DataType dataType, object value, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            bool success = false;
            try
            {
                await Task.Run(() =>
                {
                    success = Write(address, dataType, value, slaveId, endianTypes);
                });
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return success;
        }

        public async Task<WriteResponseParameter> WriteAsync(WriteRequestParameter requestParam, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            var response = new WriteResponseParameter()
            {
                ProtocolType = ProtocolType,
                DataType = requestParam.DataType,
                Value = requestParam.Value
            };
            try
            {
                await Task.Run(() =>
                {
                    var result = Write(requestParam, slaveId, endianTypes);
                    if (result != null)
                    {
                        response = result;
                    }
                    else
                    {
                        response.Success = false;
                    }
                });
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = ex.Message;
            }
            return response;
        }

        public async Task<BatchReadResponseParameter> BatchReadAsync(IEnumerable<ReadRequestParameter> requestParameters, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            var response = new BatchReadResponseParameter();
            try
            {
                await Task.Run(() =>
                {
                    var result = BatchRead(requestParameters, slaveId, endianTypes);
                    if (result != null)
                    {
                        response = result;
                    }
                    else
                    {
                        response.Success = false;
                    }
                });
            }
            catch(Exception ex)
            {
                throw ex;
            }
            return response;
        }

        public async Task<BatchWriteResponseParameter> BatchWriteAsync(IEnumerable<WriteRequestParameter> requestParameters, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            var response = new BatchWriteResponseParameter();
            try
            {
                await Task.Run(() =>
                {
                    var result = BatchWrite(requestParameters, slaveId, endianTypes);
                    if (result != null)
                    {
                        response = result;
                    }
                    else
                    {
                        response.Success = false;
                    }
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }
        #endregion
    }
}
