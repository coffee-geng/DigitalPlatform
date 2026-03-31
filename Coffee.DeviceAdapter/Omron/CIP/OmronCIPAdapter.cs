using Coffee.DeviceAdapter.Omron.CIP;
using Coffee.DigitalPlatform.Common;
using Coffee.Omron.Communication;
using Coffee.Omron.Communication.Base;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DeviceAdapter
{
    public class OmronCIPAdapter : IProtocolAdapter
    {
        private readonly CIP _cipClient;

        private readonly OmronCIP_Options _cipOptions;

        private readonly ILogger<OmronCIPAdapter> _logger;

        private readonly CounterInterlocked _conter = new CounterInterlocked(1);

        public OmronCIPAdapter(OmronCIP_Options cipOptions, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<OmronCIPAdapter>();

            _cipOptions = cipOptions ?? throw new ArgumentNullException(nameof(cipOptions));
            _cipClient = new CIP(cipOptions.IP.ToString(), cipOptions.Port);
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
                if (_cipOptions is null)
                    return Coffee.DeviceAdapter.ProtocolType.Unknown;
                else
                    return Coffee.DeviceAdapter.ProtocolType.OmronCIP;
            }
        }

        public bool IsConnected
        {
            get
            {
                //尝试一次连接设备的操作，如果有异常，则返回不能连接
                //工业通信中推荐使用主动的心跳机制来确保通信链路真正健康，在 PLC 和上位机之间建立一个定期变化的信号（读任意标签，并且忽略标签不存在的异常）
                return _cipClient.CanRead();
            }
        }

        public bool Connect()
        {
            try
            {
                _logger.LogInformation($"Connecting to Omron CIP device at Socket: {_cipOptions.IP} : {_cipOptions.Port}");
                _cipClient.Open();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Failed to connect to Omron CIP device at Socket: {_cipOptions.IP} : {_cipOptions.Port}");
                return false;
            }
        }

        public void Disconnect()
        {
            try
            {
                _cipClient.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to disconnect to Omron CIP device at Socket: {_cipOptions.IP} : {_cipOptions.Port}");
            }
        }

        public ProtocolData Read(string tag, DataType dataType, ushort length = 1, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            var cipData = new CipData()
            {
                DataType = dataType,
                DataLength = length,
                ProtocolType = ProtocolType
            };
            try
            {
                byte[] result = _cipClient.Read(tag);
                cipData.Bytes = result;

                try
                {
                    //反射调用泛型方法：public List<T> GetDatas<T>(byte[] bytes)
                    var method = typeof(OmronBase).GetMethod("GetDatas");
                    var genericType1 = DataTypeHelper.GetTypeFromDataType(dataType);
                    var genericMethod = method.MakeGenericMethod(genericType1);
                    cipData.Value = genericMethod.Invoke(_cipClient, new object[] { cipData.Bytes });
                }
                catch (Exception ex1)
                {
                    throw new Exception($"将返回值转换成指定类型{Enum.GetName(typeof(DataType), dataType)}时发生错误！");
                }
                cipData.Success = true;
                return cipData;
            }
            catch (Exception ex)
            {
                cipData.Success = false;
                cipData.ErrorMessage = new Exception($"读取标签为{tag}的点位数据时发生错误！", ex).Message;
                return cipData;
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

        public bool Write(string tag, DataType dataType, object value, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
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

                    //反射调用泛型方法：public byte[] GetBytes<T>(params T[] values)
                    var method2 = typeof(OmronBase).GetMethod("GetBytes");
                    var genericMethod2 = method2.MakeGenericMethod(genericType1);
                    bytesToWrite = (byte[])genericMethod2.Invoke(_cipClient, new object[] { dataArray });
                }
                catch (Exception ex1)
                {
                    throw new Exception($"写入数据不符合指定类型{Enum.GetName(typeof(DataType), dataType)}！");
                }

                var cipType = DataTypeHelper.GetCipTypeFromDataType(dataType);
                _cipClient.Write(tag, cipType, bytesToWrite);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"写入标签为{tag}的点位地址时发生错误！", ex);
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
                //欧姆龙CIP协议原生支持多区块多地址读
                Dictionary<string, Exception> varAddrWithErrorList = new Dictionary<string, Exception>();
                IList<CIP_Parameter> dataParameters = new List<CIP_Parameter>();
                Dictionary<CIP_Parameter, ReadRequestParameter> requestDataParamDict = new Dictionary<CIP_Parameter, ReadRequestParameter>();

                foreach (var requestItem in requestParam)
                {
                    try
                    {
                        CIP_Parameter dataParam = new CIP_Parameter()
                        {
                            Tag = requestItem.Address
                        };
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
                    response.ErrorMessage = new Exception($"标签为{string.Join(",", varAddrWithErrorList)}的点位地址格式不正确，无法读取！").Message;
                    return response;
                }

                try
                {
                    _cipClient.MultipleRead(dataParameters.ToArray());

                    foreach (var dataParam in dataParameters)
                    {
                        var requestPara = requestDataParamDict[dataParam];
                        var cipData = new CipData()
                        {
                            DataType = requestPara.DataType,
                            DataLength = requestPara.Length,
                            ProtocolType = ProtocolType
                        };
                        cipData.Bytes = dataParam.Data;

                        try
                        {
                            //反射调用泛型方法：public List<T> GetDatas<T>(byte[] bytes)
                            var method = typeof(OmronBase).GetMethod("GetDatas");
                            var genericType1 = DataTypeHelper.GetTypeFromDataType(cipData.DataType);
                            var genericMethod = method.MakeGenericMethod(genericType1);
                            cipData.Value = genericMethod.Invoke(_cipClient, new object[] { cipData.Bytes });
                        }
                        catch (Exception ex1)
                        {
                            varAddrWithErrorList.Add(requestPara.Address, ex1);
                        }
                        var responseParam = new ReadResponseParameter(cipData);
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
                    response.ErrorMessage = new Exception($"读取标签为{string.Join(",", requestParam.Select(p => p.Address))}的点位地址的数据时发生错误！").Message;
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
                    response.ErrorMessage = new Exception($"写入标签为{string.Join(",", varAddrWithErrorList)}的点位地址的数据时发生错误！").Message;
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
                _logger.LogInformation($"Connecting to Omron CIP device at Socket: {_cipOptions.IP} : {_cipOptions.Port}");

                return await Task.Run(() =>
                {
                    _cipClient.Open();
                    return true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to connect to Omron CIP device at Socket: {_cipOptions.IP} : {_cipOptions.Port}");
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                _logger.LogInformation($"Connecting to Omron CIP device at Socket: {_cipOptions.IP} : {_cipOptions.Port}");

                await Task.Run(() =>
                {
                    _cipClient.Close();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to connect to Omron CIP device at Socket: {_cipOptions.IP} : {_cipOptions.Port}");
            }
        }

        public async Task<ProtocolData> ReadAsync(string address, DataType dataType, ushort length = 1, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            var cipData = new CipData()
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
                        cipData.Success = result.Success;
                        cipData.ErrorMessage = result.ErrorMessage;
                        cipData.Bytes = result.Bytes;
                        cipData.Value = result.Value;
                    }
                    else
                    {
                        cipData.Success = false;
                    }
                });
            }
            catch (Exception ex)
            {
                cipData.Success = false;
                cipData.ErrorMessage = ex.ToString();
            }
            return cipData;
        }

        public async Task<ReadResponseParameter> ReadAsync(ReadRequestParameter requestParam, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            var cipData = new CipData()
            {
                DataType = requestParam.DataType,
                DataLength = requestParam.Length,
                ProtocolType = ProtocolType
            };
            var response = new ReadResponseParameter(cipData);

            try
            {
                await Task.Run(() =>
                {
                    var result = Read(requestParam, slaveId, endianTypes);
                    if (result != null)
                    {
                        response = result;
                    }
                    else
                    {
                        cipData.Success = false;
                    }
                });
            }
            catch (Exception ex)
            {
                cipData.Success = false;
                cipData.ErrorMessage = ex.Message;
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
            catch (Exception ex)
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

        public async Task<BatchReadResponseParameter> BatchReadAsync(IEnumerable<ReadRequestParameter> requestParam, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            var response = new BatchReadResponseParameter();
            try
            {
                await Task.Run(() =>
                {
                    var result = BatchRead(requestParam, slaveId, endianTypes);
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

        public async Task<BatchWriteResponseParameter> BatchWriteAsync(IEnumerable<WriteRequestParameter> requestParam, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            var response = new BatchWriteResponseParameter();
            try
            {
                await Task.Run(() =>
                {
                    var result = BatchWrite(requestParam, slaveId, endianTypes);
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
