using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Zhaoxi.FinsProtocol.Server
{
    class Program
    {
        static SerialPort serialPort = null;
        static Socket socketServer = null;
        static List<Socket> clients = new List<Socket>();

        static bool isExit = false;
        static List<Task> tasks = new List<Task>();

        static Program program = new Program();

        AutoResetEvent autoResetEvent = new AutoResetEvent(false);

        static void Main(string[] args)
        {
            Console.WriteLine(">>>> 欢迎使用朝夕教育FinsTCP协议学习仿真服务器 <<<<");
            Console.WriteLine("");
            Console.WriteLine(">> 该服务器为串口转网口（FinsTCP）服务器，串口连接CX-Simulator，而后启动TCP服务");
            Console.WriteLine(">>");

            string comStr = System.Configuration.ConfigurationManager.AppSettings["port"].ToString();
            int baudRate = 9600;
            Parity parity = Parity.None;
            int dataBits = 8;
            StopBits stopBits = StopBits.One;
            int nodeAddr = 10;

            Console.WriteLine(">> 串口参数如下：");
            Console.WriteLine($">> 名称：{comStr} / 波特率：{baudRate} / 数据位：{dataBits} / 校验位：{parity} / 停止位：{stopBits} \n>> Node address：{nodeAddr}");
            while (!program.ConnectSerialPort(comStr, baudRate, parity, dataBits, stopBits))
            {
                Console.WriteLine(">> 串口连接失败，正在重试....");
                Thread.Sleep(1000);
            }
            Console.WriteLine(">> 串口已打开！");
            Console.WriteLine(">> ");

            while (!program.Start())
            {
                Console.WriteLine(">> TCP服务启动失败，正在重试....");
                Thread.Sleep(1000);
            }
            Console.WriteLine(">> FinsTCP服务器启动成功！IP:127.0.0.1  Port:9600\n");



            Console.ReadKey();

            Console.WriteLine(">> 程序正在退出....");
            program.Stop();
            isExit = false;
            Task.WaitAll(tasks.ToArray(), 5000);
        }

        bool ConnectSerialPort(string comName, int baudRate, Parity parity, int dataBits, StopBits stopBits)
        {
            try
            {
                serialPort = new SerialPort(comName, baudRate, parity, dataBits, stopBits);
                serialPort.Open();
                serialPort.DataReceived += SerialPort_DataReceived;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(">> 连接异常：" + ex.Message);
            }
            return false;
        }

        static List<byte> bytes = new List<byte>();
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            while (serialPort.BytesToRead > 0)
            {
                byte[] buffer = new byte[serialPort.BytesToRead];
                serialPort.Read(buffer, 0, serialPort.BytesToRead);
                bytes.AddRange(buffer);
                if (bytes.Last() == 0x0d)
                {
                    Console.WriteLine(">> [串口] 发送:" + string.Join("", bytes.Select(r => ((char)r).ToString().ToUpper())));
                    // 一帧报文结束
                    bytes.RemoveRange(0, 15);
                    bytes.RemoveRange(bytes.Count - 4, 4);
                    bytes = AsciiArrayToByteArray(bytes);

                    // 组装TCP响应报文
                    List<byte> finsHeader = new List<byte> { 0x46, 0x49, 0x4E, 0x53 };
                    List<byte> tcpRespBytes = new List<byte>()
                    {
                        0x00,0x00,0x00,0x02,//命令码：读写0x02
                        0x00,0x00,0x00,0x00,//错误码：0x00表示成功
                        // UDP头
                        0xC0,0x00,0x02,0x00,
                        0x7B,//计算机地址
                        0x00,0x00,
                        0x01,//PLC地址
                        0x00,0x00
                    };
                    tcpRespBytes.AddRange(bytes);
                    int len = tcpRespBytes.Count;
                    List<byte> lenBytes = new List<byte> {
                        (byte)(len / 256 / 256 / 256 & 256),
                        (byte)(len / 256 / 256 & 256),
                        (byte)(len / 256 & 256),
                        (byte)(len % 256)
                    };
                    //var lenBytes = bytesStr.Select(str => str.ToString("X2")).ToList();
                    //List lenBytes = new byte[] { (byte)(len / 256 & 256), (byte)(len & 256) };
                    finsHeader.AddRange(lenBytes);
                    finsHeader.AddRange(tcpRespBytes);

                    bytes.Clear();

                    Console.WriteLine(">> [TCP] 发送:" + string.Join(" ", finsHeader.Select(r => r.ToString("x2").ToUpper())));
                    Console.WriteLine("");
                    clientSocket.Send(finsHeader.ToArray());
                    autoResetEvent.Set();
                }
            }

        }

        private List<byte> AsciiArrayToByteArray(List<byte> value)
        {
            List<string> asciiStrList = new List<string>();
            foreach (var item in value)
            {
                asciiStrList.Add(((char)item).ToString());
            }

            // 将每两个Ascii字符组成一个16进制 转换成字节  ---  对应：3
            List<byte> resultBytes = new List<byte>();
            for (int i = 0; i < asciiStrList.Count; i++)
            {
                var stringHex = asciiStrList[i].ToString() + asciiStrList[++i].ToString();
                resultBytes.Add(Convert.ToByte(stringHex, 16));
            }
            return resultBytes;
        }


        /// <summary>
        /// 启动服务
        /// </summary>
        public bool Start()
        {
            try
            {
                //1 创建Socket对象
                socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //2 绑定ip和端口 
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Any, 9600);
                socketServer.Bind(ipEndPoint);
                socketServer.ReceiveTimeout = 600000;
                socketServer.SendTimeout = 600000;

                //3、开启侦听(等待客户机发出的连接),并设置最大客户端连接数为10
                socketServer.Listen(10);

                Task.Run(() => { Accept(socketServer); });

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(">> TCP服务器启动异常：" + ex.Message);
            }
            return false;
        }

        /// <summary>
        /// 停止服务
        /// </summary>
        public void Stop()
        {
            if (socketServer?.Connected ?? false) socketServer.Shutdown(SocketShutdown.Both);
            socketServer?.Close();
        }

        Socket clientSocket = null;
        /// <summary>
        /// 客户端连接到服务端
        /// </summary>
        /// <param name="socket"></param>
        void Accept(Socket socket)
        {
            while (true)
            {
                try
                {
                    try
                    {
                        //阻塞等待客户端连接
                        clientSocket = socket.Accept();
                        clients.Add(clientSocket);
                        Task.Run(() => { Receive(clientSocket); });
                    }
                    catch (SocketException)
                    {
                        isTcpConnected = false;
                        foreach (var item in clients)
                        {
                            if (item?.Connected ?? false) item.Shutdown(SocketShutdown.Both);
                            item?.Close();
                        }
                    }
                }
                catch (SocketException ex)
                {
                    isTcpConnected = false;
                    if (ex.SocketErrorCode != SocketError.Interrupted)
                        Console.WriteLine(ex.Message);
                }

            }
        }

        private byte[] BasicCommand = new byte[]
        {
            0x46, 0x49, 0x4E, 0x53,
            0x00, 0x00, 0x00, 0x0C,// 后面长度
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x01
        };

        private bool ByteArryThan(byte[] items1, byte[] items2)
        {
            if (items1.Length != items2.Length)
                return false;
            else
            {
                //只判断前16个字节
                for (int i = 0; i < 16; i++)
                {
                    if (items1[i] != items2[i])
                        return false;
                }
            }
            return true;
        }


        bool isTcpConnected = false;
        /// <summary>
        /// 接收客户端发送的消息
        /// </summary>
        /// <param name="newSocket"></param>
        void Receive(Socket newSocket)
        {
            while (newSocket.Connected)
            {
                try
                {
                    List<byte> completeRequetData = new List<byte>();

                    byte[] requetData1 = new byte[20];
                    //读取客户端发送过来的数据
                    //Console.WriteLine("1");
                    requetData1 = SocketRead(newSocket, requetData1.Length);
                    //Console.WriteLine("2");
                    if (!isTcpConnected)
                    {
                        Console.WriteLine(">> [TCP] 接收:" + string.Join(" ", requetData1.Select(r => r.ToString("x2").ToUpper())));
                        if (ByteArryThan(requetData1, BasicCommand))
                        {
                            isTcpConnected = true;
                            string respStr = "46 49 4E 53 00 00 00 10 00 00 00 06 00 00 00 00 00 00 00 01 00 00 00 89";
                            Console.WriteLine(">> [TCP] 发送:" + respStr);
                            Console.WriteLine("");
                            var response = respStr.Split(' ').Where(t => t?.Length == 2).Select(t => Convert.ToByte(t, 16)).ToArray();
                            newSocket.Send(response);
                            continue;
                        }
                    }


                    byte[] lenBytes = new byte[4];
                    lenBytes[0] = requetData1[7];
                    lenBytes[1] = requetData1[6];
                    lenBytes[3] = requetData1[4];
                    lenBytes[2] = requetData1[5];
                    int len = BitConverter.ToInt32(lenBytes, 0);
                    len += 8;
                    // 不支持
                    byte[] requetData2 = new byte[len - 20];
                    //Console.WriteLine("3");
                    requetData2 = SocketRead(newSocket, requetData2.Length);
                    //Console.WriteLine("4");

                    completeRequetData.AddRange(requetData1);
                    completeRequetData.AddRange(requetData2);

                    //var isBit = completeRequetData[28] == 0x02 ||
                    //  completeRequetData[28] == 0x30 ||
                    //  completeRequetData[28] == 0x31 ||
                    //  completeRequetData[28] == 0x32 ||
                    //  completeRequetData[28] == 0x33;

                    //var addressLenght = isBit ? 1 : 2;

                    ////1 读 2 写
                    //var readWriteCode = requetData2[27 - 20];
                    ////读写数据长度
                    //var readWriteLength = requetData2[32 - 20] * 256 + requetData2[33 - 20];
                    //if (readWriteCode == 0x02)//写
                    //{
                    //    byte[] requetData3 = SocketRead(newSocket, readWriteLength * addressLenght);
                    //    completeRequetData.AddRange(requetData3);
                    //}

                    Console.WriteLine(">> [TCP] 接收:" + string.Join(" ", completeRequetData.Select(r => r.ToString("x2").ToUpper())));
                    completeRequetData.RemoveRange(0, 26);
                    string pduStr = string.Join(" ", completeRequetData.Select(r => r.ToString("x2").ToUpper()));
                    Console.WriteLine(">> [TCP] 提取:" + pduStr);

                    // 拼接C-Model报文
                    string unit = ConfigurationManager.AppSettings["unit"].ToString();
                    string model = $"@{unit}FA000000A00" + pduStr.Replace(" ", "");
                    Console.WriteLine(">> [串口] 发送:" + model);

                    model += FCS(model) + "*";
                    List<byte> reqBytes = new List<byte>(Encoding.ASCII.GetBytes(model));
                    reqBytes.Add(0x0d);
                    clientSocket = newSocket;
                    serialPort.Write(reqBytes.ToArray(), 0, reqBytes.Count);
                    autoResetEvent.WaitOne();
                    //Console.WriteLine("");

                }
                catch (Exception ex)
                {
                    //if (newSocket?.Connected ?? false) newSocket?.Shutdown(SocketShutdown.Both);
                    //newSocket?.Close();

                    Console.WriteLine(ex.Message);
                    isTcpConnected = false;
                    
                }
            }
        }

        const int BufferSize = 4096;
        /// <summary>
        /// 读取
        /// </summary>
        /// <param name="socket">socket</param>
        /// <param name="receiveCount">读取长度</param>
        /// <returns></returns>
        protected byte[] SocketRead(Socket socket, int receiveCount)
        {
            byte[] receiveBytes = new byte[receiveCount];
            int receiveFinish = 0;
            while (receiveFinish < receiveCount)
            {
                // 分批读取
                int receiveLength = (receiveCount - receiveFinish) >= BufferSize ? BufferSize : (receiveCount - receiveFinish);
                var readLeng = socket.Receive(receiveBytes, receiveFinish, receiveLength, SocketFlags.None);
                if (readLeng == 0)
                {
                    isTcpConnected = false;
                    if (socket.Connected) socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    throw new Exception("连接断开");
                }
                receiveFinish += readLeng;
            }
            return receiveBytes;
        }

        public string FCS(string s)　　//帧校验函数FCS
        {
            //获取s对应的字节数组
            byte[] b = Encoding.ASCII.GetBytes(s);
            // xorResult 存放校验结果。注意：初值去首元素值！
            byte xorResult = b[0];
            // 求xor校验和。
            for (int i = 1; i < b.Length; i++)
            {
                xorResult ^= b[i];
            }
            return Convert.ToString(xorResult, 16).ToUpper().PadLeft(2, '0');

        }
    }
}
