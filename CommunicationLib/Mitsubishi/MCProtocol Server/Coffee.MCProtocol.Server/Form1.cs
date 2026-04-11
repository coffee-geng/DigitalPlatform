using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Coffee.MCProtocol.Server
{
    public partial class Form1 : Form
    {
        //private AxActProgTypeLib.AxActProgType axActProgType = new AxActProgTypeLib.AxActProgType();
        //private AxActUtlTypeLib.AxActUtlType axActUtlType = new AxActUtlTypeLib.AxActUtlType();

        public Form1()
        {
            InitializeComponent();

            //var vvv = Convert.ToUInt32("000015",16);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.button1.Enabled = false;
            if (this.button1.Text == "启动")
            {
                axActProgType1.ActUnitType = 0x30;
                axActProgType1.ActProtocolType = 0x00;
                int code = axActProgType1.Open();
                if (code == 0)
                    Console.WriteLine(">> MX Component 连接成功");
                else
                {
                    Console.WriteLine(">> MX Component 连接失败！");
                }

                Task task = Task.Run(async () =>
                {
                    while (!this.Start())
                    {
                        Console.WriteLine(">> TCP服务启动失败，正在重试....");
                        await Task.Delay(1000);
                    }
                    Console.WriteLine($">> MC TCP服务器启动成功！IP:{this.textBox2.Text}  Port:{this.textBox1.Text}\n");
                    isTcpConnected = true;
                });
                Task.WaitAll(new Task[] { task });

                if (isTcpConnected)
                    this.button1.Text = "停止";
            }
            else if (this.button1.Text == "停止")
            {
                int code = axActProgType1.Close();
                if (code == 0)
                    Console.WriteLine(">> MX Component 已断开连接");
                else
                    Console.WriteLine(">> MX Component 断开失败");

                try
                {
                    this.Stop();

                    Console.WriteLine(">> TCP服务已停止");

                    this.button1.Text = "启动";
                }
                catch (Exception ex)
                {
                    Console.WriteLine($">> TCP服务停止失败！{ex.Message}");
                }
            }
            this.button1.Enabled = true;

            //Task.Run(async () =>
            //{
            //    while (true)
            //    {
            //        await Task.Delay(2000);

            //        axActProgType1.ReadDeviceRandom2("D0", 1, out short value);

            //        this.Invoke(new Action(() =>
            //        {
            //            this.listBox1.Items.Insert(0, $"{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")} : {value}");
            //        }));
            //    }
            //});
        }

        protected override void OnClosed(EventArgs e)
        {
            axActProgType1.Close();
        }


        Socket socketServer = null;
        Socket clientSocket = null;
        bool isTcpConnected = false;
        /// <summary>
        /// 启动服务
        /// </summary>
        private bool Start()
        {
            try
            {
                //1 创建Socket对象
                socketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

                //2 绑定ip和端口 
                IPEndPoint ipEndPoint = new IPEndPoint(IPAddress.Parse(this.textBox2.Text), int.Parse(this.textBox1.Text));
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
                    //阻塞等待客户端连接
                    clientSocket = socket.Accept();
                    Task.Run(() => { Receive(clientSocket); });
                }
                catch (SocketException ex)
                {
                    isTcpConnected = false;

                    if (clientSocket?.Connected ?? false)
                        clientSocket.Shutdown(SocketShutdown.Both);
                    clientSocket?.Close();

                    if (ex.SocketErrorCode != SocketError.Interrupted)
                        Console.WriteLine(ex.Message);
                }
            }
        }

        /// <summary>
        /// 接收客户端发送的消息
        /// </summary>
        /// <param name="newSocket"></param>
        void Receive(Socket newSocket)
        {
            while (newSocket.Connected)
            {
                string pType = "";
                this.Invoke(new Action(() =>
                {
                    pType = this.comboBox1.Text;
                }));
                try
                {
                    List<byte> completeRequetData = new List<byte>();

                    //Console.WriteLine("2");
                    if (pType == "Qna-3E")
                    {
                        Console.WriteLine();
                        // 接收
                        List<byte> respBytes = Qna3EReceive(newSocket);
                        // 处理
                        Qna3EProcess(respBytes);
                    }
                    else if (pType == "A-1E")
                    {
                        // 接收
                        // 处理

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    isTcpConnected = false;
                }
            }
        }

        List<(byte, string)> Area = new List<(byte, string)>() {
            (0x9C,"X"),
            (0x9D,"Y"),
            (0x90,"M"),
            (0x92,"L"),
            (0x98,"S"),
            (0xA0,"B"),
            (0x93,"F"),
            (0xC1,"TS"),
            (0xC0,"TC"),
            (0xC2,"TN"),
            (0xC4,"CS"),
            (0xC3,"CC"),
            (0xC5,"CN"),
            (0xA8,"D"),
            (0xB4,"W"),
            (0xAF,"R"),
            (0xB0,"ZR"),
        };
        private void Qna3EProcess(List<byte> reqBytes)
        {
            int radix = 16;
            this.Invoke(new Action(() =>
            {
                radix = int.Parse(this.comboBox2.Text);
            }));

            Console.WriteLine($">> [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] 请求:" + string.Join(" ", reqBytes.Select(b => b.ToString("X2"))));
            // 成批读取
            // 按字从D区读取N个字，1个字1个字地址
            // 按字从Y区读取N个字，只能从每个字的头地址开始读，即可以指定地址为Y00,Y20，而不能指定Y10
            // 按位从Y区读取N个位，1个位1个位地址
            // 按位从D区读取N个位，16个位1个字地址
            if (reqBytes[11] == 0x01 && reqBytes[12] == 0x04)
            {
                // 软元件编号
                uint addr = BitConverter.ToUInt32(new byte[] { reqBytes[15], reqBytes[16], reqBytes[17], 0x00 }, 0);
                // 软元件代码
                byte code = reqBytes[18];
                string area_str = Area.FirstOrDefault(a => a.Item1 == code).Item2;
                // 读取点数
                ushort count = BitConverter.ToUInt16(new byte[] { reqBytes[19], reqBytes[20] }, 0);

                ushort countByAddr = count; //从当前区域读取的地址数
                if (new byte[] { 0x9C, 0x9D, 0x90 }.Contains(code))
                {
                    //if (reqBytes[13] == 0x00) //如果是在位区域按字读取
                    //{
                    //    countByAddr *= 16;
                    //}
                }
                else if (reqBytes[13] == 0x01) //如果是在字区域按位读取
                {
                    countByAddr = countByAddr % 16 == 0 ? (ushort)(countByAddr / 16) : (ushort)(countByAddr / 16 + 1);
                }

                short[] values = new short[count];
                string[] addrs = new string[countByAddr];

                for (int i = 0; i < countByAddr; i++)
                {
                    if (new byte[] { 0x9C, 0x9D, 0x90 }.Contains(code))//从位软元件区域读
                    {
                        int offset = i;
                        if (reqBytes[13] == 0x00) //如果是在位区域按字读取，仅读取每个字的第一个位，地址偏移按照16的倍数进行计算
                        {
                            offset = i * 16;
                        }
                        if (new byte[] { 0x9C, 0x9D }.Contains(code))
                        {
                            if (radix == 16)
                            {
                                addr = Convert.ToUInt32(reqBytes[17].ToString("X2") + reqBytes[16].ToString("X2") + reqBytes[15].ToString("X2"), 16);
                                addrs[i] = area_str + (addr + offset).ToString("X");
                            }
                            else if (radix == 8)
                            {
                                addrs[i] = area_str + Convert.ToString(addr + offset, 8); //10进制转8进制
                            }
                        }
                        else
                        {
                            addrs[i] = area_str + (addr + offset).ToString("X");
                        }
                    }
                    else
                    {
                        addrs[i] = area_str + (addr + i).ToString();
                    }
                }
                Console.WriteLine(">> 请求地址:" + string.Join(" ", addrs));

                int result = 0;
                //注意：按字从位软元件区域读取状态位，必须从每个字的第一个位开始读，即地址必须是Y00,Y20,Y40...，不能是Y10,Y30
                //如果是从位软元件区域按字读，则需一个字一个字的读取，每次读一个字
                if (new byte[] { 0x9C, 0x9D, 0x90 }.Contains(code) && reqBytes[13] == 0x00)
                {
                    for (int k = 0; k < addrs.Length; k++)
                    {
                        result = axActProgType1.ReadDeviceBlock2(string.Join("\n", addrs[k]), 1, out short v);
                        values[k] = v;
                    }
                }
                else
                {
                    result = axActProgType1.ReadDeviceRandom2(string.Join("\n", addrs), countByAddr, out values[0]);
                }

                // 组装响应报文
                List<byte> respBytes = reqBytes.GetRange(0, 7);
                respBytes[0] += 0x80;

                // 数据部分
                if (reqBytes[13] == 0x01)//按位读取
                {
                    List<byte> datas = new List<byte>();
                    for (int i = 0; i < count; i++)
                    {
                        if (new byte[] { 0x9C, 0x9D, 0x90 }.Contains(code)) //按位从位软元件区域读
                        {
                            datas.Add((byte)(values[i] > 0 ? 0x01 : 0x00));
                        }
                        else //按位从字软元件区域读
                        {
                            //对块存储区按位读取，每个字有16个位，计算当前读取到字的第几个位。每个字的低位和高位分别存储在不同的字节中
                            int byteIndex = i / 8;
                            int wordIndex = i / 16;
                            if (byteIndex >= datas.Count)
                            {
                                datas.Add(0x00);
                            }
                            if (byteIndex % 2 == 0) //低位
                            {
                                byte bit = (byte)(values[wordIndex] & (0x01 << (i % 16)));
                                datas[byteIndex] |= bit;
                            }
                            else //高位
                            {
                                byte bit = (byte)((values[wordIndex] & (0x01 << (i % 16))) >> 8);
                                datas[byteIndex] |= bit;
                            }
                        }
                    }
                    // 响应数据长度+状态
                    respBytes.Add((byte)((datas.Count + 2) % 256));
                    respBytes.Add((byte)((datas.Count + 2) / 256 % 256));
                    respBytes.Add(0x00);
                    respBytes.Add(0x00);
                    respBytes.AddRange(datas);
                }
                else if (reqBytes[13] == 0x00) //按字读取
                {
                    if (new byte[] { 0x9C, 0x9D, 0x90 }.Contains(code)) //按字从位软元件区域读
                    {
                        List<byte> datas = new List<byte>();
                        for (int i = 0; i < values.Length; i++)
                        {
                            short v = values[i];
                            datas.Add((byte)(values[i] % 256));
                            datas.Add((byte)(values[i] / 256 % 256));
                        }
                        // 响应数据长度+状态
                        respBytes.Add((byte)((datas.Count + 2) % 256));
                        respBytes.Add((byte)((datas.Count + 2) / 256 % 256));
                        respBytes.Add(0x00);
                        respBytes.Add(0x00);
                        respBytes.AddRange(datas);
                    }
                    else //按字从字软元件区域读
                    {
                        // 响应数据长度+状态
                        respBytes.Add((byte)((count * 2 + 2) % 256));
                        respBytes.Add((byte)((count * 2 + 2) / 256 % 256));
                        respBytes.Add(0x00);
                        respBytes.Add(0x00);
                        for (int i = 0; i < values.Length; i++)
                        {
                            respBytes.Add((byte)(values[i] % 256));
                            respBytes.Add((byte)(values[i] / 256 % 256));
                        }
                    }
                }
                else
                {
                    respBytes.Add(0x02);
                    respBytes.Add(0x00);

                    respBytes.Add(0xC0);
                    respBytes.Add(0x50);
                    Console.WriteLine("-- 异常：0x" + result.ToString("X8"));
                }
                Console.WriteLine($">> [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] 响应:" + string.Join(" ", respBytes.Select(b => b.ToString("X2"))));
                clientSocket.Send(respBytes.ToArray());
            }
            // 成批写入
            // 按字从D区写入N个字，1个字1个字地址，datas字节数组存储N个字的数据
            // 按字从Y区写入N个字，1个字16个位地址，datas字节数组存储N*16个状态数据（客户端可以传入任意数量的状态位，服务端自动补0）
            // 注意：目前按字从Y区读有问题，但是按字从Y区写没有问题，可能是MXComponent的限制，暂时无解
            // 按位从Y区写入N个位，1个位1个位地址，datas字节数组存储N个状态位的数据
            // 按位从D区写入N个位，16个位1个字地址，datas字节数组存储N个表示状态位的的字节数据
            else if (reqBytes[11] == 0x01 && reqBytes[12] == 0x14)
            {
                // 软元件编号
                uint addr = BitConverter.ToUInt32(new byte[] { reqBytes[15], reqBytes[16], reqBytes[17], 0x00 }, 0);
                // 软元件代码
                byte code = reqBytes[18];
                string area_str = Area.FirstOrDefault(a => a.Item1 == code).Item2;
                // 写入点数
                ushort count = BitConverter.ToUInt16(new byte[] { reqBytes[19], reqBytes[20] }, 0);

                List<string> addrList = new List<string>();
                for (int i = 0; i < count; i++)
                {
                    if (reqBytes[13] == 0x01)//位
                    {
                        if (new byte[] { 0x9C, 0x9D, 0x90 }.Contains(code)) //按位从位软元件区域写
                        {
                            if (new byte[] { 0x9C, 0x9D }.Contains(code))
                            {
                                if (radix == 16)
                                {
                                    addr = Convert.ToUInt32(reqBytes[17].ToString("X2") + reqBytes[16].ToString("X2") + reqBytes[15].ToString("X2"), 16);
                                    addrList.Add(area_str + (addr + i).ToString("X"));
                                }
                                else if (radix == 8)
                                    addrList.Add(area_str + Convert.ToString(addr + i, 8)); //10进制转8进制
                            }
                            else
                            {
                                addrList.Add(area_str + (addr + i).ToString("X"));
                            }
                        }
                        else //按位从字软元件区域写
                        {
                            if (i % 16 == 0)
                            {
                                addrList.Add(area_str + (addr + i / 16).ToString());
                            }
                        }
                    }
                    else if (reqBytes[13] == 0x00)// 字
                    {
                        if (new byte[] { 0x9C, 0x9D, 0x90 }.Contains(code)) //按字从位软元件区域写
                        {
                            if (new byte[] { 0x9C, 0x9D }.Contains(code))
                            {
                                if (radix == 16)
                                {
                                    addr = Convert.ToUInt32(reqBytes[17].ToString("X2") + reqBytes[16].ToString("X2") + reqBytes[15].ToString("X2"), 16);
                                    for (int j = 0; j < 16; j++)
                                    {
                                        addrList.Add(area_str + (addr + i * 16 + j).ToString("X"));
                                    }
                                }
                                else if (radix == 8)
                                {
                                    for (int j = 0; j < 16; j++)
                                    {
                                        addrList.Add(area_str + Convert.ToString(addr + i * 16 + j, 8)); //10进制转8进制
                                    }
                                }
                            }
                            else
                            {
                                for (int j = 0; j < 16; j++)
                                {
                                    addrList.Add(area_str + (addr + i * 16 + j).ToString("X"));
                                }
                            }
                        }
                        else //按字从字软元件区域写
                        {
                            addrList.Add(area_str + (addr + i).ToString());
                        }
                    }
                }
                string[] addrs = addrList.ToArray();
                Console.WriteLine(">> 请求地址:" + string.Join(" ", addrs));

                // 写入数据
                short[] datas = new short[addrs.Length];
                int result = 0;

                int countByAddress = count; //从当前区域写入的地址数
                if (reqBytes[13] == 0x01)//按位写
                {
                    if (new byte[] { 0x9C, 0x9D, 0x90 }.Contains(code) == false) //按位从字软元件区域写
                    {
                        countByAddress = (count % 16 == 0) ? count / 16 : count / 16 + 1;
                    }
                }
                else if (reqBytes[13] == 0x00)// 按字写
                {
                    if (new byte[] { 0x9C, 0x9D, 0x90 }.Contains(code)) //按字从位软元件区域写
                    {
                        countByAddress = count * 16;
                    }
                }
                //位软元件区域的一个字就是一个状态位
                int index = 21, bit_offset = 0;
                for (int i = 0; i < countByAddress; i++)
                {
                    if (reqBytes[13] == 0x01)//位
                    {
                        if (new byte[] { 0x9C, 0x9D, 0x90 }.Contains(code)) //按位从位软元件区域写
                        {
                            byte bit = reqBytes[21 + i];
                            datas[i] = (short)(bit > 0 ? 0x01 : 0x00);
                        }
                        else //按位从字软元件区域写
                        {
                            byte byte1 = reqBytes[21 + i * 2];
                            byte byte2 = reqBytes[21 + i * 2 + 1];
                            short word = (short)(byte1 + (byte2 << 8));
                            datas[i] = word;
                        }
                    }
                    else if (reqBytes[13] == 0x00)// 字
                    {
                        if (new byte[] { 0x9C, 0x9D, 0x90 }.Contains(code)) //按字从位软元件区域写
                        {
                            datas[i] = (short)((reqBytes[index] & (1 << bit_offset)) > 0 ? 0x01 : 0x00);
                            bit_offset++;
                            if (bit_offset % 8 == 0)
                            {
                                bit_offset = 0;
                                index++;
                            }
                        }
                        else //按字从字软元件区域写
                        {
                            List<byte> wordBytes = reqBytes.GetRange(index, 2);
                            datas[i] = (BitConverter.ToInt16(wordBytes.ToArray(), 0));
                            index += 2;
                        }
                    }
                    result = axActProgType1.SetDevice2(addrs[i], datas[i]);
                    Console.Write($"{addrs[i]} - {datas[i]}\t");
                }
                Console.WriteLine("");
                List<byte> respBytes = reqBytes.GetRange(0, 7);
                respBytes[0] += 0x80;
                respBytes.Add(0x02);
                respBytes.Add(0x00);
                if (result == 0)
                {
                    respBytes.Add(0x00);
                    respBytes.Add(0x00);
                }
                else
                {
                    respBytes.Add(0xC0);
                    respBytes.Add(0x50);
                    Console.WriteLine("-- 异常：0x" + result.ToString("X8"));
                }

                Console.WriteLine($">> [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] 响应:" + string.Join(" ", respBytes.Select(b => b.ToString("X2"))));
                clientSocket.Send(respBytes.ToArray());
            }
            // 随机读取
            else if (reqBytes[11] == 0x03 && reqBytes[12] == 0x04)
            {
                // 只有字操作
                // 字点数
                byte wCount = reqBytes[15];
                // 双字点数
                byte dwCount = reqBytes[16];


                int index = 17;
                List<string> addrs = new List<string>();
                for (int i = 0; i < wCount; i++)
                {
                    uint addr = BitConverter.ToUInt32(new byte[] { reqBytes[index], reqBytes[index + 1], reqBytes[index + 2], 0x00 }, 0);
                    // 软元件代码
                    byte code = reqBytes[index + 3];
                    string area_str = Area.FirstOrDefault(a => a.Item1 == code).Item2;
                    if (new byte[] { 0x9C, 0x9D }.Contains(code))
                    {
                        if (radix == 16)
                        {
                            addr = Convert.ToUInt32(reqBytes[index + 2].ToString("X2") + reqBytes[index + 1].ToString("X2") + reqBytes[index].ToString("X2"), 16);
                            addrs.Add(area_str + addr.ToString("X"));
                        }
                        else if (radix == 8)
                            //addrs.Add(area_str + ((addr % 8) + (addr / 8 * 10)).ToString());
                            addrs.Add(area_str + Convert.ToString(addr, 8)); //10进制转8进制
                    }
                    else
                    {
                        string aStr = Area.FirstOrDefault(a => a.Item1 == code).Item2 + addr.ToString();
                        addrs.Add(aStr);
                    }
                    index += 4;
                }

                for (int i = 0; i < dwCount; i++)
                {
                    // 软元件编号
                    uint addr = BitConverter.ToUInt32(new byte[] { reqBytes[index], reqBytes[index + 1], reqBytes[index + 2], 0x00 }, 0);
                    // 软元件代码
                    byte code = reqBytes[index + 3];

                    string aStr = Area.FirstOrDefault(a => a.Item1 == code).Item2;
                    if (new byte[] { 0x9C, 0x9D }.Contains(code))
                    {
                        addr = Convert.ToUInt32(reqBytes[index + 2].ToString("X2") + reqBytes[index + 1].ToString("X2") + reqBytes[index].ToString("X2"), 16);
                        addrs.Add(aStr + addr.ToString());
                        addrs.Add(aStr + (addr + radix).ToString("X"));
                    }
                    else
                    {
                        addrs.Add(aStr + addr.ToString());
                        if (code == 0x90)
                            addr += 16;
                        else
                            addr += 1;
                        addrs.Add(aStr + addr.ToString());
                    }
                    index += 4;
                }

                Console.WriteLine(">> 请求地址:" + string.Join(" ", addrs));

                int count = wCount + dwCount * 2;
                short[] values = new short[count];
                //int result = axActProgType1.ReadDeviceRandom2(string.Join("\n", addrs), count, out values[0]);
                int result = 0;// axActProgType1.ReadDeviceBlock2(string.Join("\n", addrs), count, out values[0]);
                for (int i = 0; i < addrs.Count; i++)
                {
                    //X,Y软元件区域的地址必须按照16的倍数进行指定，即可以指定000,020,而不能是010
                    result = axActProgType1.ReadDeviceBlock2(addrs[i], 1, out short v);
                    values[i] = v;
                }

                List<byte> respBytes = reqBytes.GetRange(0, 7);
                respBytes[0] += 0x80;

                ushort len = (ushort)(count * 2 + 2);
                if (result == 0)
                {
                    respBytes.AddRange(BitConverter.GetBytes(len));

                    respBytes.Add(0x00);
                    respBytes.Add(0x00);

                    for (int i = 0; i < count; i++)
                    {
                        respBytes.AddRange(BitConverter.GetBytes(values[i]));
                    }
                }
                else
                {
                    respBytes.Add(0x02);
                    respBytes.Add(0x00);

                    respBytes.Add(0xC0);
                    respBytes.Add(0x50);
                    Console.WriteLine("-- 异常：0x" + result.ToString("X8"));
                }
                Console.WriteLine($">> [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] 响应:" + string.Join(" ", respBytes.Select(b => b.ToString("X2"))));
                clientSocket.Send(respBytes.ToArray());

            }
            // 随机写入
            else if (reqBytes[11] == 0x02 && reqBytes[12] == 0x14)
            {
                int bit_count = reqBytes[15];
                int w_count = reqBytes[15];
                int dw_count = reqBytes[16];
                int result = 0;
                if (reqBytes[13] == 0x01)// 位
                {
                    int index = 16;
                    for (int i = 0; i < bit_count; i++)
                    {
                        uint addr = BitConverter.ToUInt32(new byte[] { reqBytes[index], reqBytes[index + 1], reqBytes[index + 2], 0x00 }, 0);
                        // 软元件代码
                        byte code = reqBytes[index + 3];

                        string area_str = Area.FirstOrDefault(a => a.Item1 == code).Item2;
                        if (new byte[] { 0x9C, 0x9D }.Contains(code))
                        {
                            if (radix == 16)
                            {
                                addr = Convert.ToUInt32(reqBytes[index + 2].ToString("X2") + reqBytes[index + 1].ToString("X2") + reqBytes[index].ToString("X2"), 16);
                                area_str += addr.ToString("X");
                            }
                            else if (radix == 8)
                                //area_str += ((addr % 8) + (addr / 8 * 10)).ToString();
                                area_str += Convert.ToString(addr, 8); //10进制转8进制
                        }
                        else
                        {
                            area_str = Area.FirstOrDefault(a => a.Item1 == code).Item2 + addr.ToString();
                        }
                        short v = reqBytes[index + 4];
                        result = axActProgType1.SetDevice2(area_str, v);
                        Console.WriteLine($"{area_str}->{v}");

                        index += 5;
                    }
                }
                else//字
                {
                    int index = 17;
                    // 写入wCount个字
                    for (int k = 0; k < w_count; k++)
                    {
                        // 软元件编号
                        uint addr = BitConverter.ToUInt32(new byte[] { reqBytes[index], reqBytes[index + 1], reqBytes[index + 2], 0x00 }, 0);
                        // 软元件代码
                        byte code = reqBytes[index + 3];
                        string area_str = Area.FirstOrDefault(a => a.Item1 == code).Item2;
                        if (new byte[] { 0x9C, 0x9D }.Contains(code))
                        {
                            if (radix == 16)
                            {
                                addr = Convert.ToUInt32(reqBytes[index + 2].ToString("X2") + reqBytes[index + 1].ToString("X2") + reqBytes[index].ToString("X2"), 16);
                                area_str += addr.ToString("X");
                            }
                            else if (radix == 8)
                                //area_str += ((addr % 8) + (addr / 8 * 10)).ToString();
                                area_str += Convert.ToString(addr, 8); //10进制转8进制
                        }
                        else
                        {
                            area_str = Area.FirstOrDefault(a => a.Item1 == code).Item2 + addr.ToString();
                        }
                        index += 4;
                        // 数据
                        short v = BitConverter.ToInt16(reqBytes.GetRange(index, 2).ToArray(), 0);
                        index += 2;


                        //result = axActProgType1.WriteDeviceRandom2(area_str, 1, ref v);
                        if (new byte[] { 0x9C, 0x9D, 0x90 }.Contains(code))
                            area_str = "K4" + area_str;
                        result = axActProgType1.SetDevice2(area_str, v);
                        Console.WriteLine(area_str + "->" + v + "  " + result.ToString("X"));
                    }
                    // 写dwCount个双字
                    for (int i = 0; i < dw_count; i++)
                    {
                        // 软元件编号
                        uint addr = BitConverter.ToUInt32(new byte[] { reqBytes[index], reqBytes[index + 1], reqBytes[index + 2], 0x00 }, 0);
                        // 软元件代码
                        byte code = reqBytes[index + 3];
                        string aStr = Area.FirstOrDefault(a => a.Item1 == code).Item2;
                        string area_temp = "";
                        if (new byte[] { 0x9C, 0x9D }.Contains(code))
                        {
                            addr = Convert.ToUInt32(reqBytes[index + 2].ToString("X2") + reqBytes[index + 1].ToString("X2") + reqBytes[index].ToString("X2"), 16);
                            area_temp += "K4" + aStr + addr.ToString("X");
                            area_temp += "\n" + "K4" + aStr + (addr + radix).ToString("X");
                        }
                        else if (code == 0x90)
                        {
                            area_temp = "K4" + aStr + addr.ToString();
                            addr += 16;
                            area_temp = "\n" + "K4" + aStr + addr.ToString();
                        }
                        else
                        {
                            area_temp += aStr + addr.ToString();
                            addr += 1;
                            area_temp += "\n" + aStr + addr.ToString();
                        }
                        index += 4;
                        // 数据
                        short[] v = new short[2];
                        v[0] = BitConverter.ToInt16(reqBytes.GetRange(index, 2).ToArray(), 0);
                        index += 2;

                        v[1] = BitConverter.ToInt16(reqBytes.GetRange(index, 2).ToArray(), 0);
                        index += 2;

                        result = axActProgType1.WriteDeviceRandom2(area_temp, 2, ref v[0]);
                        Console.WriteLine(area_temp + "->" + string.Join(" ", v) + "  " + result.ToString("X"));
                    }
                }

                List<byte> respBytes = reqBytes.GetRange(0, 7);
                respBytes[0] += 0x80;
                respBytes.Add(0x02);
                respBytes.Add(0x00);
                if (result == 0)
                {
                    respBytes.Add(0x00);
                    respBytes.Add(0x00);
                }
                else
                {
                    respBytes.Add(0xC0);
                    respBytes.Add(0x50);
                    Console.WriteLine("-- 异常：0x" + result.ToString("X8"));
                }
                Console.WriteLine($">> [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] 请求:" + string.Join(" ", respBytes.Select(b => b.ToString("X2"))));
                clientSocket.Send(respBytes.ToArray());
            }
            // 多块成批读取
            else if (reqBytes[11] == 0x06 && reqBytes[12] == 0x04)
            {
                // 字点数
                byte wCount = reqBytes[15];
                // 位点数
                byte bCount = reqBytes[16];

                int index = 17;
                List<byte> dataBytes = new List<byte>();
                // 字处理
                for (int k = 0; k < wCount; k++)
                {
                    // 软元件编号
                    uint addr = BitConverter.ToUInt32(new byte[] { reqBytes[index++], reqBytes[index++], reqBytes[index++], 0x00 }, 0);
                    // 软元件代码
                    byte code = reqBytes[index++];
                    string aStr = Area.FirstOrDefault(a => a.Item1 == code).Item2;
                    ushort count = BitConverter.ToUInt16(new byte[] { reqBytes[index++], reqBytes[index++] }, 0);

                    short[] values = new short[count];

                    string[] addrs = new string[count];
                    for (int i = 0; i < count; i++)
                    {
                        addrs[i] = aStr + (addr + i).ToString();
                    }

                    Console.WriteLine(">> 请求地址:" + string.Join(" ", addrs));
                    int result = axActProgType1.ReadDeviceRandom2(string.Join("\n", addrs), count, out values[0]);
                    if (result == 0)
                    {
                        foreach (var item in values)
                        {
                            dataBytes.AddRange(BitConverter.GetBytes(item));
                        }
                    }
                    else
                    {
                        Console.WriteLine("-- 异常：0x" + result.ToString("X8"));
                    }
                }
                // 位处理
                for (int k = 0; k < bCount; k++)
                {
                    // 软元件编号
                    uint addr = BitConverter.ToUInt32(new byte[] { reqBytes[index++], reqBytes[index++], reqBytes[index++], 0x00 }, 0);
                    // 软元件代码
                    byte code = reqBytes[index++];
                    string aStr = Area.FirstOrDefault(a => a.Item1 == code).Item2;
                    ushort count = BitConverter.ToUInt16(new byte[] { reqBytes[index++], reqBytes[index++] }, 0);

                    short[] values = new short[count];

                    string[] addrs = new string[count];
                    for (int i = 0; i < count; i++)
                    {
                        if (new byte[] { 0x9C, 0x9D }.Contains(code))
                        {
                            if (radix == 16)
                                addrs[i] = aStr + (addr + i).ToString("X");
                            else if (radix == 8)
                                addrs[i] = aStr + Convert.ToString(addr + i, 8); //10进制转8进制
                        }
                        else
                            addrs[i] = aStr + (addr + i).ToString();
                    }

                    Console.WriteLine(">> 请求地址:" + string.Join(" ", addrs));
                    int result = axActProgType1.ReadDeviceRandom2(string.Join("\n", addrs), count, out values[0]);

                    if (result == 0)
                    {
                        for (int i = 0; i < count; i++)
                        {
                            if (i % 8 == 0)
                            {
                                byte b = 0x00;
                                dataBytes.Add(b);
                            }
                            dataBytes[dataBytes.Count - 1] |= (byte)(values[i] << (i % 8));
                        }
                    }
                    else
                    {
                        Console.WriteLine("-- 异常：0x" + result.ToString("X8"));
                    }
                }

                // 
                List<byte> respBytes = reqBytes.GetRange(0, 7);
                respBytes[0] += 0x80;
                respBytes.AddRange(BitConverter.GetBytes((ushort)(dataBytes.Count + 2)));

                respBytes.Add(0x00);
                respBytes.Add(0x00);

                respBytes.AddRange(dataBytes);

                Console.WriteLine($">> [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] 响应:" + string.Join(" ", respBytes.Select(b => b.ToString("X2"))));
                clientSocket.Send(respBytes.ToArray());
            }
            // 多块成批写入
            else if (reqBytes[11] == 0x06 && reqBytes[12] == 0x14)
            {
                // 字点数
                byte wCount = reqBytes[15];
                // 位点数
                byte bCount = reqBytes[16];

                int index = 17;
                int result = 0;
                for (int i = 0; i < wCount; i++)
                {
                    // 软元件编号
                    uint addr = BitConverter.ToUInt32(new byte[] { reqBytes[index], reqBytes[index + 1], reqBytes[index + 2], 0x00 }, 0);
                    // 软元件代码
                    byte code = reqBytes[index + 3];
                    string aStr = Area.FirstOrDefault(a => a.Item1 == code).Item2;
                    string area_temp = "";
                    ushort count = BitConverter.ToUInt16(new byte[] { reqBytes[index + 4], reqBytes[index + 5] }, 0);
                    index += 6;
                    for (int j = 0; j < count; j++)
                    {
                        var v = BitConverter.ToInt16(reqBytes.GetRange(index, 2).ToArray(), 0);
                        area_temp = aStr + (addr + j).ToString();
                        result = axActProgType1.WriteDeviceRandom2(area_temp, 1, ref v);
                        Console.WriteLine($"{area_temp} -> {v}");
                        index += 2;
                    }
                    //index += count;
                }
                for (int i = 0; i < bCount; i++)
                {
                    // 软元件代码
                    byte code = reqBytes[index + 3];
                    string aStr = Area.FirstOrDefault(a => a.Item1 == code).Item2;
                    string area_temp = "";
                    ushort count = BitConverter.ToUInt16(new byte[] { reqBytes[index + 4], reqBytes[index + 5] }, 0);
                    int bit_offset = 0, byte_offset = 0;
                    for (int j = 0; j < count; j++)
                    {
                        // 软元件编号
                        uint addr = BitConverter.ToUInt32(new byte[] { reqBytes[index], reqBytes[index + 1], reqBytes[index + 2], 0x00 }, 0);
                        if (new byte[] { 0x9C, 0x9D }.Contains(code))
                        {
                            if (radix == 16)
                            {
                                addr = Convert.ToUInt32(reqBytes[index + 2].ToString("X2") + reqBytes[index + 1].ToString("X2") + reqBytes[index].ToString("X2"), 16);
                                area_temp = aStr + (addr + j).ToString("X");
                            }
                            else if (radix == 8)
                                area_temp = aStr + Convert.ToString(addr + j, 8); //10进制转8进制
                        }
                        else
                            area_temp = aStr + (addr + j).ToString();

                        var v = (reqBytes[index + 6 + byte_offset] & (1 << bit_offset)) > 0 ? 1 : 0;
                        Console.WriteLine($"{area_temp} -> {v}");
                        axActProgType1.SetDevice2(area_temp, (short)v);
                        bit_offset++;
                        if (bit_offset % 8 == 0)
                        {
                            bit_offset = 0;
                            byte_offset++;
                        }
                    }
                    index += (6 + (int)Math.Ceiling((double)count / 8));
                }

                List<byte> respBytes = reqBytes.GetRange(0, 7);
                respBytes[0] += 0x80;
                respBytes.Add(0x02);
                respBytes.Add(0x00);
                if (result == 0)
                {
                    respBytes.Add(0x00);
                    respBytes.Add(0x00);
                }
                else
                {
                    respBytes.Add(0xC0);
                    respBytes.Add(0x50);
                    Console.WriteLine("-- 异常：0x" + result.ToString("X8"));
                }
                Console.WriteLine($">> [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] 请求:" + string.Join(" ", respBytes.Select(b => b.ToString("X2"))));
                clientSocket.Send(respBytes.ToArray());
            }
            // 远程RUN
            else if (reqBytes[11] == 0x01 && reqBytes[12] == 0x10)
            {
                int code = axActProgType1.SetCpuStatus(0);

                List<byte> respBytes = reqBytes.GetRange(0, 7);
                respBytes[0] += 0x80;
                respBytes.Add(0x02);
                respBytes.Add(0x00);

                respBytes.Add(0x00);
                respBytes.Add(0x00);

                Console.WriteLine($">> [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] 请求:" + string.Join(" ", respBytes.Select(b => b.ToString("X2"))));
                clientSocket.Send(respBytes.ToArray());
            }
            // 远程STOP
            else if (reqBytes[11] == 0x02 && reqBytes[12] == 0x10)
            {
                int code = axActProgType1.SetCpuStatus(1);

                List<byte> respBytes = reqBytes.GetRange(0, 7);
                respBytes[0] += 0x80;
                respBytes.Add(0x02);
                respBytes.Add(0x00);

                respBytes.Add(0x00);
                respBytes.Add(0x00);

                Console.WriteLine($">> [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] 请求:" + string.Join(" ", respBytes.Select(b => b.ToString("X2"))));
                clientSocket.Send(respBytes.ToArray());
            }
            // CPU Type
            else if (reqBytes[11] == 0x01 && reqBytes[12] == 0x01)
            {
                int result = axActProgType1.GetCpuType(out string name, out int code);

                List<byte> respBytes = reqBytes.GetRange(0, 7);
                respBytes[0] += 0x80;
                respBytes.Add(0x14);
                respBytes.Add(0x00);

                respBytes.Add(0x00);
                respBytes.Add(0x00);

                if (name.Length < 16)
                    name.PadRight(16 - name.Length, (char)0x20);
                respBytes.AddRange(Encoding.Default.GetBytes(name));
                respBytes.Add((byte)(code % 256));
                respBytes.Add((byte)(code / 256 % 256));

                Console.WriteLine($">> [{DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")}] 请求:" + string.Join(" ", respBytes.Select(b => b.ToString("X2"))));
                clientSocket.Send(respBytes.ToArray());
            }
        }

        /// <summary>
        /// 接收
        /// </summary>
        /// <param name="socket">socket</param>
        /// <returns></returns>
        protected List<byte> Qna3EReceive(Socket socket)
        {
            List<byte> bytes = new List<byte>();
            byte[] respBytes = new byte[9];
            var len = socket.Receive(respBytes, 0, 9, SocketFlags.None);
            if (len == 0)
            {
                isTcpConnected = false;
                if (socket.Connected) socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                throw new Exception("连接断开");
            }
            bytes.AddRange(respBytes);

            ushort dataLen = BitConverter.ToUInt16(new byte[] { respBytes[7], respBytes[8] }, 0);
            respBytes = new byte[dataLen];
            len = socket.Receive(respBytes, 0, dataLen, SocketFlags.None);
            if (len == 0)
            {
                isTcpConnected = false;
                if (socket.Connected) socket.Shutdown(SocketShutdown.Both);
                socket.Close();
                throw new Exception("连接断开");
            }
            bytes.AddRange(respBytes);

            return bytes;
        }

    }
}
