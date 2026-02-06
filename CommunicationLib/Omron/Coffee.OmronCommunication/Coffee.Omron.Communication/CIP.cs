using Coffee.Omron.Communication.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.Omron.Communication
{
    /// <summary>
    /// 基于Ethernet/IP的CIP协议通信对象
    /// </summary>
    public class CIP : OmronBase
    {
        byte _slot = 0x00;
        byte[] _session_Handle;
        SocketSender sender = null;

        public CIP(string host, int port, byte slot = 0x00)
        {
            _slot = slot;
            sender = new SocketSender(host, port);
        }

        byte[] flag = Encoding.ASCII.GetBytes("coffee\t\r");
        public override void Open(int timeout = 3000)
        {
            if (sender != null)
            {
                sender.ResponseTimeOut = timeout;
                sender.Open();

                //创建CIP Session，请求SessionHandle
                //注意：CIP所有报文按小端处理
                byte[] bytes = new byte[] {
                    0x65,0x00, //命令码 表示注册Session
                    0x04,0x00, //封装标头后续字节数（命令特定数据部分的字节数）
                    0x00,0x00,0x00,0x00, //Session Handle
                    0x00,0x00,0x00,0x00, //状态码
                    flag[0],flag[1],flag[2],flag[3],flag[4],flag[5],flag[6],flag[7],// 命令发送者附加数据，任何值
                    0x00,0x00,0x00,0x00, //选项，未启用 默认4个0

                    // 命令特定数据
                    0x01,0x00, //协议版本 固定0x01
                    0x00,0x00  //选项，特定用户 0~7位 保留使用 8~15 未来扩展
                };
                byte[] resp = sender.SendAndReceive(bytes);

                this.CheckResponse(resp);

                //从请求会话的响应报文中获取SessionHandle
                _session_Handle = resp.Skip(4).Take(4).ToArray();
            }
        }

        public override void Close()
        {
            sender?.Close();
        }


        protected override void CheckResponse(byte[] bytes)
        {
            //从响应报文中获取CIP Header状态码
            //因为前3个字节都为0，只有最后一个字节有值，所以取这四个字节的和就是最后一个字节的值
            byte header_code = (byte)(bytes.Skip(8).Take(4).ToList().Sum(b => b));
            if (header_code != 0x00) //请求成功状态码为0x00
            {
                if (CIP_Errors.HeaderErrors.ContainsKey(header_code))
                    throw new Exception(CIP_Errors.HeaderErrors[header_code]);
                else
                    throw new Exception($"未知异常: {header_code}");
            }

            // 这个针对读写的时候进行结果状态判断
            //即使请求会话的响应报文没有返回这么多字节，也不会有问题。因为SendAndReceive一次读取1024个字节，即使没有足够的数据，也会自动填充0x00。这个时候，bytes[42]和bytes[43]就是0x0000
            string resp_status = bytes[43].ToString("X2") + bytes[42].ToString("X2");
            if (resp_status != "0000")
            {
                if (CIP_Errors.RespErrors.ContainsKey(resp_status))
                    throw new Exception(CIP_Errors.RespErrors[resp_status]);
                else
                    throw new Exception($"未知异常: {resp_status}");
            }
        }


        public byte[] Read(string tag)
        {
            List<byte> bytes = new List<byte>{
                0x6F,0x00, //命令码 表示SendRRData
                0x2E,0x00, //封装标头后续字节数（命令数据部分字节数，动态赋值sp_len）
                _session_Handle[0],_session_Handle[1],_session_Handle[2],_session_Handle[3],// Session ID
                0x00,0x00,0x00,0x00, //Header状态码
                flag[0],flag[1],flag[2],flag[3],flag[4],flag[5],flag[6],flag[7],// 命令发送者附加数据，任何值
                0x00,0x00,0x00,0x00,// 选项信息

                //命令特定数据部分
                0x00,0x00,0x00,0x00, //Interface handle 固定4个0
                0x01,0x00,  //超时时间

                0x02,0x00, //下面有2个Item Address Item 和 DataItem
                // Address Item
                0x00,0x00, //封装的项类型
                0x00,0x00, //当前Item项中后续数据字节数，当前无
                // Data item
                0xB2,0x00,  //封装的项类型 Unconnected Message
                0x1E,0x00,  //当前Item项中后续数据字节数（动态赋值item_len）

                //CIP指令
                0x52, //服务默认0x52
                0x02, //请求路径大小，即多少个字
                0x20,0x06,0x24,0x01,  //请求路径，类ID + 实例ID
                0x0A,0x00, //默认超时时间

                0x10,0x00, //从服务标识到服务命令指定数据的长度（动态赋值cmd_len）

                0x4C, //服务标识 Read Tag Sercice （读指令）
                0x00, //标签后续字数（动态赋值addr_len）， 从下个字节到标签完的字数 （扩展符号 + 标签字节数 + 标签字节数据）
            };

            int tag_len = tag.Length;
            List<byte> tag_bytes = new List<byte>()
            {
                0x91, //扩展符号，默认为0x91，表示使用SYM
                (byte)tag_len //标签字符数量
            };
            //标签字符字节
            //当标签字节数为单数时，后面需补0x00。因为前面指定的是标签后续字数，所以字节数必须是偶数
            tag_bytes.AddRange(Encoding.UTF8.GetBytes(tag));
            if (tag_len % 2 > 0)
                tag_bytes.Add(0x00);

            // 相关长度计算
            int addr_len = tag_bytes.Count / 2; //标签后续字数（扩展符号 + 标签字节数 + 标签字节数据）
            int cmd_len = tag_bytes.Count + 4; //从服务标识到服务命令指定数据的长度（+4表示需加上服务标识，标签后续字数及服务命令指定数据）
            int item_len = cmd_len + 14; //当前Item项中后续数据字节数（+14表示需加上剩余的最后4字节及从范围在(Item项后续字节数 ~ 从服务标识到服务命令指定数据的长度]）
            int sp_len = item_len + 16; //命令数据部分字节数（+16表示需加上从命令特定数据开始到Item项后续数据字节数这16个字节）

            // 相关长度信息填充
            bytes[2] = (byte)(sp_len % 256);
            bytes[3] = (byte)(sp_len / 256 % 256);

            bytes[38] = (byte)(item_len % 256);
            bytes[39] = (byte)(item_len / 256 % 256);

            bytes[48] = (byte)(cmd_len % 256);
            bytes[49] = (byte)(cmd_len / 256 % 256);

            bytes[51] = (byte)addr_len;

            // 拼接Tag地址信息
            bytes.AddRange(tag_bytes);

            // 接尾
            bytes.AddRange(new byte[] { 0x01, 0x00 }); //服务命令指定数据，默认值
            bytes.AddRange(new byte[] { 0x01, 0x00 }); //默认值

            bytes.Add(0x01); 
            bytes.Add(_slot); //最后字节0x00 可能会填充PLC的插槽号

            byte[] resp = sender.SendAndReceive(bytes.ToArray());

            // 校验
            this.CheckResponse(resp);

            // 数据解析
            // 通过bytes[44]取数据类型 Data Type Code (in hex)
            string datatypeCode = resp[44].ToString("X2");
            int byte_len = -1; //当前数据类型的位数
            if (CIP_TypeCode.TypeLength.ContainsKey(datatypeCode))
                byte_len = CIP_TypeCode.TypeLength[datatypeCode];

            if (byte_len == -1) return null;

            //当数据类型是字符串时，对应的位数是0
            //这时，结果数据先存储字符个数（2字节），再存储具体字符的值
            if (byte_len == 0)
            {
                int str_len = resp[46] + resp[47] * 256; //小端处理
                return resp.Skip(48).Take(str_len).ToArray(); //返回具体的字符串
            }

            byte_len /= 8;// 当前数据类型的字节数 = 当前数据类型的位数 / 8

            byte[] data_bytes = resp.Skip(46).Take(byte_len).ToArray(); //返回具体的数据
            Array.Reverse(data_bytes); //小端转大端处理
            return data_bytes;
        }


        public void Write(string tag, CIP_DataTypes dataType, byte[] data)
        {
            List<byte> bytes = new List<byte>{
                0x6F,0x00, //命令码 表示SendRRData
                0x2E,0x00, //封装标头后续字节数（命令数据部分字节数，动态赋值sp_len）
                _session_Handle[0],_session_Handle[1],_session_Handle[2],_session_Handle[3],// Session ID
                0x00,0x00,0x00,0x00, //Header状态码
                flag[0],flag[1],flag[2],flag[3],flag[4],flag[5],flag[6],flag[7],// 命令发送者附加数据，任何值
                0x00,0x00,0x00,0x00,// 选项信息

                //命令特定数据部分
                0x00,0x00,0x00,0x00, //Interface handle 固定4个0
                0x01,0x00,  //超时时间

                0x02,0x00, //下面有2个Item Address Item 和 DataItem
                // Address Item
                0x00,0x00, //封装的项类型
                0x00,0x00, //当前Item项中后续数据字节数，当前无
                // Data item
                0xB2,0x00,  //封装的项类型 Unconnected Message
                0x1E,0x00,  //当前Item项中后续数据字节数（动态赋值item_len）

                //CIP指令
                0x52, //服务默认0x52
                0x02, //请求路径大小，即多少个字
                0x20,0x06,0x24,0x01,  //请求路径，类ID + 实例ID 
                0x0A,0x00, //默认超时时间

                0x10,0x00, //从服务标识到服务命令指定数据的长度（动态赋值cmd_len）

                0x4D, //服务标识 （写指令）
                0x00, //标签后续字数（动态赋值addr_len）， 从下个字节到标签完的字数 （扩展符号 + 标签字节数 + 标签字节数据）
            };
            int tag_len = tag.Length;
            List<byte> tag_bytes = new List<byte>()
            {
                0x91, //扩展符号，默认为0x91，表示使用SYM
                (byte)tag_len //标签字符数量
            };
            //标签字符字节
            //当标签字节数为单数时，后面需补0x00。因为前面指定的是标签后续字数，所以字节数必须是偶数
            tag_bytes.AddRange(Encoding.UTF8.GetBytes(tag));
            if (tag_len % 2 > 0)
                tag_bytes.Add(0x00);

            int addr_len = tag_bytes.Count / 2; //标签后续字数（扩展符号 + 标签字节数 + 标签字节数据）

            tag_bytes.AddRange(new byte[] { (byte)dataType, 0x00 }); //写入数据类型（2字节）

            tag_bytes.AddRange(new byte[] { 0x01, 0x00 } ); //写入数量，默认为一个

            if (dataType != CIP_DataTypes.STRING)
            {
                Array.Reverse(data);
            }
            else //如果写入的是字符串，则需先写入字符的个数，再写入具体的字符串
            {
                byte[] len_bytes = BitConverter.GetBytes((ushort)data.Length);
                if (!BitConverter.IsLittleEndian) //保证数据的字节序是小端处理
                    Array.Reverse(len_bytes);
                tag_bytes.AddRange(len_bytes);
            }

            tag_bytes.AddRange(data);
            //CIP写数据时，传入的字节数组必须是偶数，否则会抛异常。所以当写入的数据类型是8位时，就是单字节，需要填充一个0x00的字节
            if (dataType == CIP_DataTypes.BOOL ||
            dataType == CIP_DataTypes.SINT ||
                dataType == CIP_DataTypes.USINT ||
                dataType == CIP_DataTypes.WORD)
                tag_bytes.Add(0x00);


            int cmd_len = tag_bytes.Count + 2; //从服务标识到服务命令指定数据的长度（+2表示需加上服务标识，标签后续字数）
            int item_len = cmd_len + 14; //当前Item项中后续数据字节数（+14表示需加上剩余的最后4字节及从范围在(Item项后续字节数 ~ 从服务标识到服务命令指定数据的长度]）
            int sp_len = item_len + 16; //命令数据部分字节数（+16表示需加上从命令特定数据开始到Item项后续数据字节数这16个字节）

            // 相关长度信息填充
            bytes[2] = (byte)(sp_len % 256);
            bytes[3] = (byte)(sp_len / 256 % 256);

            bytes[38] = (byte)(item_len % 256);
            bytes[39] = (byte)(item_len / 256 % 256);

            bytes[48] = (byte)(cmd_len % 256);
            bytes[49] = (byte)(cmd_len / 256 % 256);

            bytes[51] = (byte)addr_len;

            // 拼接Tag地址信息
            bytes.AddRange(tag_bytes);

            // 接尾
            bytes.AddRange(new byte[] { 0x01, 0x00 }); //默认值

            bytes.Add(0x01);
            bytes.Add(_slot); //最后字节0x00 可能会填充PLC的插槽号

            byte[] resp = sender.SendAndReceive(bytes.ToArray());

            // 校验/检查
            // 状态
            this.CheckResponse(resp);
        }


        // 针对NJ/NX系列PLC的标签读取，当前环境无法测试
        // 后续结合实际情况自行进行测试
        // 如果有问题，可以自行调度，如果处理不了的   可以进行沟通
        public void MultipleRead(params CIP_Parameter[] tags)
        {
            List<byte> bytes = new List<byte>{
                0x6F,0x00, //命令码 表示SendRRData
                0x2E,0x00, //封装标头后续字节数（命令数据部分字节数，动态赋值sp_len）
                _session_Handle[0],_session_Handle[1],_session_Handle[2],_session_Handle[3],// Session ID
                0x00,0x00,0x00,0x00, //Header状态码
                flag[0],flag[1],flag[2],flag[3],flag[4],flag[5],flag[6],flag[7],// 命令发送者附加数据，任何值
                0x00,0x00,0x00,0x00, //选项信息

                //命令特定数据部分
                0x00,0x00,0x00,0x00, //Interface handle 固定4个0
                0x01,0x00,  //超时时间

                0x02,0x00, //下面有2个Item Address Item 和 DataItem
                // Address Item
                0x00,0x00, //封装的项类型
                0x00,0x00, //当前Item项中后续数据字节数，当前无
                // Data item
                0xB2,0x00,  //封装的项类型 Unconnected Message
                0x1E,0x00,  //当前Item项中后续数据字节数（动态赋值item_len）

                //CIP指令
                0x52, //服务默认0x52
                0x02, //请求路径大小，即多少个字
                0x20,0x06,0x24,0x01,  //请求路径，类ID + 实例ID 
                0x0A,0x00, //默认超时时间

                0x10,0x00, //从服务标识到服务命令指定数据的长度（动态赋值cmd_len）

                0x0A, //Multiple_Service_Packet 标记【附录十一】
                0x02, //请求路径大小--字数  Word
                0x20,0x02,0x24,0x01, //请求路径，类ID  实例 ID等

                //请求的标签数 Request Data Start
                // 第一个标签的偏移量（每个偏移量都是相对于前一个标签的偏移量）
                // 第 ... 个标签的偏移量
            };
            // 标签数
            bytes.Add((byte)(tags.Length % 256));
            bytes.Add((byte)(tags.Length / 256 % 256));

            int tag1_start = tags.Length * 2 + 2; //第一个标签的偏移量 = 2个字节标签数 + N个标签起始地址所占字节（每个地址2个字节）
            int tagN_start = tag1_start; //第i个标签的偏移量

            List<byte> _tagBytes = new List<byte>();
            for (int i = 0; i < tags.Length; i++)
            {
                //第i个标签地址
                bytes.Add((byte)(tagN_start % 256));
                bytes.Add((byte)(tagN_start / 256 % 256));
                
                _tagBytes.Add(0x4C); //服务标识 Read Tag Sercice （读指令）
                //标签后续字数（扩展符号 + 标签字节数 + 标签字节数据）
                _tagBytes.Add((byte)Math.Ceiling((tags[i].Tag.Length + 2) * 1.0 / 2));
                _tagBytes.Add(0x91); //扩展符号，默认为0x91，表示使用SYM
                _tagBytes.Add((byte)tags[i].Tag.Length); //标签字符数量
                //标签数据。
                //当标签字节数为单数时，后面需补0x00。因为前面指定的是标签后续字数，所以字节数必须是偶数
                _tagBytes.AddRange(Encoding.UTF8.GetBytes(tags[i].Tag));
                if (tags[i].Tag.Length % 2 > 0)
                    _tagBytes.Add(0x00);

                _tagBytes.AddRange(new byte[] { 0x01, 0x00 }); //服务命令指定数据，默认值

                tagN_start = tag1_start + _tagBytes.Count;
            }
            bytes.AddRange(_tagBytes);

            bytes.Add(0x01);
            bytes.Add(0x00);
            bytes.Add(0x01);
            bytes.Add(_slot);

            byte[] resp = sender.SendAndReceive(bytes.ToArray());

            this.CheckResponse(resp);
            
            int start_index = 46; //从bytes[46]开始取数据，即从【项数】后读数据。bytes[45]*256+bytes[44]是项数，即总共有几项数据要读取
            int temp = 0;
            foreach (var tag in tags)
            {
                //获取第N个标签的数据的偏移量，即从【项数】后开始计算偏移量，直到第N个标签的结果状态位置
                byte[] offset = resp.Skip(start_index + temp).Take(2).ToArray();
                //因为start_index就是【项数】后的哪个位置，也就是偏移量计算的起始位置，这样完美地找到第N个标签的结果状态位置
                int start = offset[1] * 256 + offset[0] + start_index;

                //获取第N个标签的读取结果状态
                string resp_status = bytes[start].ToString("X2") + bytes[start + 1].ToString("X2");
                if (resp_status != "0000")
                {
                    if (CIP_Errors.RespErrors.ContainsKey(resp_status))
                        tag.Error = new Exception(CIP_Errors.RespErrors[resp_status]);
                    else
                        tag.Error = new Exception($"未知异常: {resp_status}");
                }

                start += 2; //跳过2个字节的结果状态，开始读数据类型Data Type Code (in hex)

                string datatypeCode = resp[start].ToString("X2");
                int byte_len = -1; //当前数据类型的位数
                if (CIP_TypeCode.TypeLength.ContainsKey(datatypeCode))
                    byte_len = CIP_TypeCode.TypeLength[datatypeCode];

                if (byte_len == -1)
                    tag.Error = new Exception("未知数据类型");

                start += 2;  //跳过2个字节的数据类型，开始读数据
                //当数据类型是字符串时，对应的位数是0
                //这时，结果数据先存储字符个数（2字节），再存储具体字符的值
                if (byte_len == 0)
                {
                    int str_len = resp[start] + resp[start + 1] * 256; //小端处理
                    tag.Data = resp.Skip(start + 2).Take(str_len).ToArray(); //读取具体的字符串
                }

                byte_len /= 8; // 当前数据类型的字节数 = 当前数据类型的位数 / 8

                byte[] data_bytes = resp.Skip(start).Take(byte_len).ToArray(); //读取具体的值数据
                Array.Reverse(data_bytes); //小端转大端处理

                tag.Data = data_bytes;

                temp += 2; //定位到下一个标签（每个标签起始地址占2个字节）
            }
        }
    }
}
