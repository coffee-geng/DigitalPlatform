using Coffee.Siemens.Communication;
using Sharp7;
using System.Net.Sockets;
using s7=Coffee.Siemens.Communication;

namespace Coffee.SiemensCommunication
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            var aa = ResponseStatus.ErrorClasses;
            var bb = ResponseStatus.DataReturnCodes;

            //Sharp7LibTest();
            var socket = S7Connect();
            //S7Read(socket);
            //S7Write(socket);
            //S7GetDateTime(socket);
            ReadS7Lib();
            Console.ReadLine();
        }

        static void Sharp7LibTest()
        {
            var client = new Sharp7.S7Client();
            int result = client.ConnectTo("192.168.2.4", 0, 2);
            byte[] buffer = new byte[2];
            result = client.DBRead(100, 0, 2, buffer);

            result = client.DBWrite(100, 0, 2, new byte[] { 0x01, 0x01 });
        }

        static Socket S7Connect()
        {
            //TCP三次握手
            Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
            socket.Connect("192.168.2.4", 102); //102是连接到仿真机默认端口号

            //COTP
            byte[] bytes = new byte[]
            {
                //TPKT
                0x03,
                0x00,
                0x00, 0x16, //报文总的字节数
                //COTP
                0x11,
                0xe0, //TPDU Type 连接请求
                0x00,0x00,
                0x00,0x01,
                0x00,

                //0xc0 
                0xc0,
                0x01, //0xc0 参数长度
                0x0a, //2的10次方 tpdu size=1024

                //0xc1 通信源对象的相关配置 src-tsap
                0xc1,
                0x02, //0xc1 参数长度
                0x10, //S7双边模式
                0x00, //上位机不需要设置机架插槽

                //0xc2 通信目标对象的相关配置 dst-tsap
                0xc2,
                0x02, //0xc2 参数长度
                0x03, //S7单边模式
                0x00, //0~4 插槽 5~7机架  机架 * 32 + 插槽
            };
            socket.Send(bytes);

            byte[] respBytes = new byte[22]; //COTP返回始终是22个字节
            socket.Receive(respBytes, 0, respBytes.Length, SocketFlags.None);

            // Setup Communication
            bytes = new byte[]
            {
                //TPKT
                0x03,
                0x00,
                0x00, 0x19, //报文总的字节数

                //COTP
                0x02, //在COTP中，当前字节以后的字节数
                0xf0, //TPDU Type 数据传输
                0x80, //Last Data Unit: Yes 最高位置1

                // S7 Header
                0x32, //协议ID，默认0x32  0x72是S7 Plus
                0x01, //ROSCTR:  JOB request
                0x00, 0x00, //
                0x00, 0x00, //累加序号 发送什么返回什么
                0x00, 0x08, //Parameter length
                0x00, 0x00, //Data length 没有数据就是0x00

                //S7 Paramter
                0xf0, //Function: Setup communication （附录五）
                0x00, //保留字段
                //任务处理队列长度
                0x00, 0x01, //Max AmQ(parallel jobs with ack) calling 正在处理的过程中，需要应答的任务的队列可以有多少个，默认是1，就是一次可以排多少个工作任务
                0x00, 0x01, //Max AmQ(parallel jobs with ack) called
                0x03,0xc0, // PDU的最大长度，设置的大一些就行 在上位机中设置的在通信过程中传递的数据不要超过960个字节  具体的PLC会反馈一个PDU的最大长度
            };
            socket.Send(bytes);

            respBytes = new byte[27];
            socket.Receive(respBytes);
            ushort maxPduSize = BitConverter.ToUInt16(new byte[] { respBytes[26], respBytes[25] });

            return socket;
        }

        static void S7Read(Socket socket)
        {
            byte[] bytes = new byte[]
            {
                //TPKT
                0x03,
                0x00,
                0x00, 0x37, //报文总的字节数

                //COTP
                0x02, //在COTP中，当前字节以后的字节数
                0xf0, //TPDU Type 数据传输
                0x80, //Last Data Unit: Yes 最高位置1

                // S7 Header
                0x32, //协议ID，默认0x32  0x72是S7 Plus
                0x01, //ROSCTR:  JOB request
                0x00, 0x00, //
                0x00, 0x00, //累加序号 发送什么返回什么
                0x00, 0x26, //Parameter length
                0x00, 0x00, //Data length 没有数据就是0x00

                //S7 Paramter
                0x04, //Function: Read Var (0x04)[附录五]
                0x03, //Item的数量 Item中包含了请求的地址以及数据类型相关信息 因为现在就请求一个地址，所以为1

                //S7 Paramter - Item1
                0x12, //结构标识，一般默认0x12
                0x0a, //当前Item部分，此字节往后的字节长度
                0x10, //Syntax Id: S7ANY (0x10)[附录六]
                0x02, //Parameter Item 表中的 Transport size: BYTE (2)[附录七] 传输数据类型作为读取的单位
                0x00, 0x02, //根据传输数据类型，读取的数量
                0x00, 0x64, //数据块编号（注意：对应的是DB块的编号，如果区域不是DB块，则为0x00, 0x00） 例如：DB100.DBB0
                0x84, //存储区域 Area （附录八）
                //变量地址（占3个字节）18-3位： Byte Address 2-0位：Bit Address
                //例如： 
                //DB1.DBX100.5
                //0x00 0x03 0x25
                //0000 0000 0000 0110 0100 101
                //0000 0000 0000 0011 0010 0101
                0x00,0x00,0x00,

                //S7 Paramter - Item2
                0x12, //结构标识，一般默认0x12
                0x0a, //当前Item部分，此字节往后的字节长度
                0x10, //Syntax Id: S7ANY (0x10)[附录六]
                0x04, //Parameter Item 表中的 Transport size: WORD (2)[附录七] 传输数据类型作为读取的单位
                0x00, 0x01, //根据传输数据类型，读取的数量
                0x00, 0x64, //数据块编号（注意：对应的是DB块的编号，如果区域不是DB块，则为0x00, 0x00） 例如：DB100.DBW5
                0x84, //存储区域 Area （附录八）
                //变量地址（占3个字节）18-3位： Byte Address 2-0位：Bit Address
                0x00,0x00,0x50, //

                //S7 Paramter - Item3
                0x12, //结构标识，一般默认0x12
                0x0a, //当前Item部分，此字节往后的字节长度
                0x10, //Syntax Id: S7ANY (0x10)[附录六]
                0x01, //Parameter Item 表中的 Transport size: BIT (2)[附录七] 传输数据类型作为读取的单位
                //根据传输数据类型，读取的数量。只支持读一个BIT
                //注意：如果请求BIT的Item不是最后一个Item，需要添加一个0x00的FillBtye字节；如果是最后一个Item就不需要添加
                0x00, 0x01,
                0x00, 0x00, //数据块编号（注意：对应的是DB块的编号，如果区域不是DB块，则为0x00, 0x00） 例如：DB100.DBW5
                0x82, //存储区域 Area （附录八）
                //变量地址（占3个字节）18-3位： Byte Address 2-0位：Bit Address
                0x00,0x00,0x50 //
            };

            socket.Send(bytes);
        }

        static void S7Write(Socket socket)
        {
            byte[] bytes = new byte[]
            {
                //TPKT
                0x03,
                0x00,
                0x00, 0x27, //报文总的字节数

                //COTP
                0x02, //在COTP中，当前字节以后的字节数
                0xf0, //TPDU Type 数据传输
                0x80, //Last Data Unit: Yes 最高位置1

                // S7 Header
                0x32, //协议ID，默认0x32  0x72是S7 Plus
                0x01, //ROSCTR:  JOB request
                0x00, 0x00, //
                0x00, 0x00, //累加序号 发送什么返回什么
                0x00, 0x0e, //Parameter length
                0x00, 0x08, //Data length 没有数据就是0x00

                //S7 Paramter
                0x05, //Function: Write Var (0x05)[附录五]
                0x01, //Item的数量 Item中包含了请求的地址以及数据类型相关信息 因为现在就请求一个地址，所以为1

                //S7 Paramter - Item1
                0x12, //结构标识，一般默认0x12
                0x0a, //当前Item部分，此字节往后的字节长度
                0x10, //Syntax Id: S7ANY (0x10)[附录六]
                0x02, //Parameter Item 表中的 Transport size: BYTE (2)[附录七] 传输数据类型作为读取的单位
                0x00, 0x04, //根据传输数据类型，写入的数量
                0x00, 0x64, //数据块编号（注意：对应的是DB块的编号，如果区域不是DB块，则为0x00, 0x00） 例如：DB100.DBB0
                0x84, //存储区域 Area （附录八）
                //变量地址（占3个字节）18-3位： Byte Address 2-0位：Bit Address
                0x00,0x00,0x50,

               //S7 Data - Item1
               0x00, //Return code: Reserved (0x00) [附录九]
               0x04, //Data Item 表中的 Transport size: WORD, DWORD, BYTE 都是0x04
               0x00, 0x20, //数据长度，按照bit计算。例如：写入2个字节，数据长度是16位，即0x10，而不是2
               0x02, 0x9a, 0x02, 0x9b  //写入的数据字节 这里写入的是666
            };

            socket.Send(bytes);
        }

        static void S7GetDateTime(Socket socket)
        {
            byte[] bytes = new byte[]
            {
                //TPKT
                0x03,
                0x00,
                0x00, 0x1d, //报文总的字节数

                //COTP
                0x02, //在COTP中，当前字节以后的字节数
                0xf0, //TPDU Type 数据传输
                0x80, //Last Data Unit: Yes 最高位置1

                // S7 Header
                0x32, //协议ID，默认0x32  0x72是S7 Plus
                0x07, //ROSCTR:  User Data
                0x00, 0x00, //
                0x00, 0x00, //累加序号 发送什么返回什么
                0x00, 0x08, //Parameter length
                0x00, 0x04, //Data length 没有数据就是0x00

                //S7-Parameter
                0x00,0x01,0x12, //Parameter head
                0x04, //Parameter部分后续字节长度
                0x11,
                //前4位 如果是0100 Type:Request 如果是1000 Type:Response
                //后4为 0111 Function group: Time functions
                0x47,
                0x01, //Sub function: Read clock
                0x00, //Sequence number

                //S7 -Data
                0x0a, //Return code: Object does not exist (0x0a)
                0x00, //Transport size: NULL (0x00)
                0x00,0x00 //Data部分后续字节的长度
            };
            socket.Send(bytes);
        }

        //2025-10-01 20:05:00 Wednesday
        static void S7SetDateTime(Socket socket)
        {
            byte[] bytes = new byte[]
            {
                //TPKT
                0x03,
                0x00,
                0x00, 0x27, //报文总的字节数

                //COTP
                0x02, //在COTP中，当前字节以后的字节数
                0xf0, //TPDU Type 数据传输
                0x80, //Last Data Unit: Yes 最高位置1

                // S7 Header
                0x32, //协议ID，默认0x32  0x72是S7 Plus
                0x07, //ROSCTR:  User Data
                0x00, 0x00, //
                0x00, 0x00, //累加序号 发送什么返回什么
                0x00, 0x08, //Parameter length
                0x00, 0x1e, //Data length 没有数据就是0x00

                //S7-Parameter
                0x00,0x01,0x12, //Parameter head
                0x04, //Parameter部分后续字节长度
                0x11, //Method (Request/Response) : Request (0x11)
                //前4位 如果是0100 Type:Request 如果是1000 Type:Response
                //后4为 0111 Function group: Time functions
                0x47,
                0x02, //Sub function: Set clock
                0x00, //Sequence number: 0

                //S7 -Data
                0xff, //Return code: OSuccess (0xff)
                0x09, //Transport size: OCTET STRING (0x09)
                0x00,0x0a, //Data部分后续字节的长度
                //时间
                0x00, //S7 Timestamp - Reserved: 0x00
                0x19, //S7 Timestamp - Year 1: 19
                0x24, //S7 Timestamp - Year 2: 21
                0x10, //S7 Timestamp - Month: 5
                0x01, //S7 Timestamp - Day: 8
                0x20, //S7 Timestamp - Hour: 20
                0x05, //S7 Timestamp - Minute: 00
                0x00, //S7 Timestamp - Second: 00
                0x03 // 毫秒和星期一共2个字节，其中低4位表示星期，高位5到16位表示毫秒
            };
            socket.Send(bytes);
        }

        static void ReadS7Lib()
        {
            s7.S7Client client = new s7.S7Client();
            client.Connect("192.168.2.4", 0, 1);
            var p1 = new DataParameter(S7_Functions.ReadVariable)
            {
                Area = S7_Areas.DB,
                DBNumber = 100,
                ByteAddress = 0,
                Count = 2,
                ParameterVarType = S7_ParameterVarType.WORD,
                DataBytes = new byte[] { 0x00, 0xbb }
            };
            //var p2= new DataParameter(S7_Functions.ReadVariable)
            //{
            //    Area = S7_Areas.Q,
            //    DBNumber = 0,
            //    ByteAddress = 0,
            //    BitAddress = 5,
            //    Count = 3,
            //    ParameterVarType = S7_ParameterVarType.BIT
            //};
            client.Write(p1);
        }
    }
}
