using Coffee.Omron.Communication;
using Coffee.Omron.Communication.Base;
using System.IO.Ports;
using System.Net.Sockets;
using System.Text;

namespace Coffee.OmronCommunication
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            //FINS_Serial_TestLib();
            //var b = Convert.FromHexString("006F");
            CIP_Test();
        }

        static void FINS_Serial_Test()
        {
            SerialPort serialPort = new SerialPort("COM1");
            serialPort.Open();
            string unitNo = "10";
            ushort byte_addr = 100;
            byte bit_addr = 0;
            ushort count = 5;
            string cmd = $"@{unitNo}FA000000000010182" + $"{byte_addr.ToString("X4")}{bit_addr.ToString("X2")}{count.ToString("X4")}";
            var fcs = FCS(cmd);
            cmd += fcs;
            cmd += "*\r";
           byte[] req =  Encoding.ASCII.GetBytes(cmd);
            serialPort.Write(req, 0, req.Length);
            byte[] resp = new byte[serialPort.BytesToRead];
            serialPort.Read(resp, 0, resp.Length);
            string str_resp = Encoding.ASCII.GetString(resp);
        }
        static void FINS_Serial_TestLib()
        {
            FINS fins = new FINS("COM1", 9600, 8, Parity.None, StopBits.One);
            fins.Open();
            FINS_Parameter param1 = new FINS_Parameter()
            {
                Area = Area.DM,
                WordAddr = 100,
                BitAddr = 0,
                Count = 1,
                DataType = DataTypes.WORD,
                Data = new byte[] { 0x00, 0x7B, 0X00, 0X7C }
            };
            //var resp = fins.Read(10, param1);

            fins.Write(10, param1);
        }

        static void FINS_TCPLIB_Test()
        {
            
        }

        static void CIP_Test()
        {
            Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Connect("192.168.2.4", 44818);

            //创建CIP Session
            //CIP所有报文按小端处理
            byte[] data = new byte[] {
                0x65, 0x00, //命令码 表示注册Session
                0x04, 0x00, //封装标头后续字节数（命令数据部分字节数）
                0x00, 0x00, 0x00, 0x00, //Session Handle
                0x00, 0x00, 0x00, 0x00,//状态码
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //发送者附加信息 8字节
                0x00, 0x00, 0x00, 0x00, //选项，未启用 默认4个0

                //命令特定数据部分
                0x01, 0x00, //协议版本 固定0x01
                0x00, 0x00 //选项，特定用户 0~7位 保留使用 8~15 未来扩展
            };
            socket.Send(data);

            //接受Session
            byte[] resp = new byte[28];
            socket.Receive(resp, 0, 28, SocketFlags.None);
            string statusCode = string.Join(" ", resp.Skip(8).Take(4).Select(x => x.ToString("X2")));
            byte[] session = resp.Skip(4).Take(4).ToArray();
            string sessionId = string.Join(" ", session.Select(x => x.ToString("X2")));

            //读取单个标签数据 ServerIn
            byte[] dataR = new byte[] {
                0x6F, 0x00, //命令码 表示SendRRData
                0x2C, 0x00, //封装标头后续字节数（命令数据部分字节数）
                session[0], session[1], session[2], session[3], //Session Handle
                0x00, 0x00, 0x00, 0x00, //状态码
                0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, //发送者附加信息 8字节
                0x00, 0x00, 0x00, 0x00, //选项，未启用 默认4个0

                //命令特定数据部分
                0x00, 0x00, 0x00, 0x00, //Interface handle 固定4个0
                0x01, 0x00, //Timeout
                0x02, 0x00, //下面有2个Item Address Item 和 DataItem
                //Address Item
                0x00, 0x00, //封装的项类型
                0x00, 0x00, //当前Item项中后续数据字节数，当前无
                //Data Item
                0xB2, 0x00, //封装的项类型 Unconnected Message
                0x1C, 0x00, //当前Item项中后续数据字节数

                //CIP指令
                0x52, //服务默认0x52
                0x02, //请求路径大小，即多少个字
                0x20,0x06,0x24,0x01, //请求路径，类ID + 实例ID
                0x0A,0x00, //默认超时时间
                
                0x0E,0x00, //从服务标识到服务命令指定数据的长度
                0x4C, //服务标识 Read Tag Sercice

                0x05, //标签后续字数， 从下个字节到标签完的字数 （扩展符号 + 标签字节数 + 标签字节数据）
                0x91, //扩展符号，默认为0x91，表示使用SYM
                0x08, //标签字符数量
                0x53, 0x65, 0x72, 0x76, 0x65, 0x72, 0x49, 0x6e, //ServerIn，标签字节数为单数时，后面补0x00，因为前面指定的是标签后续字数，所以字节数必须是偶数

                0x01,0x00, //服务命令指定数据，默认值
                0x01,0x00, //默认值
                0x01,0x00, //最后字节0x00 可能会填充PLC的插槽号
            };
            socket.Send(data);
        }

        private static string FCS(string cmd)
        {
            byte[] b = Encoding.ASCII.GetBytes(cmd);
            byte xorResult = b[0];
            for (int i = 1; i < b.Length; i++)
            {
                xorResult ^= b[i];
            }

            return xorResult.ToString("X2");
        }
    }
}
