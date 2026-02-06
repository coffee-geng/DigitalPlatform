using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DeviceService
{
    public interface ICommunicationService
    {
        Task<bool> ConnectDeviceAsync(string deviceNum, Dictionary<string, ProtocolParameter> communicationParameters);

        Task<T> ReadAsync<T>(string deviceNum, Variable<T> variable);

        Task<bool> WriteAsync<T>(string deviceNum, Variable<T> variable);

        Task<object> ReadAsync(string deviceNum, Variable variable);

        Task<bool> WriteAsyn(string deviceNum, Variable variable);
    }

    public class CommunicationService : ICommunicationService
    {
        private readonly IProtocolManager _protocolManager;

        private readonly ILogger<CommunicationService> _logger;

        public CommunicationService(IProtocolManager protocolManager, ILoggerFactory loggerFactory) 
        { 
            if (protocolManager == null)
                throw new ArgumentNullException(nameof(protocolManager));
            if (loggerFactory == null) 
                throw new ArgumentNullException(nameof(loggerFactory));
            _protocolManager = protocolManager;
            _logger = loggerFactory.CreateLogger<CommunicationService>();
        }

        public async Task<bool> ConnectDeviceAsync(string deviceNum, Dictionary<string, ProtocolParameter> communicationParameters)
        {
            try
            {
                var adapter = await _protocolManager.ConnectAsync(deviceNum, communicationParameters);
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception("无法连接设备！", ex);
            }
        }

        public async Task<T> ReadAsync<T>(string deviceNum, Variable<T> variable)
        {
            if (string.IsNullOrEmpty(deviceNum))
                throw new ArgumentNullException(nameof(deviceNum));
            if (variable == null)
                throw new ArgumentNullException(nameof(variable));

            var result = await _protocolManager.ReadAsync<T>(deviceNum, variable);
            if (result != null && result.Success && string.IsNullOrEmpty(result.ErrorMessage))
            {
                return result.GetValue<T>();
            }
            else
            {
                throw new Exception($"读取设备{deviceNum}的点位信息{variable.VarAddress}时发生异常！");
            }
        }

        public async Task<object> ReadAsync(string deviceNum, Variable variable)
        {
            if (string.IsNullOrEmpty(deviceNum))
                throw new ArgumentNullException(nameof(deviceNum));
            if (variable == null)
                throw new ArgumentNullException(nameof(variable));

            var result = await _protocolManager.ReadAsync(deviceNum, variable);
            if (result.Success && string.IsNullOrEmpty(result.ErrorMessage))
            {
                return result.Value;
            }
            else
            {
                throw new Exception($"读取设备{deviceNum}的点位信息{variable.VarAddress}时发生异常！");
            }
        }

        public async Task<bool> WriteAsyn(string deviceNum, Variable variable)
        {
            if (string.IsNullOrEmpty(deviceNum))
                throw new ArgumentNullException(nameof(deviceNum));
            if (variable == null)
                throw new ArgumentNullException(nameof(variable));

            try
            {
                var result = await _protocolManager.WriteAsync(deviceNum, variable);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"写入设备{deviceNum}的点位信息{variable.VarAddress}时发生异常！");
            }
        }

        public async Task<bool> WriteAsync<T>(string deviceNum, Variable<T> variable)
        {
            if (string.IsNullOrEmpty(deviceNum))
                throw new ArgumentNullException(nameof(deviceNum));
            if (variable == null)
                throw new ArgumentNullException(nameof(variable));

            try
            {
                var result = await _protocolManager.WriteAsync<T>(deviceNum, variable);
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception($"写入设备{deviceNum}的点位信息{variable.VarAddress}时发生异常！");
            }
        }
    }
}
