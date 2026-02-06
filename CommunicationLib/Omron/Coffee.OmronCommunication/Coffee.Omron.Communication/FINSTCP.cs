using Coffee.Omron.Communication.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.Omron.Communication
{
    public class FINSTCP : FinsCommand
    {
        int pc_node, plc_node;
        SocketSender socketSender = null;

        public FINSTCP(string host, int port, int timeout = 3000)
        {
            socketSender = new SocketSender(host, port);
            socketSender.ResponseTimeOut = timeout;
        }

        public override void Open(int timeout = 3000)
        {
            socketSender.ResponseTimeOut = timeout;
            socketSender.Open();

            string ip = socketSender.IP.ToString();
            pc_node = byte.Parse(ip.Split('.')[3]);

            // 请求连接  通信请求报文
            byte[] bytes = new byte[] {
                0x46,0x49,0x4E,0x53, //FINS
                0x00,0x00,0x00,0x0C, // 后续字节数
                // Command - 表示客户端向服务端发送的连接请求
                0x00,0x00,0x00,0x00,
                0x00,0x00,0x00,0x00, // 异常码--
                0x00,0x00,0x00,(byte)pc_node //Client node addr 只有最低位有值，表示本机IP地址末位
            };

            byte[] resp = socketSender.SendAndReceive(bytes);

            //响应报文的前4个字节必须是FINS字符
            if (!resp.Take(4).SequenceEqual(new byte[] { 0x46, 0x49, 0x4E, 0x53 }))
                throw new Exception("返回字节数据异常");

            //取得异常码，异常码占4个字节，只有最低位有值，其他位始终为零
            byte[] status = resp.Skip(12).Take(4).ToArray();
            if (status[3] > 0)
                throw new Exception(FINS_TcpHeaderErrors.Errors[status[3]]);

            plc_node = resp[23]; //通信请求响应报文中的最后一个字节返回服务器IP地址末位
        }
        public override void Close()
        {
            socketSender.Close();
        }

        //服务ID。传输给一标识。00到FF之间的任意数字
        //响应报文会返回相同的服务ID，用于确定请求报文和响应报文是同一组请求
        int sid = 0;

        public byte[] Read(Area area,
             int wordAddr, byte bitAddr = 0, ushort count = 1,
             DataTypes dataType = DataTypes.WORD)
        {
            //在按位处理的基础上最高位置1，就是按字处理
            byte a = (byte)area;
            if (dataType == DataTypes.WORD)
            {
                a += 0x80;
            }

            sid = (sid + 1) % 0xFF; //确保服务ID只占一个字节

            byte[] bytes = new byte[] {
                // ------    FINSTCP Header    --------
                0x46,0x49,0x4E,0x53, // FINS
                0x00,0x00,0x00,0x1A, // 后续字节数
                // Command - 表示客户端向服务端发送的连接请求
                0x00,0x00,0x00,0x02,
                // 异常码--
                0x00,0x00,0x00,0x00,

                // ------    FINS Header    --------
                0x80, //ICF
                0x00, //Rsv 预留，默认0x00
                0x02, //GCT 网关数量，一般为0x02
                0x00, //DNA 目标网络地址
                (byte)pc_node, //DA1 目标的IP地址末位（PLC）
                0x00, //DA2 目标单位地址
                0x00, //SNA 源网络地址
                (byte)plc_node, //SA1 源的IP地址末位（PC)
                0x00, //SA2 源单位地址
                (byte)sid ,// SID 服务ID。传输给一标识。00到FF之间的任意数字

                0x01,0x01,// Command 读取存储区数据
                (byte)a, //Area 请求操作的存储区
                (byte)(wordAddr/256%256),(byte)(wordAddr%256),// 起始地址(WORD)，100 大端处理
                bitAddr,// 位地址，10
                (byte)(count/256%256),(byte)(count%256)// 读取数量 大端处理
            };

            byte[] resp = socketSender.SendAndReceive(bytes);

            this.CheckResponse(resp);
            
            //一个位数据占一个字节，一个字数据占2个字节
            int len = count;
            if (dataType == DataTypes.WORD)
                len = count * 2;
            byte[] data_bytes = resp.Skip(30).Take(len).ToArray();

            return data_bytes;
        }

        public void Write(byte[] dataBytes, Area area, int wordAddr,
            byte bitAddr = 0, DataTypes dataType = DataTypes.WORD)
        {
            int count = dataBytes.Length;
            byte a = (byte)area;
            if (dataType == DataTypes.WORD)
            {
                a += 0x80;
                count = dataBytes.Length / 2;
            }

            sid = (sid + 1) % 0xFF;
            //后续字节数 = 写入数据字节数 + 26
            int len = dataBytes.Length + 0x1A;

            List<byte> bytes = new List<byte> {
                // ------    FINSTCP Header    --------
                0x46,0x49,0x4E,0x53, // FINS
                // 后续字节数 大端处理
                (byte)(len/256/256/256%256),
                (byte)(len/256/256%256),
                (byte)(len/256%256),
                (byte)(len%256),
                // Command - 表示客户端向服务端发送的连接请求
                0x00,0x00,0x00,0x02,
                // 异常码--
                0x00,0x00,0x00,0x00,

                // ------    FINS Header    --------
                0x80, //ICF
                0x00, //Rsv 预留，默认0x00
                0x02, //GCT 网关数量，一般为0x02
                0x00, //DNA 目标网络地址
                (byte)pc_node, //DA1 目标的IP地址末位（PLC）
                0x00, //DA2 目标单位地址
                0x00, //SNA 源网络地址
                (byte)plc_node, //SA1 源的IP地址末位（PC)
                0x00, //SA2 源单位地址
                (byte)sid , // SID 服务ID。传输给一标识。00到FF之间的任意数字

                0x01,0x02, //Command 内存写入
                (byte)a, //Area 请求操作的存储区
                (byte)(wordAddr/256%256),(byte)(wordAddr%256),// 起始地址，100
                bitAddr,// 位地址，10
                (byte)(count/256%256),(byte)(count%256)// 写入个数
            };
            bytes.AddRange(dataBytes);

            byte[] resp = socketSender.SendAndReceive(bytes.ToArray());

            this.CheckResponse(resp);
        }

        public void MultipleRead(params FINS_Parameter[] parameters)
        {
            List<byte> bytes = new List<byte>()
            {
                0x46,0x49,0x4E,0x53, // FINS
                0x00,0x00,0x00,0x00, // 后续字节数
                // Command - 表示客户端向服务端发送的连接请求
                0x00,0x00,0x00,0x02,
                // 异常码--
                0x00,0x00,0x00,0x00,

                // ------    FINS Header    --------
                0x80,
                0x00,
                0x02,
                0x00,(byte)pc_node,0x00,
                0x00,(byte)plc_node,0x00,
                (byte)sid ,// SID

                0x01,0x04, //Command 读取多块数据，每个块只能读一个长度
            };

            // 后续字节的长度是剔除FINS和Length后计算的，所以要减8
            int len = bytes.Count() - 8;
            foreach (var item in parameters)
            {
                //在按位处理的基础上最高位置1，就是按字处理
                byte a = (byte)item.Area;
                if (item.DataType == DataTypes.WORD)
                {
                    a += 0x80;
                }
                bytes.Add(a);

                bytes.Add((byte)(item.WordAddr / 256 % 256)); //大端处理
                bytes.Add((byte)(item.WordAddr % 256));

                bytes.Add(item.BitAddr);

                len += 4; //每个读取块的请求报文段 区域代码 + 起始字地址 + 起始位地址
            }
            //更新后续字节数 大端处理
            bytes[4] = (byte)(len / 256 / 256 / 256 % 256);
            bytes[5] = (byte)(len / 256 / 256 % 256);
            bytes[6] = (byte)(len / 256 % 256);
            bytes[7] = (byte)(len % 256);

            byte[] resp = socketSender.SendAndReceive(bytes.ToArray());

            this.CheckResponse(resp);

            // 第30个字节是区域代码 后面跟着对应的数据
            int index = 31;
            foreach (var item in parameters)
            {
                int data_len = 1;
                if (item.DataType == DataTypes.WORD)
                    data_len = 2;

                item.Data = resp.Skip(index).Take(data_len).ToArray();
                index += data_len + 1; // + 1是因为要忽略掉区域代码
            }
        }

        public void PlcRun(RunModes mode = RunModes.RUN)
        {
            sid = (sid + 1) % 0xFF;
            byte[] bytes = new byte[] {
                0x46,0x49,0x4E,0x53, //FINS
                0x00,0x00,0x00,0x17, //后续字节数
                0x00,0x00,0x00,0x02, // Command - 表示客户端向服务端发送的连接请求
                0x00,0x00,0x00,0x00, //异常码

                // ------    FINS Header    --------
                0x80,
                0x00,
                0x02,
                0x00,(byte)pc_node,0x00,
                0x00,(byte)plc_node,0x00,
                (byte)sid , // SID

                0x04,0x01, //功能码 PLC启动
                0xFF,0xFF,
                (byte)mode //以监视模式还是运行模式启动
            };

            byte[] resp = socketSender.SendAndReceive(bytes);

            this.CheckResponse(resp);
        }

        public void PlcStop()
        {
            sid = (sid + 1) % 0xFF;
            byte[] bytes = new byte[] {
                0x46,0x49,0x4E,0x53, //FINS
                0x00,0x00,0x00,0x14, //后续字节数
                0x00,0x00,0x00,0x02, // Command - 表示客户端向服务端发送的连接请求
                0x00,0x00,0x00,0x00, //异常码

                // ------    FINS Header    --------
                0x80,
                0x00,
                0x02,
                0x00,(byte)pc_node,0x00,
                0x00,(byte)plc_node,0x00,
                (byte)sid , // SID

                0x04,0x02, //功能码 PLC停止
            };

            byte[] resp = socketSender.SendAndReceive(bytes);

            this.CheckResponse(resp);
        }

        public DateTime GetPlcClock()
        {
            sid = (sid + 1) % 0xFF;
            byte[] bytes = new byte[] {
                0x46,0x49,0x4E,0x53, //FINS
                0x00,0x00,0x00,0x14, //后续字节数
                0x00,0x00,0x00,0x02, // Command - 表示客户端向服务端发送的连接请求
                0x00,0x00,0x00,0x00, //异常码

                // ------    FINS Header    --------
                0x80,
                0x00,
                0x02,
                0x00,(byte)pc_node,0x00,
                0x00,(byte)plc_node,0x00,
                (byte)sid , // SID

                0x07,0x01, //功能码 读取日期时间
            };

            byte[] resp = socketSender.SendAndReceive(bytes);

            this.CheckResponse(resp);

            // 解析时间数据
            byte[] date_bytes = resp.Skip(30).Take(6).ToArray();

            // 年
            byte b_year = date_bytes[0];
            if (!int.TryParse("20" + b_year.ToString("X2"), out int year))
            {
                throw new Exception("时间转换失败");
            }
            // 月
            byte m_byte = date_bytes[1];
            if (!int.TryParse(m_byte.ToString("X2"), out int month))
            {
                throw new Exception("时间转换失败");
            }
            // 日
            byte dayByte = date_bytes[2];
            if (!int.TryParse(dayByte.ToString("X2"), out int day))
            {
                throw new Exception("时间转换失败");
            }
            // 时
            byte hourByte = date_bytes[3];
            if (!int.TryParse(hourByte.ToString("X2"), out int hour))
            {
                throw new Exception("时间转换失败");
            }
            // 分
            byte minuteByte = date_bytes[4];
            if (!int.TryParse(minuteByte.ToString("X2"), out int minute))
            {
                throw new Exception("时间转换失败");
            }
            // 秒
            byte secondByte = date_bytes[5];
            if (!int.TryParse(secondByte.ToString("X2"), out int second))
            {
                throw new Exception("时间转换失败");
            }

            DateTime dt = new DateTime(year, month, day, hour, minute, second);
            return dt;

        }

        public void SetPlcClock(DateTime time)
        {
            sid = (sid + 1) % 0xFF;
            byte[] bytes = new byte[] {
                0x46,0x49,0x4E,0x53, //FINS
                0x00,0x00,0x00,0x19, //后续字节数
                0x00,0x00,0x00,0x02, // Command - 表示客户端向服务端发送的连接请求
                0x00,0x00,0x00,0x00, //异常码

                // ------    FINS Header    --------
                0x80,
                0x00,
                0x02,
                0x00,(byte)pc_node,0x00,
                0x00,(byte)plc_node,0x00,
                (byte)sid , // SID

                0x07,0x02, //功能码 写入日期时间

                //写入的日期时间，在PLC中是16进制格式的年月日，如2024年，存储的是16进制的值为24的值，而不是10进制
                Convert.ToByte((time.Year-2000).ToString(), 16),
                Convert.ToByte(time.Month.ToString(), 16),
                Convert.ToByte(time.Day.ToString(), 16),
                Convert.ToByte(time.Hour.ToString(), 16),
                Convert.ToByte(time.Minute.ToString(), 16),
            };

            byte[] resp = socketSender.SendAndReceive(bytes);

            this.CheckResponse(resp);
        }

        protected override void CheckResponse(byte[] resp)
        {
            //确保响应报文是以FINS开头的
            if (!resp.Take(4).SequenceEqual(new byte[] { 0x46, 0x49, 0x4E, 0x53 }))
                throw new Exception("数据头不是FINS或ASCII格式");

            //忽略掉FINS、后续字节数、Command后，取错误代码
            byte[] h_status = resp.Skip(12).Take(4).ToArray();
            //通信报文头的错误代码占4个字节，但是始终只有最低位有值
            if (h_status[3] > 0)
                throw new Exception(FINS_TcpHeaderErrors.Errors[h_status[3]]);

            if (resp[25] != sid)
                throw new Exception("Seesion ID 不一致");

            //响应的异常码，即报文操作的异常码，而不是报文头异常码
            string end_status = resp[28].ToString("X2") + resp[29].ToString("X2");
            if (end_status != "0000")
            {
                if (FINS_ResponseErrors.Errors.ContainsKey(end_status))
                    throw new Exception(FINS_ResponseErrors.Errors[end_status]);
                else
                    throw new Exception("未知错误!");
            }
        }

        /// <summary>
        /// CIO0.15    CIO10
        /// IR10
        /// DR10
        /// TK10
        /// D100       D10.15
        /// H0.5       H10
        /// W0.5       W10
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public byte[] Read(string variable, ushort count)
        {
            FINS_Parameter finsParameter = this.GetAddress(variable);
            finsParameter.Count = count;
            return this.Read(finsParameter);
        }

        public byte[] Read(FINS_Parameter finsParameter)
        {
            return this.Read(finsParameter.Area, finsParameter.WordAddr,
                finsParameter.BitAddr,
                finsParameter.Count,
                finsParameter.DataType);
        }

        public void Write(string variable, byte[] dataBytes)
        {
            FINS_Parameter finsParameter = this.GetAddress(variable);
            finsParameter.Data = dataBytes;
            this.Write(finsParameter);
        }

        public void Write(FINS_Parameter finsParameter)
        {
            this.Write(finsParameter.Data, finsParameter.Area, finsParameter.WordAddr,
                finsParameter.BitAddr, finsParameter.DataType);
        }
    }
}
