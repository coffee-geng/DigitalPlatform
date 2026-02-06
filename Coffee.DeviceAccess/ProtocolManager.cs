using Coffee.DeviceAdapter;
using Coffee.DigitalPlatform.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DeviceAccess
{
    public interface IProtocolManager
    {
        Task<IProtocolAdapter> ConnectAsync(string deviceNum, Dictionary<string, ProtocolParameter> communicationParameters);

        Task DisconnectAsync(string deviceNum);

        Task<ProtocolData> ReadAsync(string deviceNum, Variable variable);

        Task<bool> WriteAsync(string deviceNum, Variable variable);

        Task<ProtocolData<T>> ReadAsync<T>(string deviceNum, Variable<T> variable);

        Task<bool> WriteAsync<T>(string deviceNum, Variable<T> variable);
    }

    public class ProtocolManager : IProtocolManager
    {
        private readonly ILogger<ProtocolManager> _logger;
        private readonly IConnectionPool _connectionPool;

        //字典保存设备编号和该设备使用的通讯协议配置信息
        private readonly ConcurrentDictionary<string, IProtocolOptions> _deviceProtocolDict;

        public ProtocolManager(ILoggerFactory loggerFactory)
        {
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            _logger = loggerFactory.CreateLogger<ProtocolManager>();

            var protocolFactory = new ProtocolFactory(loggerFactory);
            _connectionPool = new ConnectionPool(protocolFactory, loggerFactory);

            _deviceProtocolDict = new ConcurrentDictionary<string, IProtocolOptions>();
        }

        public async Task<IProtocolAdapter> ConnectAsync(string deviceNum, Dictionary<string, ProtocolParameter> communicationParameters)
        {
            if (string.IsNullOrWhiteSpace(deviceNum))
                throw new ArgumentNullException(nameof(deviceNum));
            if (communicationParameters == null)
                throw new ArgumentNullException(nameof(communicationParameters));

            //通信参数为空，则表示使用当前deviceNum上次使用的通信参数，该参数保存在ProtocolManager的字典中
            //如果找不到，则抛出异常
            if (communicationParameters == null)
            {
                if (!_deviceProtocolDict.TryGetValue(deviceNum, out _))
                {
                    throw new Exception($"连接设备{deviceNum}时出错，因为没有找到该设备的通信协议配置参数！");
                }
            }
            else
            {
                if (!communicationParameters.TryGetValue("Protocol", out ProtocolParameter protocolParam) || protocolParam.PropValueType != typeof(string))
                {
                    throw new Exception($"连接设备时必须明指明通信协议");
                }
                string protocolName = protocolParam.PropValue;
                try
                {
                    //根据协议名，创建通信协议配置对象
                    Assembly adapterAssembly = Assembly.LoadFrom("Coffee.DeviceAdapter.dll");
                    //协议配置对象总是协议名+Options后缀
                    Type adapterOptionType = adapterAssembly.GetType($"Coffee.DeviceAdapter.{protocolName + "_Options"}");
                    IProtocolOptions adapterOption = (IProtocolOptions)Activator.CreateInstance(adapterOptionType);

                    foreach (var parameter in communicationParameters.Values)
                    {
                        adapterOption.Config(parameter.PropName, parameter.PropValue, parameter.PropValueType);
                    }

                    _deviceProtocolDict[deviceNum] = adapterOption;
                }
                catch (Exception ex)
                {
                    throw new Exception($"不能创建协议名为{protocolName}的通信协议配置对象！");
                }
            }

            return await _connectionPool.OpenConnectionAsync(deviceNum, _deviceProtocolDict[deviceNum]);
        }

        public Task DisconnectAsync(string deviceNum)
        {
            throw new NotImplementedException();
        }

        public async Task<ProtocolData> ReadAsync(string deviceNum, Variable variable)
        {
            if (variable == null)
                throw new ArgumentNullException(nameof(variable));

            IProtocolAdapter adapter = await openConnectionAsync(deviceNum);

            var dataType = DataTypeHelper.GetDataTypeFromType(variable.VarType);
            if (dataType.HasValue)
            {
                ProtocolData result = await adapter.ReadAsync(variable.VarAddress, dataType.Value);
                return result;
            }
            else
            {
                throw new Exception($"读取点位信息{variable.VarAddress}出错！");
            }
        }

        public async Task<bool> WriteAsync(string deviceNum, Variable variable)
        {
            if (variable == null)
                throw new ArgumentNullException(nameof(variable));

            IProtocolAdapter adapter = await openConnectionAsync(deviceNum);

            var dataType = DataTypeHelper.GetDataTypeFromType(variable.VarType);
            if (dataType.HasValue)
            {
                bool result = await adapter.WriteAsync(variable.VarAddress, dataType.Value, variable.VarValue);
                return result;
            }
            else
            {
                throw new Exception($"写入点位信息{variable.VarAddress}出错！");
            }
        }

        public async Task<ProtocolData<T>> ReadAsync<T>(string deviceNum, Variable<T> variable)
        {
            if (variable == null)
                throw new ArgumentNullException(nameof(variable));

            IProtocolAdapter adapter = await openConnectionAsync(deviceNum);

            var dataType = DataTypeHelper.GetDataTypeFromType(typeof(T));
            if (dataType.HasValue)
            {
                try
                {
                    ProtocolData result = await adapter.ReadAsync(variable.VarAddress, dataType.Value);
                    //构建继承自ProtocolData的泛型版本类
                    Type resultTypeByGeneric = TypeUtils.CreateGenericTypeBaseOnNonGeneric(result.GetType(), new Type[] { typeof(T) });
                    dynamic resultByGeneric = Activator.CreateInstance(resultTypeByGeneric);
                    foreach (var prop in result.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
                    {
                        var targetProp = resultTypeByGeneric.GetProperty(prop.Name);
                        if (targetProp != null && targetProp.CanWrite)
                        {
                            object value = prop.GetValue(result);
                            targetProp.SetValue(resultByGeneric, value);
                        }
                    }
                    return resultByGeneric;
                }
                catch (Exception ex)
                {
                    throw new Exception($"读取点位信息{variable.VarAddress}出错！");
                }
            }
            else
            {
                throw new Exception($"读取点位信息{variable.VarAddress}出错！");
            }
        }

        public async Task<bool> WriteAsync<T>(string deviceNum, Variable<T> variable)
        {
            if (variable == null)
                throw new ArgumentNullException(nameof(variable));

            IProtocolAdapter adapter = await openConnectionAsync(deviceNum);

            var dataType = DataTypeHelper.GetDataTypeFromType(typeof(T));
            if (dataType.HasValue)
            {
                try
                {
                    bool result = await adapter.WriteAsync(variable.VarAddress, dataType.Value, variable.VarValue);
                    return result;
                }
                catch (Exception ex)
                {
                    throw new Exception($"写入点位信息{variable.VarAddress}出错！");
                }
            }
            else
            {
                throw new Exception($"写入点位信息{variable.VarAddress}出错！");
            }
        }

        private async Task<IProtocolAdapter> openConnectionAsync(string deviceNum)
        {
            if (string.IsNullOrEmpty(deviceNum))
                throw new ArgumentNullException(nameof(deviceNum));

            if (!_deviceProtocolDict.TryGetValue(deviceNum, out var adapterOption))
            {
                throw new Exception($"设备{deviceNum}的通信协议配置不正确，不能连接到该设备！");
            }
            IProtocolAdapter adapter = null;
            try
            {
                adapter = await _connectionPool.OpenConnectionAsync(deviceNum, adapterOption);
                return adapter;
            }
            catch (Exception ex)
            {
                throw new Exception($"设备{deviceNum}的通信协议配置不正确，不能连接到该设备！");
            }
        }
    }
}
