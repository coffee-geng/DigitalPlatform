using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Coffee.Mitsubishi.Base;

namespace Coffee.Mitsubishi
{
    public class Mc3E
    {
        Socket socket = null;
        string _host;
        int _port;
        public Mc3E(string host, int port)
        {
            _host = host;
            _port = port;
            socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
        }

        public void Open(int timeout = 3000)
        {
            socket.ReceiveTimeout = timeout;
            socket.Connect(_host, _port);
        }
        public void Close()
        {
            if (socket == null) return;
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        // 批量读写，返回的数据是大端字节序。
        // 根据参数RequestType，可以按字或位读取数据。
        public byte[] Read(Areas area, string addr, ushort count,
            RequestType type = RequestType.WORD, bool isOctal = true)
        {
            byte[] addr_bytes = this.GetAddress(addr, area);

            byte[] bytes = new byte[] {
                0x50,0x00, //请求副头部，固定50 00 
                0x00, //网络号
                0xFF, //PLC编号，固定值
                0xFF,0x03, //目标模块IO编号，固定FF 03
                0x00, //目标模块站号

                // 剩余字节长度
                0x0C,0x00,  // 注意小端字节序

                0x10,0x00,  // PLC响应超时时间，以250ms为单位计算

                // 主指令
                0x01,0x04,
                // 子指令  0x0001按位操作 0x0000按字操作
                (byte)type,0x00,
                // 首地址
                addr_bytes[0],
                addr_bytes[1],
                addr_bytes[2],      
                // 软元件区域代码
                (byte)area,  
                // 读取长度
                (byte)(count % 256),
                (byte)(count / 256 % 256)
            };
            byte[] resp = this.SendAndReceive(bytes);

            // 检查状态码
            this.CheckResponse(resp);

            // 获取数据字节，进行返回
            // 按字取   字节数 = count * 2     位软元件    count*16位信息
            // 按位取   1个字节2个状态   字节数 = count / 2

            if (RequestType.WORD == type)
            {
                byte[] values_bytes = resp.Skip(11).Take(count * 2).ToArray();
                List<byte> data_values = new List<byte>();

                // 按字进行位软元件请求
                if (new Areas[] { Areas.X, Areas.Y, Areas.M }.Contains(area)) // 关于这个列表可以将所有位软元件编号都放进来
                {
                    foreach (byte value in values_bytes) //按字读取位软元件，XY是8进制数据存储
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            data_values.Add((byte)((value & (1 << i)) > 0 ? 0x01 : 0x00));
                        }
                    }
                }
                else// 按字进行字软元件请求
                {
                    for (int i = 0; i < count * 2; i += 2)
                    {
                        byte[] temp = values_bytes.Skip(i).Take(2).ToArray();
                        Array.Reverse(temp);
                        data_values.AddRange(temp);
                    }
                }
                return data_values.ToArray();
            }
            else if (RequestType.BIT == type)
            {
                // 0x11  0x01
                // 0x01 0x01 0x00 0x01
                byte[] state_bytes = resp.Skip(11).Take(count / 2 + (count % 2)).ToArray(); //要读取多少个字节
                byte[] state_value = new byte[count];
                int index = 0;
                for (int i = 0; i < count; i++) //三菱PLC采用小端字节序，而客户端一般使用大端字节序，所以需要颠倒字节序
                {
                    if (i % 2 == 0)
                        state_value[i] = (byte)((state_bytes[index] & (1 << 4)) > 0 ? 0x01 : 0x00);
                    else
                    {
                        state_value[i] = (byte)((state_bytes[index] & 1) > 0 ? 0x01 : 0x00);
                        index++;
                    }
                }

                return state_value;
            }
            else
                return Array.Empty<byte>();
        }

        public byte[] Read(string address, ushort count,
            RequestType type = RequestType.WORD, bool isOctal = true)
        {
            (Areas area, string start) = this.GetAddress(address);
            return this.Read(area, start, count, type, isOctal);
        }

        // datas:{0x01,0x00,0x00,0x01,0x01}
        // datas字节按大端处理
        // 根据参数RequestType，可以按字或位写入数据。
        public void Write(byte[] datas, Areas area, string addr,
            RequestType type = RequestType.WORD, bool isOctal = true)
        {
            int count = 0;
            // 数据处理
            List<byte> data_bytes = new List<byte>();
            if (type == RequestType.WORD)
            {
                // 这里只是部分位软元件，其他自行添加
                if (new Areas[] { Areas.X, Areas.Y, Areas.M }.Contains(area))
                {
                    count = (int)Math.Ceiling(datas.Length * 1.0 / 16); //当写入位数据时，参数data传入的每个字节可表示一个布尔值。count表示写入多少个字

                    //因为按字处理，即使只传入4个布尔值，占半个字节，实际上也要用一个字（即2个字节）存储
                    int byte_count = (int)(count * 2); //总共用多少个字节存储数据。例如：如果传入18个布尔值，则byte_count为4，其中前3个字节存储数据，最后一个字节空着
                    byte[] bytes = new byte[byte_count];
                    int index = -1;
                    for (int i = 0; i < datas.Length; i++)
                    {
                        if (i % 8 == 0)
                            index++;

                        byte current = 0x00;
                        if (datas[i] == 0x01)
                            current = (byte)(current | (1 << (i % 8)));

                        bytes[index] |= current;
                    }
                    data_bytes = new List<byte>(bytes);
                }
                else
                {
                    count = datas.Length / 2; //写入多少个字
                    for (int i = 0; i < datas.Length; i += 2)
                    {
                        //默认参数传入的是大端字节序，所以要颠倒
                        data_bytes.Add(datas[i + 1]);
                        data_bytes.Add(datas[i]);
                    }
                }
            }
            else if (type == RequestType.BIT) //只支持按位写布尔值数据，而不能写整数等
            {
                count = datas.Length; //写入多少个布尔值

                byte[] temp = new byte[datas.Length / 2 + (datas.Length % 2)]; //一个字节可以存储2个布尔值，必须保证是偶数，即如果9个布尔值存5个字节
                int index = 0;
                for (int i = 0; i < datas.Length; i++)
                {
                    //传入的布尔值数据两两一组，为了使布尔值数据在内存中顺序存放，所以规定第一个数据放在字节的高位，第二个放地位
                    //例如：传入数据 [true, false, true, true, false, true, false, false, true]，则数据存储格式二进制为 0001 0000 0001 0001 0000 0001 0000 0000 0001
                    //转换为16进制为 0x10 0x11 0x01 0x00 0x10 （其中最后一个16进制数据因为只占半个字节，所以要补0）
                    byte d = 0x00;
                    if (i % 2 == 0 && datas[i] == 0x01)
                    {
                        d = (byte)(d | (1 << 4));
                        temp[index] |= d;
                    }
                    if (i % 2 != 0 && datas[i] == 0x01)
                    {
                        d = (byte)(d | 1);
                        temp[index] |= d;
                        index++;
                    }
                }
                data_bytes = new List<byte>(temp);
            }

            //  地址处理
            byte[] addr_bytes = this.GetAddress(addr, area);

            List<byte> req_bytes = new List<byte>{
                0x50,0x00, //请求副头部，固定50 00 
                0x00, //网络号
                0xFF, //PLC编号，固定值
                0xFF,0x03, //目标模块IO编号，固定FF 03
                0x00, //目标模块站号

                // 剩余字节长度
                0x12,0x00, // 注意小端字节序

                0x10,0x00, // PLC响应超时时间，以250ms为单位计算

                // 主指令
                0x01,0x14,
                // 子指令  0x0001按位操作 0x0000按字操作
                (byte)type,0x00,
                // 首地址
                addr_bytes[0],
                addr_bytes[1],
                addr_bytes[2],
                // 软元件区域代码
                (byte)area,
                // 写入数量
                (byte)(count % 256),
                (byte)(count / 256 % 256),
            };
            req_bytes.AddRange(data_bytes);
            int len = 12 + data_bytes.Count;
            req_bytes[7] = (byte)(len % 256);
            req_bytes[8] = (byte)(len / 256 % 256);

            byte[] resp_bytes = this.SendAndReceive(req_bytes.ToArray());

            // 检查状态码
            this.CheckResponse(resp_bytes);
        }

        public void Write(byte[] datas, string address, RequestType type = RequestType.WORD, bool isOctal = true)
        {
            (Areas area, string start) = this.GetAddress(address);
            this.Write(datas, area, start, type, isOctal);
        }

        // 随机读写。
        // 可以按字、双字读取数据，但不能按位读取。
        public void RandomRead(List<DataParameter> w_address, List<DataParameter> dw_address, bool isOctal = true)
        {
            List<byte> bytes = new List<byte>{
                0x50,0x00, //请求副头部，固定50 00
                0x00, //网络号
                0xFF, //PLC编号
                0xFF,0x03, //目标模块IO编号，固定FF 03
                0x00, //目标模块站号

                // 剩余字节长度
                0x0C,0x00,  // 注意小端字节序
                0x10,0x00,  // PLC响应超时时间，以250ms为单位计算

                0x03,0x04, //操作指令
                0x00,0x00, //子命令 0x0001按位操作 0x0000按字操作

                (byte)w_address.Count, //字操作点数
                (byte)dw_address.Count, //双字操作点数
            };

            foreach (var address in w_address)
            {
                byte[] addr_bytes = this.GetAddress(address.Address, address.Area, isOctal);
                // 拼接请求的地址
                bytes.AddRange(addr_bytes); //软元件编号
                bytes.Add((byte)address.Area); //软元件区域代码
            }
            foreach (var address in dw_address)
            {
                byte[] addr_bytes = this.GetAddress(address.Address, address.Area, isOctal);
                // 拼接请求的地址
                bytes.AddRange(addr_bytes);
                bytes.Add((byte)address.Area);
            }

            // 计算请求长度  字节数
            int len = 8 + (w_address.Count + dw_address.Count) * 4;
            bytes[7] = (byte)(len % 256);
            bytes[8] = (byte)(len / 256 % 256);

            byte[] resp = this.SendAndReceive(bytes.ToArray());
            // 检查响应状态
            this.CheckResponse(resp);

            // 数据字节整理
            int index = 11;
            foreach (var address in w_address)
            {
                if (address.Datas == null)
                    address.Datas = new List<byte>();

                byte[] values_bytes = resp.Skip(index).Take(2).ToArray();
                if (new Areas[] { Areas.X, Areas.Y, Areas.M }.Contains(address.Area))
                {
                    foreach (byte value in values_bytes) //字节的每一位都是一个布尔值
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            address.Datas.Add((byte)((value & (1 << i)) > 0 ? 0x01 : 0x00));
                        }
                    }
                }
                else
                {
                    Array.Reverse(values_bytes); //小端转大端
                    address.Datas.AddRange(values_bytes);
                }
                index += 2;
            }
            foreach (var address in dw_address)
            {
                if (address.Datas == null)
                    address.Datas = new List<byte>();

                byte[] values_bytes = resp.Skip(index).Take(4).ToArray();
                if (new Areas[] { Areas.X, Areas.Y, Areas.M }.Contains(address.Area))
                {
                    foreach (byte value in values_bytes) //字节的每一位都是一个布尔值
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            address.Datas.Add((byte)((value & (1 << i)) > 0 ? 0x01 : 0x00));
                        }
                    }
                }
                else
                {
                    Array.Reverse(values_bytes);
                    address.Datas.AddRange(values_bytes);
                }

                index += 4;
            }
        }

        // 按位进行写入
        public void RandomWriteBit(List<DataParameter> address, bool isOctal = true)
        {
            List<byte> bytes = new List<byte>{
                0x50,0x00, //请求副头部，固定50 00
                0x00, //网络号
                0xFF, //PLC编号
                0xFF,0x03, //目标模块IO编号，固定FF 03
                0x00, //目标模块站号

                // 剩余字节长度
                0x0C,0x00,  // 注意小端字节序
                0x10,0x00,  // PLC响应超时时间，以250ms为单位计算

                0x02,0x14, //操作指令
                0x01,0x00, //子命令 按位操作

                (byte)address.Count, //操作点数
            };
            //写入位数据（3字节的软元件编号 + 1字节的软元件区域代码）
            foreach (var item in address)
            {
                byte[] addr_bytes = this.GetAddress(item.Address, item.Area, isOctal);
                bytes.AddRange(addr_bytes);
                bytes.Add((byte)item.Area);
                //写入位值
                if (item.Datas == null || item.Datas.Count == 0)
                    throw new Exception("需要写入的数据有异常，无法写入");
                else
                {
                    if (item.Datas[0] > 0)
                        bytes.Add(0x01);
                    else
                        bytes.Add(0x00);
                }
            }
            int len = 7 + address.Count * 5; //软元件编号 + 软元件区域 + 写入位值
            bytes[7] = (byte)(len % 256);
            bytes[8] = (byte)(len / 256 % 256);

            byte[] resp = this.SendAndReceive(bytes.ToArray());

            this.CheckResponse(resp);
        }

        // 按字、双字进行写入
        public void RandomWrite(List<DataParameter> w_address, List<DataParameter> dw_address, bool isOctal = true)
        {
            List<byte> bytes = new List<byte>{
                0x50,0x00, //请求副头部，固定50 00
                0x00, //网络号
                0xFF, //PLC编号
                0xFF,0x03, //目标模块IO编号，固定FF 03
                0x00, //目标模块站号

                // 剩余字节长度
                0x0C,0x00,  // 注意小端字节序
                0x10,0x00,  // PLC响应超时时间，以250ms为单位计算

                0x02,0x14, //操作指令
                0x00,0x00, //子命令 按字操作

                (byte)w_address.Count, //操作点数 写入字个数
                (byte)dw_address.Count, //写入双字个数

            };
            foreach (var address in w_address)
            {
                byte[] addr_bytes = this.GetAddress(address.Address, address.Area, isOctal);
                // 拼接请求的地址
                bytes.AddRange(addr_bytes);
                bytes.Add((byte)address.Area);

                if (new Areas[] { Areas.X, Areas.Y, Areas.M }.Contains(address.Area))
                {
                    if (address.Datas == null || address.Datas.Count != 16)
                        throw new Exception("需要写入的数据有异常，无法写入");
                    byte[] data_bytes = new byte[2];
                    int index = 0;
                    // 将16个状态字节，转换成两个字节，一个字节8个位进行汇总
                    for (int i = 0; i < 16; i++)
                    {
                        if (i > 0 && i % 8 == 0) index++;
                        byte bit = (byte)(address.Datas[i] == 0x01 ? 0x01 : 0x00);
                        data_bytes[index] |= (byte)(bit << (i % 8));
                    }
                    bytes.AddRange(data_bytes); //写入字数据
                }
                else
                {
                    if (address.Datas == null || address.Datas.Count != 2)
                        throw new Exception("需要写入的数据有异常，无法写入");
                    bytes.AddRange(address.Datas);
                }
            }
            foreach (var address in dw_address)
            {
                byte[] addr_bytes = this.GetAddress(address.Address, address.Area);
                // 拼接请求的地址
                bytes.AddRange(addr_bytes);
                bytes.Add((byte)address.Area);


                if (new Areas[] { Areas.X, Areas.Y, Areas.M }.Contains(address.Area))
                {
                    if (address.Datas == null || address.Datas.Count != 32)
                        throw new Exception("需要写入的数据有异常，无法写入");
                    byte[] data_bytes = new byte[4];
                    int index = 0;
                    // 将32个状态字节，转换成四个字节，一个字节8个位进行汇总
                    for (int i = 0; i < 32; i++)
                    {
                        if (i > 0 && i % 8 == 0) index++;
                        byte bit = (byte)(address.Datas[i] == 0x01 ? 0x01 : 0x00);
                        data_bytes[index] |= (byte)(bit << (i % 8));
                    }
                    bytes.AddRange(data_bytes); //写入双字数据
                }
                else
                {
                    if (address.Datas == null || address.Datas.Count != 4)
                        throw new Exception("需要写入的数据有异常，无法写入");
                    bytes.AddRange(address.Datas);
                }
            }

            //软元件编号 + 软元件区域 + 写入字数据 = 6个字节
            //软元件编号 + 软元件区域 + 写入双字数据 = 8个字节
            int len = 8 + w_address.Count * 6 + dw_address.Count * 8;
            bytes[7] = (byte)(len % 256);
            bytes[8] = (byte)(len / 256 % 256);

            byte[] resp = this.SendAndReceive(bytes.ToArray());

            this.CheckResponse(resp);
        }

        // 多块批量读写   字、位
        // 只能按字读取数据，而不管读取的软元件区域是字地址还是位地址。
        // [暂时未测试，自行测试]
        public void MultiBlockRead(List<DataParameter> w_address, List<DataParameter> b_address)
        {
            List<byte> bytes = new List<byte>{
                0x50,0x00, //请求副头部，固定50 00
                0x00, //网络号
                0xFF, //PLC编号
                0xFF,0x03, //目标模块IO编号，固定FF 03
                0x00, //目标模块站号

                // 剩余字节长度
                0x0C,0x00,  // 注意小端字节序
                0x10,0x00,  // PLC响应超时时间，以250ms为单位计算

                0x06,0x04, //操作指令
                0x00,0x00, //子命令，只支持按字读取，固定0x00

                (byte)w_address.Count, //字操作点数，即要读取多少个字地址的块（可以是相同的软元件区域不同地址，也可以是不同的软元件区域）
                (byte)b_address.Count, //位操作点数，即要读取多少个位地址的块
            };

            foreach (var address in w_address)
            {
                if (new Areas[] { Areas.X, Areas.Y, Areas.M }.Contains(address.Area))
                {
                    throw new Exception("参数w_address只能从指定字地址软元件区域读取数据！");
                }
                byte[] addr_bytes = this.GetAddress(address.Address, address.Area);
                // 拼接请求的地址，软元件区域和当前地址读取的字数
                bytes.AddRange(addr_bytes);
                bytes.Add((byte)address.Area);

                bytes.Add((byte)(address.Count % 256));
                bytes.Add((byte)(address.Count / 256 % 256));
            }
            foreach (var address in b_address)
            {
                //参数b_address只能指定从位地址的软元件区域读取
                if (!new Areas[] { Areas.X, Areas.Y, Areas.M }.Contains(address.Area))
                {
                    throw new Exception("参数b_address只能从指定位地址软元件区域读取数据！");
                }
                byte[] addr_bytes = this.GetAddress(address.Address, address.Area);
                // 拼接请求的地址，软元件区域和当前地址读取的位数
                bytes.AddRange(addr_bytes);
                bytes.Add((byte)address.Area);

                bytes.Add((byte)(address.Count % 256));// 数量  表示多少字
                bytes.Add((byte)(address.Count / 256 % 256));
            }

            int len = 8 + (w_address.Count + b_address.Count) * 6; //软元件编号 + 软元件区域 + 写入字或位的数量 = 6个字节
            bytes[7] = (byte)(len % 256);
            bytes[8] = (byte)(len / 256 % 256);

            byte[] resp = this.SendAndReceive(bytes.ToArray());
            // 检查响应状态
            this.CheckResponse(resp);

            // 解析数据
            //先以字操作方式读取字地址数据数据，结果存放在参数w_address的Data属性中
            int index = 11;
            foreach (var address in w_address)
            {
                if (address.Datas == null)
                    address.Datas = new List<byte>();

                //address.Count 表示从指定地址读取多少个字
                int byte_count = address.Count * 2; //当读取某个字地址块的数据时，总共读取多少个字节
                byte[] values_bytes = resp.Skip(index).Take(byte_count).ToArray();

                for (int i = 0; i < values_bytes.Length; i += 2)
                {
                    address.Datas.Add(values_bytes[i + 1]); //颠倒，小端转大端
                    address.Datas.Add(values_bytes[i]);
                }
                index += byte_count;
            }
            
            foreach (var address in b_address)
            {
                if (address.Datas == null)
                    address.Datas = new List<byte>();

                //address.Count 表示从指定地址读取多少个字的位数据，也就是 16 * N 个位数据，每个位结果占用一个字节
                int byte_count = address.Count * 2;
                byte[] values_bytes = resp.Skip(index).Take(byte_count).ToArray();

                foreach (byte value in values_bytes) //从X、Y、M位区域读取时，读取的
                {
                    for (int i = 0; i < 8; i++)
                    {
                        address.Datas.Add((byte)((value & (1 << i)) > 0 ? 0x01 : 0x00));
                    }
                }
                index += byte_count;
            }
        }

        // [暂时未测试，自行测试]
        // 只能按字写入数据，而不管写入的软元件区域是字地址还是位地址。
        public void MultiBlockWrite(List<DataParameter> w_address, List<DataParameter> b_address)
        {
            List<byte> bytes = new List<byte> {
                // 头部信息
                0x50, 0x00, //请求副头部，固定50 00
                0x00, //网络号
                0xFF, //PLC编号
                0xFF, 0x03, //目标模块IO编号，固定FF 03
                0x00, //目标模块站号

                0x40, 0x00, // 注意小端字节序
                0x10, 0x00, // PLC响应超时时间，以250ms为单位计算

                0x06, 0x14, //操作指令
                0x00, 0x00, // 子命令，只支持按字写入，固定0x00

                (byte)w_address.Count, //字操作点数，即要写入多少个字地址的块（可以是相同的软元件区域不同地址，也可以是不同的软元件区域）
                (byte)b_address.Count, //位操作点数，即要写入多少个位地址的块
            };

            foreach (var address in w_address)
            {
                if (new Areas[] { Areas.X, Areas.Y, Areas.M }.Contains(address.Area))
                {
                    throw new Exception("参数w_address只能从指定字地址软元件区域写入数据！");
                }
                byte[] addr_bytes = this.GetAddress(address.Address, address.Area);
                // 拼接请求的地址，软元件区域和当前地址读取的字数
                bytes.AddRange(addr_bytes);
                bytes.Add((byte)address.Area);
                bytes.Add((byte)(address.Count)); //写入多少个字的数据

                if (address.Datas == null || address.Datas.Count == 0 || address.Datas.Count % 2 > 0) //因为是以字操作，写入数据的字节数必须是字的倍数
                    throw new Exception("数据准备有误，不符合字操作的格式");
                int byteSize = Marshal.SizeOf(address.Datas.First());
                if (byteSize != 2) //只支持2个字节的数据类型
                    throw new Exception("数据准备有误，不符合字操作的格式");

                //参数传入的大端处理的数据必须转换位小端处理
                List<byte> datas2 = new List<byte>();
                for (int i = 0; i < address.Datas.Count; i += byteSize)
                {
                    datas2.Add(address.Datas[i + 1]);
                    datas2.Add(address.Datas[i]);
                }
                bytes.AddRange(datas2.ToArray());
            }
            foreach (var address in b_address)
            {
                //参数b_address只能指定从位地址的软元件区域读取
                if (!new Areas[] { Areas.X, Areas.Y, Areas.M }.Contains(address.Area))
                {
                    throw new Exception("参数b_address只能从指定位地址软元件区域写入数据！");
                }
                byte[] addr_bytes = this.GetAddress(address.Address, address.Area);
                // 拼接请求的地址，软元件区域和当前地址读取的位数
                bytes.AddRange(addr_bytes);
                bytes.Add((byte)address.Area);
                bytes.Add((byte)(address.Count)); //写入多少个字的位数据，16位为一个字

                if (address.Datas == null || address.Datas.Count % 16 != 0)
                    throw new Exception("需要写入的数据有异常，无法写入！（提示：写入位状态的个数要符号字操作的格式，必须是16的整数倍。）");
                byte[] data_bytes = new byte[address.Datas.Count / 8];
                int index = 0;
                // 将16个位状态转换成两个字节
                for (int i = 0; i < address.Datas.Count; i++)
                {
                    if (i > 0 && i % 8 == 0) index++;
                    byte bit = (byte)(address.Datas[i] == 0x01 ? 0x01 : 0x00);
                    data_bytes[index] |= (byte)(bit << (i % 8));
                }
                bytes.AddRange(data_bytes);
            }

            int len = w_address.Union(b_address).Sum(a => a.Datas.Count / 2 + 5) + 8;
            bytes[7] = (byte)(len % 256);
            bytes[8] = (byte)(len / 256 % 256);

            byte[] resp = this.SendAndReceive(bytes.ToArray());
            // 检查响应状态
            this.CheckResponse(resp);
        }

        // 地址解析
        // 注意：在FX3U系列中，X、Y位软元件区是8进制的，即X0,X1...X7, X10 (X8是错误)
        //       在Q系列中，X、Y位软元件区是16进制的，即X0,X1...X7,..X9,XA...X1A都是合法的
        // M位软元件区是10进制的
        // D软元件区是按字存储，D0,D1...D9,D10,... 其中每个D存储去是16位
        public (Areas, string) GetAddress(string addr)
        {
            string prefix = addr.Substring(0, 2);
            if (Enum.TryParse<Areas>(prefix, out Areas area))
            {
                string start = addr.Substring(2);
                return (area, start);
            }
            prefix = addr.Substring(0, 1);
            if (Enum.TryParse<Areas>(prefix, out area))
            {
                string start = addr.Substring(1);

                return (area, start);
            }

            throw new Exception("地址有误，请确认");
        }

        // 数据转换
        // 从字节到数据的转换
        // 注意：此方法接收参数或返回值都是大端处理
        public List<T> GetDatas<T>(byte[] bytes)
        {
            List<T> datas = new List<T>();
            if (typeof(T) == typeof(bool))
            {
                //参数指定的每个字节存储一个布尔值
                foreach (byte b in bytes)
                {
                    dynamic d = (b == 0x01);
                    datas.Add(d);
                }
            }
            else if (typeof(T) == typeof(string))
            {
                dynamic d = Encoding.UTF8.GetString(bytes);
                datas.Add(d);
            }
            else
            {
                int size = Marshal.SizeOf<T>();
                //反射BitConverter，找到T对应的转换方法
                Type tBitConverter = typeof(BitConverter);
                MethodInfo[] mis = tBitConverter.GetMethods(BindingFlags.Public | BindingFlags.Static);
                if (mis.Count() <= 0)
                    return datas;
                MethodInfo mi = mis.FirstOrDefault(m => m.ReturnType == typeof(T) &&
                                            m.GetParameters().Count() == 2)!;

                for (int i = 0; i < bytes.Length; i += size)
                {
                    byte[] data_bytes = bytes.ToList().GetRange(i, size).ToArray();
                    //参数传入的数据默认是大端处理的，要转换成小端给BitConverter调用
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(data_bytes);
                    }
                    dynamic v = mi.Invoke(tBitConverter, new object[] { data_bytes, 0 })!;
                    datas.Add(v);
                }
            }

            return datas;
        }

        // 从数据到字节的转换
        // 注意：此方法接收参数或返回值都是大端处理
        public byte[] GetBytes<T>(params T[] values)
        {
            List<byte> bytes = new List<byte>();
            if (typeof(T) == typeof(bool))
            {
                //每个布尔值占据一个字节
                foreach (var v in values)
                {
                    bytes.Add((byte)(bool.Parse(v.ToString()) ? 0x01 : 0x00));
                }
            }
            else if (typeof(T) == typeof(string))
            {
                //多个字符串拼接成一个字符串，然后再转换成字节数组
                StringBuilder sb = new StringBuilder();
                foreach(var v in values)
                {
                    sb.Append(v as string);
                }
                byte[] str_bytes = Encoding.UTF8.GetBytes(sb.ToString());
                bytes.AddRange(str_bytes);
            }
            else
            {
                foreach (var v in values)
                {
                    dynamic d = v;
                    byte[] v_bytes = BitConverter.GetBytes(d);
                    //BitConverter返回的是小端处理，颠倒后返回大端处理的数据
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(v_bytes);
                    }
                    bytes.AddRange(v_bytes);
                }
            }

            return bytes.ToArray();
        }

        // PLC 启停
        public void PlcRun(ExecuteType et = ExecuteType.Normal, CleanMode cm = CleanMode.Normal)
        {
            byte[] bytes = new byte[] {
                // 头部
                0x50,0x00,
                0x00,
                0xFF,
                0xFF,0x03,
                0x00,

                // 剩余字节长度
                0x0A,0x00, // 注意小端字节序
                0x10,0x00, // PLC响应超时时间，以250ms为单位计算

                0x01,0x10, //操作指令
                0x00,0x00, //子命令
                (byte)et,0x00,//是否强制执行:0x01不强制执行   0x03强制执行
                (byte)cm, //清除模式 :00 不清空；01 清空锁存以外；02 清空全部
                0x00 //固定值
            };
            byte[] resp = this.SendAndReceive(bytes);

            this.CheckResponse(resp);
        }

        public void PlcStop(ExecuteType et = ExecuteType.Normal)
        {
            byte[] bytes = new byte[] {
                // 头部
                0x50,0x00,
                0x00,
                0xFF,
                0xFF,0x03,
                0x00,

                // 剩余字节长度
                0x08,0x00, // 注意小端字节序
                0x10,0x00, // PLC响应超时时间，以250ms为单位计算

                0x02,0x10, //操作指令
                0x00,0x00, //子命令
                (byte)et,0x00, // 是否强制执行:0x01不强制执行   0x03强制执行
            };

            byte[] resp = this.SendAndReceive(bytes);

            this.CheckResponse(resp);
        }

        private byte[] SendAndReceive(byte[] bytes)
        {
            socket.Send(bytes);

            byte[] resp = new byte[1024];
            socket.Receive(resp);

            return resp;
        }

        //将地址解析为字节数组。isOctal表示支持8进制还是16进制。
        //因为XY区域在FX3U中是8进制，但在Q系列是16进制。默认是8进制。
        //注意：返回的地址一定是3个字节的数组。
        private byte[] GetAddress(string address, Areas area, bool isOctal = true)
        {
            int addr = 0;
            int.TryParse(address, out addr);
            List<byte> addr_bytes = new List<byte>();
            if (new Areas[] { Areas.X, Areas.Y }.Contains(area))
            {
                string addr_str = address.PadLeft(6, '0');
                try
                {
                    byte[] _bytes = isOctal ? OctalConverter.OctalStringToByteArray(addr_str) : Convert.FromHexString(addr_str);
                    Array.Reverse(_bytes); //大端转小端
                    addr_bytes.AddRange(_bytes);
                }
                catch { throw new Exception("地址格式错误"); }
            }
            else //其他区域的地址是10进制格式（小端）
            {
                addr_bytes.Add((byte)(addr % 256));
                addr_bytes.Add((byte)(addr / 256 % 256));
                addr_bytes.Add((byte)(addr / 256 / 256 % 256));
            }

            if (addr_bytes.Count > 3)
                throw new Exception("地址超出范围");
            else if (addr_bytes.Count < 3)
            {
                while(addr_bytes.Count < 3)
                {
                    addr_bytes.Add((byte)0x00);
                }
            }

            return addr_bytes.ToArray();
        }

        private void CheckResponse(byte[] resp)
        {
            // 检查状态码，小端字节序
            string state = resp[10].ToString("X2") + resp[9].ToString("X2"); //结束代码，状态码
            if (state != "0000")
            {
                if (Status.Errors.ContainsKey(state))
                    throw new Exception(Status.Errors[state]);
                else
                    throw new Exception("未知错误");
            }
        }
    }
}
