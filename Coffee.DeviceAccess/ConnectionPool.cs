using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Coffee.DeviceAdapter;
using Microsoft.Extensions.Logging;

namespace Coffee.DeviceAccess
{
    public interface IConnectionPool : IDisposable
    {
        Task<IProtocolAdapter> OpenConnectionAsync(string connectionId, IProtocolOptions protocolOption);

        Task ReleaseConnectionAsync(string connectionId);
    }

    public class ConnectionPool : IConnectionPool
    {
        private readonly ConcurrentDictionary<string, PooledConnection> _connectionDict;

        private readonly IProtocolFactory _protocolFactory;

        private readonly ILogger<ConnectionPool> _logger;

        private readonly int _maxPoolSize;

        public ConnectionPool(IProtocolFactory protocolFactory, ILoggerFactory loggerFactory)
        {
            if (protocolFactory == null)
                throw new ArgumentNullException(nameof(protocolFactory));
            if (loggerFactory == null)
                throw new ArgumentNullException(nameof(loggerFactory));

            _protocolFactory = protocolFactory;
            _logger = loggerFactory.CreateLogger<ConnectionPool>();
            _maxPoolSize = 100;

            _connectionDict = new ConcurrentDictionary<string, PooledConnection>();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public async Task<IProtocolAdapter> OpenConnectionAsync(string connectionId, IProtocolOptions protocolOption)
        {
            if (_connectionDict.TryGetValue(connectionId, out PooledConnection connection))
            {
                if (connection.Adapter != null && connection.Adapter.IsConnected)
                {
                    connection.LastUsed = DateTime.UtcNow;
                    return connection.Adapter;
                }
                //连接已断开，移除后重新创建
                _connectionDict.TryRemove(connectionId, out _);
                connection.Adapter?.Dispose();
            }

            //检查连接池大小，清除空闲的连接
            if (_connectionDict.Count > _maxPoolSize)
            {
                CleanupIdleConnections();
            }

            //创建新连接
            var adapter = _protocolFactory.CreateAdapter(protocolOption);
            if (!await adapter.ConnectAsync())
            {
                throw new InvalidOperationException($"Failed to connect using protocol options: {protocolOption}");
            }
            var newConnection = new PooledConnection()
            {
                Adapter = adapter,
                ProtocolOptions = protocolOption,
                Created = DateTime.UtcNow,
                LastUsed = DateTime.UtcNow
            };
            _connectionDict[connectionId] = newConnection;
            return adapter;
        }

        private void CleanupIdleConnections()
        {
            var idleTimeout = TimeSpan.FromMinutes(5);
            var now = DateTime.UtcNow;

            var idleConnections = _connectionDict.Where(pair => (now - pair.Value.LastUsed) > idleTimeout).ToList();
            foreach (var connection in idleConnections)
            {
                if (_connectionDict.TryRemove(connection.Key, out var pooledConnection))
                {
                    pooledConnection.Adapter?.Dispose();
                }
            }
        }

        public Task ReleaseConnectionAsync(string connectionId)
        {
            throw new NotImplementedException();
        }
    }

    internal class PooledConnection
    {
        public IProtocolAdapter Adapter { get; set; }

        public IProtocolOptions ProtocolOptions { get; set; }

        public DateTime Created { get; set; }

        public DateTime LastUsed { get; set; }
    }
}
