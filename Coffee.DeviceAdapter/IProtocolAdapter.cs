
using Coffee.ModbusLib;
using System.IO.Ports;

namespace Coffee.DeviceAdapter
{
    public interface IProtocolOptions
    {
        void Config(string paramName, string paramValue, Type paramType);

        EndianTypes EndianType { get; set; }
    }

    public interface ISerialPortOptions : IProtocolOptions
    {
        string PortName { get; set; }

        int BaudRate { get; set; }

        Parity Parity { get; set; }

        int DataBits { get; set; }

        StopBits StopBits { get; set; }

        int ReadTimeout { get; set; }

        int ReadBufferSize { get; set; }

        int WriteTimeout { get; set; }

        int WriteBufferSize { get; set; }
    }

    public interface ISocketOptions : IProtocolOptions
    {
        string IP { get; set; }

        int Port { get; set; }

        int ReceiveTimeout { get; set; }

        int ReceiveBufferSize { get; set; }

        int SendTimeout { get; set; }

        int SendBufferSize { get; set; }
    }

    public interface IProtocolAdapter : IDisposable
    {
        ProtocolType ProtocolType { get; }

        bool IsConnected { get; }

        #region 异步方法
        Task<bool> ConnectAsync();

        Task DisconnectAsync();

        Task<ProtocolData> ReadAsync(string address, DataType dataType, ushort length = 1, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD);

        Task<bool> WriteAsync(string address, DataType dataType, object value, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD);

        Task<ReadResponseParameter> ReadAsync(ReadRequestParameter parameter, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD);

        Task<WriteResponseParameter> WriteAsync(WriteRequestParameter parameter, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD);

        Task<BatchReadResponseParameter> BatchReadAsync(IEnumerable<ReadRequestParameter> parameters, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD);

        Task<BatchWriteResponseParameter> BatchWriteAsync(IEnumerable<WriteRequestParameter> parameters, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD);
        #endregion

        #region 同步方法
        bool Connect();

        void Disconnect();

        ProtocolData Read(string address, DataType dataType, ushort length = 1, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD);

        bool Write(string address, DataType dataType, object value, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD);

        ReadResponseParameter Read(ReadRequestParameter parameter, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD);

        WriteResponseParameter Write(WriteRequestParameter parameter, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD);

        BatchReadResponseParameter BatchRead(IEnumerable<ReadRequestParameter> parameters, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD);

        BatchWriteResponseParameter BatchWrite(IEnumerable<WriteRequestParameter> parameters, byte slaveId = 1, EndianTypes endianTypes = EndianTypes.ABCD);
        #endregion
    }

    public class ReadRequestParameter
    {
        public string Address { get; set; }

        public DataType DataType { get; set; }

        public ushort Length { get; set; } = 1;
    }

    public class WriteRequestParameter
    {
        public string Address { get; set; }

        public DataType DataType { get; set; }

        public object Value { get; set; }
    }

    public class ReadResponseParameter
    {
        public ReadResponseParameter(ProtocolData result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }
            _result = result;
        }

        private ProtocolData _result;

        public bool Success
        {
            get { return _result.Success; }
        }
        public string ErrorMessage
        {
            get { return _result.ErrorMessage; }
        }

        public ProtocolType ProtocolType
        {
            get { return _result.ProtocolType; }
        }

        public DataType DataType
        {
            get { return _result.DataType; }
        }

        public object Value
        {
            get { return _result.Value; }
        }

        public DateTime ReceivedTime
        {
            get { return _result.ReceivedTime; }
        }
    }

    public class WriteResponseParameter
    {
        public bool Success { get; set; }

        public string ErrorMessage { get; set; }

        public DateTime ReceivedTime { get; set; }

        public ProtocolType ProtocolType { get; set; }

        public DataType DataType { get; set; }

        public object Value { get; set; }
    }

    public class BatchReadResponseParameter
    {
        public bool Success { get; set; }

        public string ErrorMessage { get; set; }

        public List<ReadResponseParameter> Results { get; } = new List<ReadResponseParameter>();
    }

    public class BatchWriteResponseParameter
    {
        public bool Success { get; set; }

        public string ErrorMessage { get; set; }

        public List<WriteResponseParameter> Results { get; set; } = new List<WriteResponseParameter>();
    }

    /// <summary>
    /// 协议类型枚举
    /// </summary>
    public enum ProtocolType
    {
        ModbusRTU,
        ModbusTCP,
        ModbusASCII,
        SiemensS7,
        OmronFinsTCP
    }

    /// <summary>
    /// 数据类型枚举
    /// </summary>
    public enum DataType
    {
        Bit,
        Byte,
        Int16,
        UInt16,
        Int32,
        UInt32,
        Float,
        Double,
        String,
        ByteArray
    }

    /// <summary>
    /// 字节序
    /// </summary>
    public enum EndianTypes
    {
        ABCD,
        CDAB,
        BADC,
        DCBA,
        ABCDEFGH,
        GHEFCDAB,
        BADCFEHG,
        HGFEDCBA
    }

    /// <summary>
    /// 协议数据统一格式
    /// </summary>
    public abstract class ProtocolData
    {
        public bool Success { get; set; }

        public string ErrorMessage { get; set; }

        public ProtocolType ProtocolType { get; set; }

        public DataType DataType { get; set; }

        public int DataLength { get; set; } = 1;

        public object Value { get; set; } //以对象的形式返回值

        public byte[] Bytes { get; set; } //以字节数组的形式返回值

        public DateTime ReceivedTime { get; set; }
    }

    public abstract class ProtocolData<T> : ProtocolData
    {
        public virtual T GetValue<T>() => (T)Convert.ChangeType(Value, typeof(T));
    }
}
