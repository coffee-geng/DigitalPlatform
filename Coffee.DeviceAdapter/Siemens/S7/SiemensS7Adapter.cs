using Coffee.ModbusLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DeviceAdapter
{
    public class SiemensS7Adapter : IProtocolAdapter
    {
        #region IProtocolAdapter接口
        public ProtocolType ProtocolType => throw new NotImplementedException();

        public bool IsConnected => throw new NotImplementedException();

        public BatchReadResponseParameter BatchRead(IEnumerable<ReadRequestParameter> parameters, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            throw new NotImplementedException();
        }

        public Task<BatchReadResponseParameter> BatchReadAsync(IEnumerable<ReadRequestParameter> parameters, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            throw new NotImplementedException();
        }

        public BatchWriteResponseParameter BatchWrite(IEnumerable<WriteRequestParameter> parameters, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            throw new NotImplementedException();
        }

        public Task<BatchWriteResponseParameter> BatchWriteAsync(IEnumerable<WriteRequestParameter> parameters, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            throw new NotImplementedException();
        }

        public bool Connect()
        {
            throw new NotImplementedException();
        }

        public Task<bool> ConnectAsync()
        {
            throw new NotImplementedException();
        }

        public void Disconnect()
        {
            throw new NotImplementedException();
        }

        public Task DisconnectAsync()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public ProtocolData Read(string address, DataType dataType, ushort length = 1, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            throw new NotImplementedException();
        }

        public ReadResponseParameter Read(ReadRequestParameter parameter, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            throw new NotImplementedException();
        }

        public Task<ProtocolData> ReadAsync(string address, DataType dataType, ushort length = 1, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            throw new NotImplementedException();
        }

        public Task<ReadResponseParameter> ReadAsync(ReadRequestParameter parameter, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            throw new NotImplementedException();
        }

        public bool Write(string address, DataType dataType, object value, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            throw new NotImplementedException();
        }

        public WriteResponseParameter Write(WriteRequestParameter parameter, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            throw new NotImplementedException();
        }

        public Task<bool> WriteAsync(string address, DataType dataType, object value, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            throw new NotImplementedException();
        }

        public Task<WriteResponseParameter> WriteAsync(WriteRequestParameter parameter, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD)
        {
            throw new NotImplementedException();
        }


        #endregion
    }
}
