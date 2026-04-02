using Coffee.DigitalPlatform.Common;
using Coffee.ModbusLib;
using Coffee.Omron.Communication;
using Coffee.Omron.Communication.Base;
using Coffee.Siemens.Communication;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DeviceAdapter
{
    public class OmronFinsAdapter : IProtocolAdapter
    {
        private readonly FinsCommand _finsClient;

        private readonly IProtocolOptions _finsOptions;

        private readonly ILogger<OmronFinsAdapter> _logger;

        private readonly CounterInterlocked _conter = new CounterInterlocked(1);

        public OmronFinsAdapter(IProtocolOptions finsOptions, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<OmronFinsAdapter>();

            _finsOptions = finsOptions ?? throw new ArgumentNullException(nameof(finsOptions));
            if (!(_finsOptions is OmronFins_Options || _finsOptions is OmronFinsTcp_Options))
            {
                throw new ArgumentException("Invalid Omron Fins options provided.", nameof(finsOptions));
            }

            if (finsOptions is OmronFins_Options finsOption)
            {
                _finsClient = new FINS(finsOption.PortName, finsOption.BaudRate, finsOption.DataBits, finsOption.Parity, finsOption.StopBits);
            }
            else if (finsOptions is OmronFinsTcp_Options finstcpOption)
            {
                if (finstcpOption.ReceiveTimeout > 0)
                    _finsClient = new FINSTCP(finstcpOption.IP.ToString(), finstcpOption.Port, finstcpOption.ReceiveTimeout);
                else
                    _finsClient = new FINSTCP(finstcpOption.IP.ToString(), finstcpOption.Port);
            }
            else
            {
                throw new ArgumentException("Unsupported Omron Fins option type.", nameof(finsOptions));
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
                if (_finsOptions is null)
                    return Coffee.DeviceAdapter.ProtocolType.Unknown;
                if (_finsOptions is OmronFins_Options)
                    return Coffee.DeviceAdapter.ProtocolType.OmronFins;
                else if (_finsOptions is OmronFinsTcp_Options)
                    return Coffee.DeviceAdapter.ProtocolType.OmronFinsTCP;
                else
                    return Coffee.DeviceAdapter.ProtocolType.Unknown;
            }
        }

        public bool IsConnected
        {
            get
            {
                //尝试一次连接设备的操作，如果有异常，则返回不能连接
                //工业通信中推荐使用主动的心跳机制来确保通信链路真正健康，在 PLC 和上位机之间建立一个定期变化的信号（默认使用DM0000）
                try
                {
                    var finsParam = new FINS_Parameter()
                    {
                        Area = Area.DM,
                        WordAddr = 0,
                        BitAddr = 0,
                        Count = 1,
                        DataType = DataTypes.WORD
                    };
                    if (_finsClient is FINS fins)
                    {
                        byte unitAddr = (_finsOptions as FinsBaseOptions).UnitAddress;
                        var result = fins.Read(unitAddr, finsParam);
                    }
                    else if (_finsClient is FINSTCP finstcp)
                    {
                        var result = finstcp.Read(finsParam);
                    }
                    else
                    {
                        return false;
                    }
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
                if (_finsOptions is OmronFins_Options finsOptions)
                {
                    _logger.LogInformation($"Connecting to Omron Fins device at SerialPort: {finsOptions.PortName}");
                }
                else if (_finsOptions is OmronFinsTcp_Options finstcpOptions)
                {
                    _logger.LogInformation($"Connecting to Omron Fins device at Socket: {finstcpOptions.IP} : {finstcpOptions.Port}");
                }
                _finsClient.Open();
                return true;
            }
            catch (Exception ex)
            {
                if (_finsOptions is OmronFins_Options finsOptions)
                {
                    _logger.LogInformation($"Failed to connect to Omron Fins device at SerialPort: {finsOptions.PortName}");
                }
                else if (_finsOptions is OmronFinsTcp_Options finstcpOptions)
                {
                    _logger.LogInformation($"Failed to connect to Omron Fins device at Socket: {finstcpOptions.IP} : {finstcpOptions.Port}");
                }
                return false;
            }
        }

        public void Disconnect()
        {
            try
            {
                _finsClient.Close();
            }
            catch (Exception ex)
            {
                if (_finsOptions is OmronFins_Options finsOptions)
                {
                    _logger.LogError($"Failed to disconnect to Omron Fins device at SerialPort: {finsOptions.PortName}");
                }
                else if (_finsOptions is OmronFinsTcp_Options finstcpOptions)
                {
                    _logger.LogError($"Failed to disconnect to Omron Fins device at Socket: {finstcpOptions.IP} : {finstcpOptions.Port}");
                }
            }
        }

        public ProtocolData Read(string address, DataType dataType, ushort length = 1, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            var finsData = new FinsData()
            {
                DataType = dataType,
                DataLength = length,
                ProtocolType = ProtocolType
            };
            if (_finsClient is not FINS && _finsClient is not FINSTCP)
            {
                finsData.Success = false;
                finsData.ErrorMessage = $"此协议未受支持: {_finsClient.GetType().FullName}";
                return finsData;
            }
            try
            {
                if (_finsClient is FINS finsClient)
                {
                    var finsOption = _finsOptions as FinsBaseOptions;
                    byte[] result = finsClient.Read(finsOption.UnitAddress, address, length);
                    finsData.Bytes = result;
                }
                else if (_finsClient is FINSTCP finstcpClient)
                {
                    byte[] result = finstcpClient.Read(address, length);
                    finsData.Bytes = result;
                }

                try
                {
                    //反射调用泛型方法：public List<T> GetDatas<T>(byte[] bytes)
                    var method = typeof(OmronBase).GetMethod("GetDatas");
                    var genericType1 = DataTypeHelper.GetTypeFromDataType(dataType);
                    var genericMethod = method.MakeGenericMethod(genericType1);
                    finsData.Value = genericMethod.Invoke(_finsClient, new object[] { finsData.Bytes });
                }
                catch (Exception ex1)
                {
                    throw new Exception($"将返回值转换成指定类型{Enum.GetName(typeof(DataType), dataType)}时发生错误！");
                }
                finsData.Success = true;
                return finsData;
            }
            catch (Exception ex)
            {
                finsData.Success = false;
                finsData.ErrorMessage = new Exception($"读取点位地址{address}的数据时发生错误！", ex).Message;
                return finsData;
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
            if (_finsClient is not FINS && _finsClient is not FINSTCP)
            {
                throw new Exception( $"此协议未受支持: {_finsClient.GetType().FullName}");
            }

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
                    bytesToWrite = (byte[])genericMethod2.Invoke(_finsClient, new object[] { dataArray });
                }
                catch (Exception ex1)
                {
                    throw new Exception($"写入数据不符合指定类型{Enum.GetName(typeof(DataType), dataType)}！");
                }

                if (_finsClient is FINS finsClient)
                {
                    var finsOption = _finsOptions as FinsBaseOptions;
                    finsClient.Write(finsOption.UnitAddress, address, bytesToWrite);
                }
                else if (_finsClient is FINSTCP finstcpClient)
                {
                    finstcpClient.Write(address, bytesToWrite);
                }
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
                if (_finsClient is not FINS && _finsClient is not FINSTCP)
                {
                    response.Success = false;
                    response.ErrorMessage = $"此协议未受支持: {_finsClient.GetType().FullName}";
                    return response;
                }

                //欧姆龙Fins协议原生支持多区块多地址读
                Dictionary<string, Exception> varAddrWithErrorList = new Dictionary<string, Exception>();
                IList<FINS_Parameter> dataParameters = new List<FINS_Parameter>();
                Dictionary<FINS_Parameter, ReadRequestParameter> requestDataParamDict = new Dictionary<FINS_Parameter, ReadRequestParameter>();

                foreach (var requestItem in requestParam)
                {
                    try
                    {
                        FINS_Parameter dataParam = _finsClient.GetAddress(requestItem.Address);
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
                    if (_finsClient is FINS finsClient)
                    {
                        var finsOption = _finsOptions as FinsBaseOptions;
                        finsClient.MultipleRead(finsOption.UnitAddress, dataParameters.ToArray());
                    }
                    else if (_finsClient is FINSTCP finstcpClient)
                    {
                        finstcpClient.MultipleRead(dataParameters.ToArray());
                    }

                    foreach (var dataParam in dataParameters)
                    {
                        var requestPara = requestDataParamDict[dataParam];
                        var finsData = new FinsData()
                        {
                            DataType = requestPara.DataType,
                            DataLength = requestPara.Length,
                            ProtocolType = ProtocolType
                        };
                        finsData.Bytes = dataParam.Data;

                        try
                        {
                            //反射调用泛型方法：public List<T> GetDatas<T>(byte[] bytes)
                            var method = typeof(OmronBase).GetMethod("GetDatas");
                            var genericType1 = DataTypeHelper.GetTypeFromDataType(finsData.DataType);
                            var genericMethod = method.MakeGenericMethod(genericType1);
                            finsData.Value = genericMethod.Invoke(_finsClient, new object[] { finsData.Bytes });
                        }
                        catch (Exception ex1)
                        {
                            varAddrWithErrorList.Add(requestPara.Address, ex1);
                        }
                        var responseParam = new ReadResponseParameter(finsData);
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
                if (_finsOptions is OmronFins_Options finsOptions)
                {
                    _logger.LogInformation($"Connecting to Omron fins device at SerialPort: {finsOptions.PortName}");
                }
                else if (_finsOptions is OmronFinsTcp_Options finstcpOptions)
                {
                    _logger.LogInformation($"Connecting to Omron fins device at Socket: {finstcpOptions.IP} : {finstcpOptions.Port}");
                }
                return await Task.Run(() =>
                {
                    _finsClient.Open();
                    return true;
                });
            }
            catch (Exception ex)
            {
                if (_finsOptions is OmronFins_Options finsOptions)
                {
                    _logger.LogError($"Failed to connect to Omron fins device at SerialPort: {finsOptions.PortName}");
                }
                else if (_finsOptions is OmronFinsTcp_Options finstcpOptions)
                {
                    _logger.LogError($"Failed to connect to Omron fins device at Socket: {finstcpOptions.IP} : {finstcpOptions.Port}");
                }
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                if (_finsOptions is OmronFins_Options finsOptions)
                {
                    _logger.LogInformation($"Connecting to Omron fins device at SerialPort: {finsOptions.PortName}");
                }
                else if (_finsOptions is OmronFinsTcp_Options finstcpOptions)
                {
                    _logger.LogInformation($"Connecting to Omron fins device at Socket: {finstcpOptions.IP} : {finstcpOptions.Port}");
                }
                await Task.Run(() =>
                {
                    _finsClient.Close();
                });
            }
            catch (Exception ex)
            {
                if (_finsOptions is OmronFins_Options finsOptions)
                {
                    _logger.LogError($"Failed to connect to Omron fins device at SerialPort: {finsOptions.PortName}");
                }
                else if (_finsOptions is OmronFinsTcp_Options finstcpOptions)
                {
                    _logger.LogError($"Failed to connect to Omron fins device at Socket: {finstcpOptions.IP} : {finstcpOptions.Port}");
                }
            }
        }

        public async Task<ProtocolData> ReadAsync(string address, DataType dataType, ushort length = 1, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            var finsData = new FinsData()
            {
                DataType = dataType,
                DataLength = length,
                ProtocolType = ProtocolType
            };
            if (_finsClient is not FINS && _finsClient is not FINSTCP)
            {
                finsData.Success = false;
                finsData.ErrorMessage = $"此协议未受支持: {_finsClient.GetType().FullName}";
                return finsData;
            }
            try
            {
                await Task.Run(() =>
                {
                    var result = Read(address, dataType, length, slaveId, endianTypes);
                    if (result != null)
                    {
                        finsData.Success = result.Success;
                        finsData.ErrorMessage = result.ErrorMessage;
                        finsData.Bytes = result.Bytes;
                        finsData.Value = result.Value;
                    }
                    else
                    {
                        finsData.Success = false;
                    }
                });
            }
            catch (Exception ex)
            {
                finsData.Success = false;
                finsData.ErrorMessage = ex.ToString();
            }
            return finsData;
        }

        public async Task<ReadResponseParameter> ReadAsync(ReadRequestParameter requestParam, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            var finsData = new FinsData()
            {
                DataType = requestParam.DataType,
                DataLength = requestParam.Length,
                ProtocolType = ProtocolType
            };
            var response = new ReadResponseParameter(finsData);
            if (_finsClient is not FINS && _finsClient is not FINSTCP)
            {
                finsData.Success = false;
                finsData.ErrorMessage = $"此协议未受支持: {_finsClient.GetType().FullName}";
                return response;
            }

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
                        finsData.Success = false;
                    }
                });
            }
            catch (Exception ex)
            {
                finsData.Success = false;
                finsData.ErrorMessage = ex.Message;
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
