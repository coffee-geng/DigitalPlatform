using Coffee.DigitalPlatform.Common;
using Coffee.Mitsubishi;
using Coffee.Mitsubishi.Base;
using Coffee.ModbusLib;
using Coffee.Siemens.Communication;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using mitsubishi= Coffee.Mitsubishi.Base;

namespace Coffee.DeviceAdapter
{
    public class Mitsubishi_MC3E_Adapter : IProtocolAdapter
    {
        private readonly Mc3E _mc3eClient;

        private readonly Mitsubishi_MC3E_Options _mc3eOptions;

        private readonly ILogger<Mitsubishi_MC3E_Adapter> _logger;

        private readonly CounterInterlocked _conter = new CounterInterlocked(1);

        public Mitsubishi_MC3E_Adapter(Mitsubishi_MC3E_Options mc3eOptions, ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));
            _logger = loggerFactory.CreateLogger<Mitsubishi_MC3E_Adapter>();

            _mc3eOptions = mc3eOptions ?? throw new ArgumentNullException(nameof(mc3eOptions));
            _mc3eClient = new Mc3E(mc3eOptions.IP.ToString(), mc3eOptions.Port);
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
                if (_mc3eOptions is null)
                    return Coffee.DeviceAdapter.ProtocolType.Unknown;
                if (_mc3eOptions is Mitsubishi_MC3E_Options)
                    return Coffee.DeviceAdapter.ProtocolType.Mitsubishi_MC3E;
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
                    var result = _mc3eClient.Read("D0", 1);
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
                int timeout = _mc3eOptions.ReceiveTimeout;
                if (timeout > 0)
                    _mc3eClient.Open(timeout);
                else
                    _mc3eClient.Open();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Failed to connect to Mitsubishi MC3E device at Socket: {_mc3eOptions.IP.ToString()} : {_mc3eOptions.Port}");
                return false;
            }
        }

        public void Disconnect()
        {
            try
            {
                _mc3eClient.Close();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to disconnect to Mitsubishi MC3E device at Socket: {_mc3eOptions.IP.ToString()} : {_mc3eOptions.Port}");
            }
        }

        public ProtocolData Read(string address, DataType dataType, ushort length = 1, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            var mc3eData = new MC3E_Data()
            {
                DataType = dataType,
                DataLength = length,
                ProtocolType = ProtocolType
            };
            RequestType requestType = dataType == DataType.Bit ? RequestType.BIT : RequestType.WORD;
            //按字读取几个字或按位读取几个位
            ushort count = length;
            if (requestType == RequestType.WORD)
            {
                // 按字从D区读取N个字，1个字1个字地址
                // 按字从Y区读取N个字，1个字16个位地址
                Type dType = DataTypeHelper.GetTypeFromDataType(dataType);
                if (dType == typeof(bool))
                {
                    count = length % 16 == 0 ? (ushort)(length / 16) : (ushort)(length / 16 + 1);
                }
                else
                {
                    int bytesOfType = Marshal.SizeOf(dType);
                    count = (ushort)(Math.Ceiling((double)(length * bytesOfType / 2)));
                }
            }
            else if (requestType == RequestType.BIT) //按位读，就是读取每个字的第0位，并将每个结果存储到一个字节数组中，每个字节存储一个位状态
            {
                // 按位从Y区读取N个位，1个位1个位地址
                // 按位从D区读取N个位，16个位1个字地址
                count = length;
            }
            try
            {
                byte[] result = _mc3eClient.Read(address, count, requestType, _mc3eOptions.IsOctal);
                mc3eData.Bytes = result;

                try
                {
                    //反射调用泛型方法：public List<T> GetDatas<T>(byte[] bytes)
                    var method = typeof(Mc3E).GetMethod("GetDatas");
                    var genericType1 = DataTypeHelper.GetTypeFromDataType(dataType);
                    var genericMethod = method.MakeGenericMethod(genericType1);
                    mc3eData.Value = genericMethod.Invoke(_mc3eClient, new object[] { result });
                }
                catch (Exception ex1)
                {
                    throw new Exception($"将返回值转换成指定类型{Enum.GetName(typeof(DataType), dataType)}时发生错误！");
                }
                mc3eData.Success = true;
                return mc3eData;
            }
            catch (Exception ex)
            {
                mc3eData.Success = false;
                mc3eData.ErrorMessage = new Exception($"读取点位地址{address}的数据时发生错误！", ex).Message;
                return mc3eData;
            }
        }

        public ReadResponseParameter Read(ReadRequestParameter requestParam, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.BigEndian)
        {
            if (requestParam == null)
                throw new ArgumentNullException("读取点位信息的请求参数不能为空！");
            var result = Read(requestParam.Address, requestParam.DataType, requestParam.Length, slaveId, endianTypes);
            var response = new ReadResponseParameter(result, requestParam);
            return response;
        }

        public bool Write(string address, DataType dataType, object value, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.BigEndian)
        {
            RequestType requestType = dataType == DataType.Bit ? RequestType.BIT : RequestType.WORD;

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
                    var method2 = typeof(Mc3E).GetMethod("GetBytes");
                    var genericMethod2 = method2.MakeGenericMethod(genericType1);
                    bytesToWrite = (byte[])genericMethod2.Invoke(_mc3eClient, new object[] { dataArray });
                }
                catch (Exception ex1)
                {
                    throw new Exception($"写入数据不符合指定类型{Enum.GetName(typeof(DataType), dataType)}！");
                }

                _mc3eClient.Write(bytesToWrite, address, requestType, _mc3eOptions.IsOctal);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception($"写入点位地址{address}时发生错误！", ex);
            }
        }

        public WriteResponseParameter Write(WriteRequestParameter requestParam, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.BigEndian)
        {
            if (requestParam == null)
                throw new ArgumentNullException("写入点位信息的请求参数不能为空！");
            var response = new WriteResponseParameter(requestParam)
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

        public BatchReadResponseParameter BatchRead(IEnumerable<ReadRequestParameter> requestParam, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.BigEndian)
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
                Dictionary<string, Exception> varAddrWithErrorList = new Dictionary<string, Exception>();
                foreach (var requestItem in requestParam)
                {
                    var resp = Read(requestItem, slaveId, endianTypes);
                    response.Results.Add(resp);
                    if (!resp.Success)
                    {
                        varAddrWithErrorList.Add(requestItem.Address, new Exception(resp.ErrorMessage));
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
                Dictionary<string, Exception> varAddrWithErrorList = new Dictionary<string, Exception>();
                foreach (var requestItem in requestParam)
                {
                    var resp = Write(requestItem, slaveId, endianTypes);
                    response.Results.Add(resp);
                    if (!resp.Success)
                    {
                        varAddrWithErrorList.Add(requestItem.Address, new Exception(resp.ErrorMessage));
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

        // 随机读写。
        // 可以按字、双字读取数据，但不能按位读取。
        public BatchReadResponseParameter RandomRead(IEnumerable<ReadRequestParameter> requestParamsByWord, IEnumerable<ReadRequestParameter> requestParamsByDWord, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.BigEndian)
        {
            bool b1 = requestParamsByWord == null || !requestParamsByWord.Any();
            bool b2 = requestParamsByDWord == null || !requestParamsByDWord.Any();
            if (b1 && b2) //至少保证传入了一个有效的请求参数
                throw new ArgumentNullException("读取点位信息的请求参数不能为空！");

            var response = new BatchReadResponseParameter();

            Dictionary<string, Exception> varAddrWithErrorList = new Dictionary<string, Exception>();
            List<(Mitsubishi.Base.DataParameter, ReadRequestParameter)> datasByWord = new List<(Mitsubishi.Base.DataParameter, ReadRequestParameter)>();
            List<(Mitsubishi.Base.DataParameter, ReadRequestParameter)> datasByDWord = new List<(Mitsubishi.Base.DataParameter, ReadRequestParameter)>();

            if (!b1)
            {
                foreach(var requestItem in requestParamsByWord)
                {
                    try
                    {
                        (Areas, string) addr = _mc3eClient.GetAddress(requestItem.Address);
                        datasByWord.Add((new Mitsubishi.Base.DataParameter()
                        {
                            Area = addr.Item1,
                            Address = addr.Item2,
                            Count = requestItem.Length
                        }, requestItem));
                    }
                    catch(Exception ex1)
                    {
                        varAddrWithErrorList.Add(requestItem.Address, ex1);
                    }
                }
            }
            if (!b2)
            {
                foreach (var requestItem in requestParamsByDWord)
                {
                    try
                    {
                        (Areas, string) addr = _mc3eClient.GetAddress(requestItem.Address);
                        datasByDWord.Add((new Mitsubishi.Base.DataParameter()
                        {
                            Area = addr.Item1,
                            Address = addr.Item2,
                            Count = requestItem.Length
                        }, requestItem));
                    }
                    catch (Exception ex2)
                    {
                        varAddrWithErrorList.Add(requestItem.Address, ex2);
                    }
                }
            }

            if (varAddrWithErrorList.Any())
            {
                response.Success = false;
                response.ErrorMessage = new Exception($"读取的某个点位地址格式不正确：{string.Join(",", varAddrWithErrorList)}").Message;
                return response;
            }

            try
            {
                _mc3eClient.RandomRead(datasByWord.Select(d => d.Item1).ToList(), datasByDWord.Select(d => d.Item1).ToList(), _mc3eOptions.IsOctal);
                
                if (datasByWord.Any())
                {
                    foreach (var data in datasByWord)
                    {
                        var requestPara = data.Item2;
                        var dataPara = data.Item1;
                        var mc3eData = new MC3E_Data()
                        {
                            DataType = requestPara.DataType,
                            DataLength = dataPara.Count,
                            ProtocolType = ProtocolType
                        };
                        mc3eData.Bytes = dataPara.Datas.ToArray();

                        var responseParameter = new ReadResponseParameter(mc3eData, requestPara);
                        try
                        {
                            mc3eData.Value = convertBytesToData(mc3eData.Bytes, mc3eData.DataType);
                            mc3eData.Success = true;
                        }
                        catch (Exception ex1)
                        {
                            mc3eData.Success = false;
                            mc3eData.ErrorMessage = ex1.Message;

                            varAddrWithErrorList.Add(requestPara.Address, ex1);
                        }
                        response.Results.Add(responseParameter);
                    }
                }
                if (datasByDWord.Any())
                {
                    foreach (var data in datasByDWord)
                    {
                        var requestPara = data.Item2;
                        var dataPara = data.Item1;
                        var mc3eData = new MC3E_Data()
                        {
                            DataType = requestPara.DataType,
                            DataLength = dataPara.Count,
                            ProtocolType = ProtocolType
                        };
                        mc3eData.Bytes = dataPara.Datas.ToArray();

                        var responseParameter = new ReadResponseParameter(mc3eData, requestPara);
                        try
                        {
                            mc3eData.Value = convertBytesToData(mc3eData.Bytes, mc3eData.DataType);
                            mc3eData.Success = true;
                        }
                        catch (Exception ex2)
                        {
                            mc3eData.Success = false;
                            mc3eData.ErrorMessage = ex2.Message;

                            varAddrWithErrorList.Add(requestPara.Address, ex2);
                        }
                        response.Results.Add(responseParameter);
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
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                var errorAddrList = new List<string>();
                errorAddrList.AddRange(datasByWord.Select(d => d.Item2.Address).ToList());
                errorAddrList.AddRange(datasByDWord.Select(d => d.Item2.Address).ToList());
                response.ErrorMessage = new Exception($"读取点位地址{string.Join(",", errorAddrList.Distinct())}的数据时发生错误！").Message;
                return response;
            }
        }

        // 多块批量读写   字、位
        // 只能按字读取数据，而不管读取的软元件区域是字地址还是位地址。
        public BatchReadResponseParameter MultiBlockRead(IEnumerable<ReadRequestParameter> requestParamsByWord, IEnumerable<ReadRequestParameter> requestsParamByBit, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.BigEndian)
        {
            bool b1 = requestParamsByWord == null || !requestParamsByWord.Any();
            bool b2 = requestsParamByBit == null || !requestsParamByBit.Any();
            if (b1 && b2) //至少保证传入了一个有效的请求参数
                throw new ArgumentNullException("读取点位信息的请求参数不能为空！");

            var response = new BatchReadResponseParameter();

            Dictionary<string, Exception> varAddrWithErrorList = new Dictionary<string, Exception>();
            List<(Mitsubishi.Base.DataParameter, ReadRequestParameter)> datasByWord = new List<(Mitsubishi.Base.DataParameter, ReadRequestParameter)>();
            List<(Mitsubishi.Base.DataParameter, ReadRequestParameter)> datasByBit = new List<(Mitsubishi.Base.DataParameter, ReadRequestParameter)>();

            if (!b1)
            {
                foreach (var requestItem in requestParamsByWord)
                {
                    try
                    {
                        (Areas, string) addr = _mc3eClient.GetAddress(requestItem.Address);
                        datasByWord.Add((new Mitsubishi.Base.DataParameter()
                        {
                            Area = addr.Item1,
                            Address = addr.Item2,
                            Count = requestItem.Length
                        }, requestItem));
                    }
                    catch(Exception ex1)
                    {
                        varAddrWithErrorList.Add(requestItem.Address, ex1);
                    }
                }
            }
            if (!b2)
            {
                foreach (var requestItem in requestsParamByBit)
                {
                    try
                    {
                        (Areas, string) addr = _mc3eClient.GetAddress(requestItem.Address);
                        datasByBit.Add((new Mitsubishi.Base.DataParameter()
                        {
                            Area = addr.Item1,
                            Address = addr.Item2,
                            Count = requestItem.Length
                        }, requestItem));
                    }
                    catch(Exception ex2)
                    {
                        varAddrWithErrorList.Add(requestItem.Address, ex2);
                    }
                }
            }

            if (varAddrWithErrorList.Any())
            {
                response.Success = false;
                response.ErrorMessage = new Exception($"读取的某个点位地址格式不正确：{string.Join(",", varAddrWithErrorList)}").Message;
                return response;
            }

            try
            {
                _mc3eClient.MultiBlockRead(datasByWord.Select(d => d.Item1).ToList(), datasByBit.Select(d => d.Item1).ToList());

                if (datasByWord.Any())
                {
                    foreach (var data in datasByWord)
                    {
                        var requestPara = data.Item2;
                        var dataPara = data.Item1;
                        var mc3eData = new MC3E_Data()
                        {
                            DataType = requestPara.DataType,
                            DataLength = dataPara.Count,
                            ProtocolType = ProtocolType
                        };
                        mc3eData.Bytes = dataPara.Datas.ToArray();

                        var responseParameter = new ReadResponseParameter(mc3eData, requestPara);
                        try
                        {
                            mc3eData.Value = convertBytesToData(mc3eData.Bytes, mc3eData.DataType);
                            mc3eData.Success = true;
                        }
                        catch (Exception ex1)
                        {
                            mc3eData.Success = false;
                            mc3eData.ErrorMessage = ex1.Message;

                            varAddrWithErrorList.Add(requestPara.Address, ex1);
                        }
                        response.Results.Add(responseParameter);
                    }
                }
                if (datasByBit.Any())
                {
                    foreach (var data in datasByBit)
                    {
                        var requestPara = data.Item2;
                        var dataPara = data.Item1;
                        var mc3eData = new MC3E_Data()
                        {
                            DataType = requestPara.DataType,
                            DataLength = dataPara.Count,
                            ProtocolType = ProtocolType
                        };
                        mc3eData.Bytes = dataPara.Datas.ToArray();

                        var responseParameter = new ReadResponseParameter(mc3eData, requestPara);
                        try
                        {
                            mc3eData.Value = convertBytesToData(mc3eData.Bytes, mc3eData.DataType);
                            mc3eData.Success = true;
                        }
                        catch (Exception ex2)
                        {
                            mc3eData.Success = false;
                            mc3eData.ErrorMessage = ex2.Message;

                            varAddrWithErrorList.Add(requestPara.Address, ex2);
                        }
                        response.Results.Add(responseParameter);
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
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                var errorAddrList = new List<string>();
                errorAddrList.AddRange(datasByWord.Select(d => d.Item2.Address).ToList());
                errorAddrList.AddRange(datasByBit.Select(d => d.Item2.Address).ToList());
                response.ErrorMessage = new Exception($"读取点位地址{string.Join(",", errorAddrList.Distinct())}的数据时发生错误！").Message;
                return response;
            }
        }

        // 按字、双字进行写入
        public BatchWriteResponseParameter RandomWrite(IEnumerable<WriteRequestParameter> requestParamsByWord, IEnumerable<WriteRequestParameter> requestParamsByDWord, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.BigEndian)
        {
            bool b1 = requestParamsByWord == null || !requestParamsByWord.Any();
            bool b2 = requestParamsByDWord == null || !requestParamsByDWord.Any();
            if (b1 && b2) //至少保证传入了一个有效的请求参数
                throw new ArgumentNullException("写入点位信息的请求参数不能为空！");

            var response = new BatchWriteResponseParameter();

            Dictionary<string, Exception> varAddrWithErrorList = new Dictionary<string, Exception>();
            List<(Mitsubishi.Base.DataParameter, WriteRequestParameter)> datasByWord = new List<(Mitsubishi.Base.DataParameter, WriteRequestParameter)>();
            List<(Mitsubishi.Base.DataParameter, WriteRequestParameter)> datasByDWord = new List<(Mitsubishi.Base.DataParameter, WriteRequestParameter)>();

            if (!b1)
            {
                foreach (var requestItem in requestParamsByWord)
                {
                    try
                    {
                        (Areas, string) addr = _mc3eClient.GetAddress(requestItem.Address);
                        byte[] bytesToWrite = convertDataToBytes(requestItem.Value, requestItem.DataType, out ushort count);
                        var dataPara = new Mitsubishi.Base.DataParameter()
                        {
                            Area = addr.Item1,
                            Address = addr.Item2,
                            Count = count,
                            Datas = bytesToWrite.ToList()
                        };
                        datasByWord.Add((dataPara, requestItem));
                    }
                    catch(Exception ex1)
                    {
                        varAddrWithErrorList.Add(requestItem.Address, ex1);
                    }
                }
            }
            if (!b2)
            {
                foreach (var requestItem in requestParamsByDWord)
                {
                    try
                    {
                        (Areas, string) addr = _mc3eClient.GetAddress(requestItem.Address);
                        byte[] bytesToWrite = convertDataToBytes(requestItem.Value, requestItem.DataType, out ushort count);
                        var dataPara = new Mitsubishi.Base.DataParameter()
                        {
                            Area = addr.Item1,
                            Address = addr.Item2,
                            Count = count,
                            Datas = bytesToWrite.ToList()
                        };
                        datasByDWord.Add((dataPara, requestItem));
                    }
                    catch (Exception ex2)
                    {
                        varAddrWithErrorList.Add(requestItem.Address, ex2);
                    }
                }
            }

            if (varAddrWithErrorList.Any())
            {
                response.Success = false;
                response.ErrorMessage = new Exception($"写入的某个点位地址或写入内容的格式不正确：{string.Join(",", varAddrWithErrorList)}").Message;
                return response;
            }

            try
            {
                var w_addr = new List<Mitsubishi.Base.DataParameter>()
                {
                    new mitsubishi.DataParameter()
                    {
                        Address = "100",
                        Area = Areas.D,
                        Count = 1,
                        Datas = new List<byte>() { 0x01, 0x00 }
                    }
                };
                var b_addr = new List<Mitsubishi.Base.DataParameter>()
                {
                    new mitsubishi.DataParameter()
                    {
                        Address = "100",
                        Area = Areas.X,
                        Count = 1,
                        Datas = new List<byte>() { 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01, 0x00, 0x01}
                    }
                };
                _mc3eClient.MultiBlockWrite(w_addr, b_addr);
                //_mc3eClient.RandomWrite(datasByWord.Select(d => d.Item1).ToList(), datasByDWord.Select(d => d.Item1).ToList(), _mc3eOptions.IsOctal);

                if (datasByWord.Any())
                {
                    foreach (var data in datasByWord)
                    {
                        var requestPara = data.Item2;
                        var dataPara = data.Item1;

                        var responseParameter = new WriteResponseParameter(requestPara)
                        {
                            DataType = requestPara.DataType,
                            Value = requestPara.Value,
                            ProtocolType = ProtocolType
                        };
                        responseParameter.Success = true;
                        response.Results.Add(responseParameter);
                    }
                }
                if (datasByDWord.Any())
                {
                    foreach (var data in datasByDWord)
                    {
                        var requestPara = data.Item2;
                        var dataPara = data.Item1;

                        var responseParameter = new WriteResponseParameter(requestPara)
                        {
                            DataType = requestPara.DataType,
                            Value = requestPara.Value,
                            ProtocolType = ProtocolType
                        };
                        responseParameter.Success = true;
                        response.Results.Add(responseParameter);
                    }
                }

                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                var errorAddrList = new List<string>();
                errorAddrList.AddRange(datasByWord.Select(d => d.Item2.Address).ToList());
                errorAddrList.AddRange(datasByDWord.Select(d => d.Item2.Address).ToList());
                response.ErrorMessage = new Exception($"写入点位地址{string.Join(",", errorAddrList.Distinct())}的数据时发生错误！").Message;
                return response;
            }
        }

        // 按位进行写入
        public BatchWriteResponseParameter RandomWriteBit(IEnumerable<WriteRequestParameter> requestParamsByBit, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.BigEndian)
        {
            if (requestParamsByBit == null || !requestParamsByBit.Any()) //至少保证传入了一个有效的请求参数
                throw new ArgumentNullException("写入点位信息的请求参数不能为空！");

            var response = new BatchWriteResponseParameter();

            Dictionary<string, Exception> varAddrWithErrorList = new Dictionary<string, Exception>();
            List<(Mitsubishi.Base.DataParameter, WriteRequestParameter)> datasByBit = new List<(Mitsubishi.Base.DataParameter, WriteRequestParameter)>();

            foreach (var requestItem in requestParamsByBit)
            {
                try
                {
                    (Areas, string) addr = _mc3eClient.GetAddress(requestItem.Address);
                    byte[] bytesToWrite = convertDataToBytes(requestItem.Value, requestItem.DataType, out ushort count);
                    var dataPara = new Mitsubishi.Base.DataParameter()
                    {
                        Area = addr.Item1,
                        Address = addr.Item2,
                        Count = count,
                        Datas = bytesToWrite.ToList()
                    };
                    datasByBit.Add((dataPara, requestItem));
                }
                catch (Exception ex1)
                {
                    varAddrWithErrorList.Add(requestItem.Address, ex1);
                }
            }

            if (varAddrWithErrorList.Any())
            {
                response.Success = false;
                response.ErrorMessage = new Exception($"写入的某个点位地址或写入内容的格式不正确：{string.Join(",", varAddrWithErrorList)}").Message;
                return response;
            }

            try
            {
                _mc3eClient.RandomWriteBit(datasByBit.Select(d => d.Item1).ToList(), _mc3eOptions.IsOctal);

                if (datasByBit.Any())
                {
                    foreach (var data in datasByBit)
                    {
                        var requestPara = data.Item2;
                        var dataPara = data.Item1;

                        var responseParameter = new WriteResponseParameter(requestPara)
                        {
                            DataType = requestPara.DataType,
                            Value = requestPara.Value,
                            ProtocolType = ProtocolType
                        };
                        responseParameter.Success = true;
                        response.Results.Add(responseParameter);
                    }
                }

                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                var errorAddrList = new List<string>();
                errorAddrList.AddRange(datasByBit.Select(d => d.Item2.Address).ToList());
                response.ErrorMessage = new Exception($"写入点位地址{string.Join(",", errorAddrList.Distinct())}的数据时发生错误！").Message;
                return response;
            }
        }

        // 只能按字写入数据，而不管写入的软元件区域是字地址还是位地址。
        public BatchWriteResponseParameter MultiBlockWrite(IEnumerable<WriteRequestParameter> requestParamsByWord, IEnumerable<WriteRequestParameter> requestParamsByBit, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.BigEndian)
        {
            bool b1 = requestParamsByWord == null || !requestParamsByWord.Any();
            bool b2 = requestParamsByBit == null || !requestParamsByBit.Any();
            if (b1 && b2) //至少保证传入了一个有效的请求参数
                throw new ArgumentNullException("写入点位信息的请求参数不能为空！");

            var response = new BatchWriteResponseParameter();

            Dictionary<string, Exception> varAddrWithErrorList = new Dictionary<string, Exception>();
            List<(Mitsubishi.Base.DataParameter, WriteRequestParameter)> datasByWord = new List<(Mitsubishi.Base.DataParameter, WriteRequestParameter)>();
            List<(Mitsubishi.Base.DataParameter, WriteRequestParameter)> datasByBit = new List<(Mitsubishi.Base.DataParameter, WriteRequestParameter)>();

            if (!b1)
            {
                foreach (var requestItem in requestParamsByWord)
                {
                    try
                    {
                        (Areas, string) addr = _mc3eClient.GetAddress(requestItem.Address);
                        byte[] bytesToWrite = convertDataToBytes(requestItem.Value, requestItem.DataType, out ushort count);
                        var dataPara = new Mitsubishi.Base.DataParameter()
                        {
                            Area = addr.Item1,
                            Address = addr.Item2,
                            Count = count,
                            Datas = bytesToWrite.ToList()
                        };
                        datasByWord.Add((dataPara, requestItem));
                    }
                    catch (Exception ex1)
                    {
                        varAddrWithErrorList.Add(requestItem.Address, ex1);
                    }
                }
            }
            if (!b2)
            {
                foreach (var requestItem in requestParamsByBit)
                {
                    try
                    {
                        (Areas, string) addr = _mc3eClient.GetAddress(requestItem.Address);
                        byte[] bytesToWrite = convertDataToBytes(requestItem.Value, requestItem.DataType, out ushort count);
                        var dataPara = new Mitsubishi.Base.DataParameter()
                        {
                            Area = addr.Item1,
                            Address = addr.Item2,
                            Count = count,
                            Datas = bytesToWrite.ToList()
                        };
                        datasByBit.Add((dataPara, requestItem));
                    }
                    catch (Exception ex2)
                    {
                        varAddrWithErrorList.Add(requestItem.Address, ex2);
                    }
                }
            }

            if (varAddrWithErrorList.Any())
            {
                response.Success = false;
                response.ErrorMessage = new Exception($"写入的某个点位地址或写入内容的格式不正确：{string.Join(",", varAddrWithErrorList)}").Message;
                return response;
            }

            try
            {
                _mc3eClient.MultiBlockWrite(datasByWord.Select(d => d.Item1).ToList(), datasByBit.Select(d => d.Item1).ToList());

                if (datasByWord.Any())
                {
                    foreach (var data in datasByWord)
                    {
                        var requestPara = data.Item2;
                        var dataPara = data.Item1;

                        var responseParameter = new WriteResponseParameter(requestPara)
                        {
                            DataType = requestPara.DataType,
                            Value = requestPara.Value,
                            ProtocolType = ProtocolType
                        };
                        responseParameter.Success = true;
                        response.Results.Add(responseParameter);
                    }
                }
                if (datasByBit.Any())
                {
                    foreach (var data in datasByBit)
                    {
                        var requestPara = data.Item2;
                        var dataPara = data.Item1;

                        var responseParameter = new WriteResponseParameter(requestPara)
                        {
                            DataType = requestPara.DataType,
                            Value = requestPara.Value,
                            ProtocolType = ProtocolType
                        };
                        responseParameter.Success = true;
                        response.Results.Add(responseParameter);
                    }
                }

                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                var errorAddrList = new List<string>();
                errorAddrList.AddRange(datasByWord.Select(d => d.Item2.Address).ToList());
                errorAddrList.AddRange(datasByBit.Select(d => d.Item2.Address).ToList());
                response.ErrorMessage = new Exception($"写入点位地址{string.Join(",", errorAddrList.Distinct())}的数据时发生错误！").Message;
                return response;
            }
        }

        public void PlcRun(ExecuteType et = ExecuteType.Normal, CleanMode cm = CleanMode.Normal)
        {
            var executeType = (mitsubishi.ExecuteType)Enum.Parse(typeof(mitsubishi.ExecuteType), et.ToString());
            var cleanMode = (mitsubishi.CleanMode)Enum.Parse(typeof(mitsubishi.CleanMode), cm.ToString());
            _mc3eClient.PlcRun(executeType, cleanMode);
        }

        public void PlcStop(ExecuteType et = ExecuteType.Normal)
        {
            var executeType = (mitsubishi.ExecuteType)Enum.Parse(typeof(mitsubishi.ExecuteType), et.ToString());
            _mc3eClient.PlcStop(executeType);
        }

        private object convertBytesToData(byte[] result, DataType dataType)
        {
            try
            {
                //反射调用泛型方法：public List<T> GetDatas<T>(byte[] bytes)
                var method = typeof(Mc3E).GetMethod("GetDatas");
                var genericType1 = DataTypeHelper.GetTypeFromDataType(dataType);
                var genericMethod = method.MakeGenericMethod(genericType1);
                return genericMethod.Invoke(_mc3eClient, new object[] { result });
            }
            catch (Exception ex1)
            {
                throw new Exception($"将返回值转换成指定类型{Enum.GetName(typeof(DataType), dataType)}时发生错误！");
            }
        }

        private byte[] convertDataToBytes(object value, DataType dataType, out ushort count)
        {
                byte[] bytesToWrite = new byte[0];
                var genericType1 = DataTypeHelper.GetTypeFromDataType(dataType);
            try
            {
                //反射调用泛型方法：public static T[] ConvertToDataArray<T>(object value)
                var method1 = typeof(DataTypeHelper).GetMethod("ConvertToDataArray");
                var genericMethod1 = method1.MakeGenericMethod(genericType1);
                dynamic dataArray = genericMethod1.Invoke(null, new object[] { value });
                count = (ushort)dataArray.Length;

                //反射调用泛型方法：public byte[] GetBytes<T>(params T[] values)
                var method2 = typeof(Mc3E).GetMethod("GetBytes");
                var genericMethod2 = method2.MakeGenericMethod(genericType1);
                bytesToWrite = (byte[])genericMethod2.Invoke(_mc3eClient, new object[] { dataArray });
                return bytesToWrite;
            }
            catch (Exception ex1)
            {
                count = 0;
                throw new Exception($"写入数据不符合指定类型{Enum.GetName(typeof(DataType), dataType)}！");
            }
        }

        public async Task<bool> ConnectAsync()
        {
            try
            {
                _logger.LogInformation($"Connecting to Mitsubishi MC3E device at Socket: {_mc3eOptions.IP.ToString()} : {_mc3eOptions.Port}");
                return await Task.Run(() =>
                {
                    int timeout = _mc3eOptions.ReceiveTimeout;
                    if (timeout > 0)
                        _mc3eClient.Open(timeout);
                    else
                        _mc3eClient.Open();
                    return true;
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to disconnect to Mitsubishi MC3E device at Socket: {_mc3eOptions.IP.ToString()} : {_mc3eOptions.Port}");
                return false;
            }
        }

        public async Task DisconnectAsync()
        {
            try
            {
                _logger.LogInformation($"Disconnecting to Mitsubishi MC3E device at Socket: {_mc3eOptions.IP.ToString()} : {_mc3eOptions.Port}");
                await Task.Run(() =>
                {
                    _mc3eClient.Close();
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Failed to disconnect to Mitsubishi MC3E device at Socket: {_mc3eOptions.IP.ToString()} : {_mc3eOptions.Port}");
            }
        }

        //由于三菱MC3E协议级别的异步访问方式在本项目中还没有实现，目前只是简单封装了异步任务的方式，以保持系统所有的协议都能问异步进行访问。
        public async Task<ProtocolData> ReadAsync(string address, DataType dataType, ushort length = 1, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.BigEndian)
        {
            var mc3eData = new MC3E_Data()
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
                        mc3eData.Success = result.Success;
                        mc3eData.ErrorMessage = result.ErrorMessage;
                        mc3eData.Bytes = result.Bytes;
                        mc3eData.Value = result.Value;
                    }
                    else
                    {
                        mc3eData.Success = false;
                    }
                });
            }
            catch (Exception ex)
            {
                mc3eData.Success = false;
                mc3eData.ErrorMessage = ex.ToString();
            }
            return mc3eData;
        }

        public async Task<ReadResponseParameter> ReadAsync(ReadRequestParameter requestParameter, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.BigEndian)
        {
            var mc3eData = new MC3E_Data()
            {
                DataType = requestParameter.DataType,
                DataLength = requestParameter.Length,
                ProtocolType = ProtocolType
            };
            var response = new ReadResponseParameter(mc3eData, requestParameter);
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
                        mc3eData.Success = false;
                    }
                });
            }
            catch (Exception ex)
            {
                mc3eData.Success = false;
                mc3eData.ErrorMessage = ex.Message;
            }
            return response;
        }

        public async Task<bool> WriteAsync(string address, DataType dataType, object value, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.BigEndian)
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

        public async Task<WriteResponseParameter> WriteAsync(WriteRequestParameter requestParam, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.BigEndian)
        {
            var response = new WriteResponseParameter(requestParam)
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

        public async Task<BatchReadResponseParameter> BatchReadAsync(IEnumerable<ReadRequestParameter> requestParameters, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.BigEndian)
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
            catch (Exception ex)
            {
                throw ex;
            }
            return response;
        }

        public async Task<BatchWriteResponseParameter> BatchWriteAsync(IEnumerable<WriteRequestParameter> requestParameters, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.BigEndian)
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

        public async Task<BatchReadResponseParameter> RandomReadAsync(IEnumerable<ReadRequestParameter> requestParamsByWord, IEnumerable<ReadRequestParameter> requestParamsByDWord, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.BigEndian)
        {
            var response = new BatchReadResponseParameter();
            try
            {
                await Task.Run(() =>
                {
                    var result = RandomRead(requestParamsByWord, requestParamsByDWord, slaveId, endianTypes);
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

        public async Task<BatchReadResponseParameter> MultiBlockReadAsync(IEnumerable<ReadRequestParameter> requestParamsByWord, IEnumerable<ReadRequestParameter> requestsParamByBit, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.BigEndian)
        {
            var response = new BatchReadResponseParameter();
            try
            {
                await Task.Run(() =>
                {
                    var result = MultiBlockRead(requestParamsByWord, requestsParamByBit, slaveId, endianTypes);
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

        public async Task<BatchWriteResponseParameter> RandomWriteAsync(IEnumerable<WriteRequestParameter> requestParamsByWord, IEnumerable<WriteRequestParameter> requestParamsByDWord, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.BigEndian)
        {
            var response = new BatchWriteResponseParameter();
            try
            {
                await Task.Run(() =>
                {
                    var result = RandomWrite(requestParamsByWord, requestParamsByDWord, slaveId, endianTypes);
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

        public async Task<BatchWriteResponseParameter> RandomWriteBitAsync(IEnumerable<WriteRequestParameter> requestParamsByBit, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.BigEndian)
        {
            var response = new BatchWriteResponseParameter();
            try
            {
                await Task.Run(() =>
                {
                    var result = RandomWriteBit(requestParamsByBit, slaveId, endianTypes);
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

        public async Task<BatchWriteResponseParameter> MultiBlockWriteAsync(IEnumerable<WriteRequestParameter> requestParamsByWord, IEnumerable<WriteRequestParameter> requestParamsByBit, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.BigEndian)
        {
            var response = new BatchWriteResponseParameter();
            try
            {
                await Task.Run(() =>
                {
                    var result = MultiBlockWrite(requestParamsByWord, requestParamsByBit, slaveId, endianTypes);
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

        public async Task PlcRunAsync(ExecuteType et = ExecuteType.Normal, CleanMode cm = CleanMode.Normal)
        {
            try
            {
                await Task.Run(() =>
                {
                    PlcRun(et, cm);
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task PlcStopAsync(ExecuteType et = ExecuteType.Normal)
        {
            try
            {
                await Task.Run(() =>
                {
                    PlcStop(et);
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion

        private static byte[] BitsToBytes(byte[] bits)
        {
            if (bits == null || !bits.Any())
                throw new ArgumentNullException(nameof(bits));

            byte[] paddedBits = bits;
            //必须是8的倍数，否则在前面补0
            if (bits.Length % 8 != 0)
            {
                int paddingLength = 8 - (bits.Length % 8);
                paddedBits = new byte[bits.Length + paddingLength];
                Array.Copy(bits, 0, paddedBits, paddingLength, bits.Length);
                bits = paddedBits;
            }

            List<byte> results = new List<byte>();

            for (int i = 0; i < paddedBits.Length; i++)
            {
                byte bit = (byte)(paddedBits[i] & 0x01);
                if (i % 8 == 0)
                {
                    byte a = (byte)(bit << 7);
                    byte b = (byte)(0x00 | a);
                    results.Add(b);
                }
                else
                {
                    results[results.Count - 1] |= (byte)(bit << (7 - i % 8));
                }
            }

            return results.ToArray();
        }
    }
}
