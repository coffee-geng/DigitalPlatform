using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.ModbusLib
{
    public abstract class ModbusSerial : ModbusMaster
    {
        public string PortName { get; set; } = "COM1";

        public int BaudRate { get; set; } = 9600;

        public Parity Parity { get; set; } = Parity.None;

        public int DataBits { get; set; } = 8;

        public StopBits StopBits { get; set; } = StopBits.One;

        public int ReadTimeout { get; set; } = 2000;

        public int ReadBufferSize { get; set; } = 4096;

        public int WriteTimeout { get; set; } = 2000;

        public int WriteBufferSize { get; set; } = 4096;

        private readonly SerialPort _serialPort;

        private readonly CancellationTokenSource _cts = new CancellationTokenSource();

        private static readonly object _lockObj = new object();

        protected ModbusSerial()
        {
            _serialPort = new SerialPort();
        }

        public override void Connect()
        {
            _serialPort.PortName = PortName;
            _serialPort.BaudRate = BaudRate;
            _serialPort.DataBits = DataBits;
            _serialPort.StopBits = StopBits;
            _serialPort.Parity = Parity.None;
            _serialPort.ReadTimeout = ReadTimeout;
            _serialPort.ReadBufferSize = ReadBufferSize;
            _serialPort.WriteTimeout = WriteTimeout;
            _serialPort.WriteBufferSize = WriteBufferSize;

            _serialPort.Open();

            //为了实现读写同步，启动一个后台线程，从队列中逐个获取任务数据，并执行读取或写入请求
            //这样用户程序中可以使用异步方法将读取或写入操作缓存到后台队列中，并在总线空闲时执行操作，避免并发问题
            Task.Run(() =>
            {
                while (!_cts.IsCancellationRequested)
                {
                    if (_serialPort == null || base.requestTransactionQueue.Count == 0)
                    {
                        Task.Delay(100);
                        continue;
                    }

                    if (requestTransactionQueue.TryDequeue(out RequestTransaction transaction))
                    {
                        byte[] responseData = null; //响应的实际数据（排除响应头和校验码）
                        Exception error = null;
                        try
                        {
                            byte[] responseBytes = this.SendAndReceive(transaction.RequestBytes, transaction.ResponseLength);
                            //校验响应的数据是否合法，并返回响应的实际数据部分
                            responseData = VerifyCode(responseBytes, transaction.ResponseLength, transaction.RequestFunction);
                        }
                        catch (Exception ex)
                        {
                            error = ex;
                        }
                        finally
                        {
                            //执行完成时，将读写的返回结果通过回调函数传出
                            transaction.Completed?.Invoke(new ReadWriteModbusCallbackResult()
                            {
                                IsCompleted = error == null ? true : false,
                                Error = error,
                                ResultData = responseData
                            });
                        }
                    }
                }
            });
        }

        public override void Disconnect()
        {
            _serialPort.Close();
            _cts.Cancel();
        }

        /// <summary>
        /// 校验响应的数据是否合法，并返回响应的实际数据部分
        /// </summary>
        /// <param name="response">响应返回的字节数组</param>
        /// <param name="dataLength">响应实际数据部分字节数</param>
        /// <returns>返回响应的实际数据部分</returns>
        protected virtual byte[] VerifyCode(byte[] response, int dataLength, Functions function)
        {
            return null;
        }

        protected override byte[] SendAndReceive(byte[] bytes, int len)
        {
            lock (_lockObj)
            {
                if (!_serialPort.IsOpen)
                {
                    throw new InvalidOperationException("串口对象未连接！");
                }

                _serialPort.Write(bytes, 0, bytes.Length);

                List<byte> responseBytes = new List<byte>();
                try
                {
                    do
                    {
                        //由于数据不能保证一次性就全部发送到缓冲区，可能要多次才能全部接收len指定长度的数据，所以通过使用ReadByte从缓冲区读一个字节，替代使用Read方法从缓冲区读取buffer指定长度的字节。
                        //由于指定了读写的超时时间，所以可以读取有效时间内的所有数据
                        responseBytes.Add((byte)_serialPort.ReadByte()); 
                    }
                    while(responseBytes.Count < len);
                }
                catch (Exception ex)
                {
                    throw new Exception("读取或写入数据超时！当前操作无效！");
                }
                return responseBytes.ToArray();
            }
        }

        protected Task SendAndReceiveAsync(byte[] bytes, int len, Functions function, Action<ReadWriteModbusCallbackResult> callback)
        {
            var transaction = new RequestTransaction()
            {
                TransactionId = CreateTransactionId(),
                RequestBytes = bytes,
                ResponseLength = len,
                RequestFunction = function,
                Completed = callback
            };
            requestTransactionQueue.Enqueue(transaction);
            return Task.CompletedTask;
        }

        protected byte[] CRC16(List<byte> data)
        {
            if (data == null || !data.Any())
                throw new ArgumentException("");

            //运算
            ushort crc = 0xFFFF;
            for (int i = 0; i < data.Count; i++)
            {
                crc = (ushort)(crc ^ (data[i]));
                for (int j = 0; j < 8; j++)
                {
                    crc = (crc & 1) != 0 ? (ushort)((crc >> 1) ^ 0xA001) : (ushort)(crc >> 1);
                }
            }
            byte hi = (byte)((crc & 0xFF00) >> 8);  //高位置
            byte lo = (byte)(crc & 0x00FF);         //低位置

            return new byte[] { hi, lo };
        }

        protected byte LRC(byte[] value)
        {
            if (value == null) return 0x00;

            int sum = 0;
            for (int i = 0; i < value.Length; i++)
            {
                sum += value[i];
            }

            sum = sum % 256;  // 只拿低位的一个字节数据
            sum = 256 - sum;

            return (byte)sum;
        }
    }
}
