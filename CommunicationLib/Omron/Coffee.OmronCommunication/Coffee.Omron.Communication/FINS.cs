using Coffee.Omron.Communication.Base;
using System.ComponentModel.DataAnnotations;
using System.IO.Ports;
using System.Text;

namespace Coffee.Omron.Communication
{
    public class FINS : FinsCommand
    {
        SerialPort SerialPort = null;

        public FINS(string portName, int baudRate, int dataBits, Parity parity, StopBits stopBits)
        {
            SerialPort = new SerialPort(portName, baudRate, parity, dataBits, stopBits);
        }

        public override void Open(int timeout = 3000)
        {
            if (SerialPort != null && !SerialPort.IsOpen)
            {
                SerialPort.ReadTimeout = timeout;
                SerialPort.Open();
            }
        }

        public override void Close()
        {
            if (SerialPort != null && SerialPort.IsOpen)
            {
                SerialPort.Close();
            }
        }

        // BIT count*2
        // WORD  count*4
        public byte[] Read(int unit, FINS_Parameter parameter)
        {
            return this.Read(unit, parameter.Area, parameter.WordAddr,
                parameter.BitAddr, parameter.Count, parameter.DataType);
        }

        public byte[] Read(int unit, Area area,
            int wordAddr, byte bitAddr = 0, ushort count = 1,
            DataTypes dataType = DataTypes.WORD)
        {
            if (SerialPort == null || !SerialPort.IsOpen) throw new Exception("串口未初始化");

            byte a = (byte)area;
            if (dataType == DataTypes.WORD) //当按位处理最高位置1，就是按字处理
            {
                a += 0x80;
            }
            //@ + 05（目标单元号 2个字符）+ FA（2个字符） + 响应等待时间 + ICF + DA2 + SA2 + SID + Command + Area + 起始字地址 + 起始位地址 + 读取个数 + FCS校验 + 结束符
            //ICF：Information Control Field 请求 0x80 响应 0xC0
            //DA2：0x00 00表示CPU单元
            //SA2：0x00 单元地址，串口时00表示CPU单元
            //SID：0x00 服务ID。传输给一标识。00到FF之间的任意数字
            //Command：功能码 0101 读取连续I/O内存区域数据
            string funcCode = "0101";
            string cmd = $"@{unit.ToString("00")}FA000000000" + $"{funcCode}" + $"{a.ToString("X2")}" +
                $"{wordAddr.ToString("X4")}{bitAddr.ToString("X2")}{count.ToString("X4")}";
            // 计算 FCS 校验
            var fcs = FCS(cmd);
            cmd += fcs;
            cmd += "*\r";

            byte[] req = Encoding.ASCII.GetBytes(cmd);

            byte[] resp = this.SendAndReceive(req);

            this.CheckResponse(resp);
            
            string resp_str = Encoding.Default.GetString(resp);
            int len = count * 2; //一个位占一个字节，每个字节是两个ASCII字符表示，所以*2
            if (dataType == DataTypes.WORD)
                len = count * 4; //一个字占2个字节，每个字节是两个ASCII字符表示，所以*4

            //串口通信是通过传递字符格式，而不是字节格式，一个字节2个字符（如果某个字节有对应的字符，如@，就占一个字符，否则占2个字符）
            //@ + 目标单元号XX(2) + FA(2) + 响应等待时间X(2) + ICF(2) + DA2 + SA2 + SID + Command(4) + 响应码(4) + Data(len) + FCS校验(2) + 结束符(2)
            string data_str = resp_str.Substring(23, len); //因为响应等待时间和ICF占2个字符，所以startOffset=23

            //  这个方法在.NET环境下有效，Framework环境下需要循环，每两个字符转一个字节
            byte[] data_bytes = Convert.FromHexString(data_str);

            return data_bytes;
        }

        //一次读多个存储区，每个位置只能读一个长度。
        public void MultipleRead(int unit, params FINS_Parameter[] parameters)
        {
            if (parameters.Length == 1)
                parameters[0].Data = this.Read(unit, parameters[0]);

            if (SerialPort == null || !SerialPort.IsOpen) throw new Exception("串口未初始化");

            string funcCode = "0104";
            string cmd = $"@{unit.ToString("00")}FA000000000{funcCode}";
            foreach (FINS_Parameter param in parameters)
            {
                byte a = (byte)param.Area;
                if (param.DataType == DataTypes.WORD)
                {
                    a += 0x80;
                }
                cmd += a.ToString("X2") +
                    $"{param.WordAddr.ToString("X4")}{param.BitAddr.ToString("X2")}"; //请求报文中不需要带上请求的长度，因为必须且只能读一个长度
            }
            // 计算 FCS 校验
            var fcs = FCS(cmd);
            cmd += fcs;
            cmd += "*\r";

            byte[] req = Encoding.ASCII.GetBytes(cmd);

            byte[] resp = this.SendAndReceive(req);

            this.CheckResponse(resp);

            // 数据的解析
            // 000082 xx xx  B0  xx xx 
            string resp_str = Encoding.Default.GetString(resp);
            int index = 25; //Read方法的响应报文是从23开始存储响应数据，而MultipleRead方法是从25开始解析响应数据，因为每个响应数据前面有2个字符长度的区域代码
            foreach (FINS_Parameter param in parameters)
            {
                int len = 2;
                if (param.DataType == DataTypes.WORD)
                    len = 4;
                string data_str = resp_str.Substring(index, len);

                param.Data = Convert.FromHexString(data_str);

                index += len + 2; //+2是因为每个响应数据前面有2个字符长度的区域代码，需过滤掉
            }
        }

        public void Write(byte[] dataBytes, int unit, Area area,
            int wordAddr, byte bitAddr = 0,
            DataTypes dataType = DataTypes.WORD)
        {
            if (SerialPort == null || !SerialPort.IsOpen) throw new Exception("串口未初始化");

            byte a = (byte)area;
            int count = dataBytes.Length;
            if (dataType == DataTypes.WORD)
            {
                a += 0x80;

                if (dataBytes.Length % 2 > 0)
                    throw new Exception("需要写入的数据字节长度不正确");

                count = dataBytes.Length / 2;
            }
            string cmd = $"@{unit.ToString("00")}FA0000000000102{a.ToString("X2")}" +
                $"{wordAddr.ToString("X4")}{bitAddr.ToString("X2")}{count.ToString("X4")}";
            // 加入数据字节
            cmd += string.Join("", dataBytes.Select(b => b.ToString("X2")));

            // 计算 FCS 校验
            var fcs = FCS(cmd);
            cmd += fcs;
            cmd += "*\r";

            byte[] req = Encoding.ASCII.GetBytes(cmd);
            byte[] resp = this.SendAndReceive(req);
            this.CheckResponse(resp);
        }


        public void PlcRun(int unit, RunModes mode = RunModes.RUN)
        {
            if (SerialPort == null || !SerialPort.IsOpen) throw new Exception("串口未初始化");

            string cmd = $"@{unit.ToString("00")}FA0000000000401FFFF";
            cmd += ((byte)mode).ToString("X2");

            // 计算 FCS 校验
            var fcs = FCS(cmd);
            cmd += fcs;
            cmd += "*\r";

            byte[] req = Encoding.ASCII.GetBytes(cmd);

            byte[] resp = this.SendAndReceive(req);

            this.CheckResponse(resp);
        }

        public void PlcStop(int unit)
        {
            if (SerialPort == null || !SerialPort.IsOpen) throw new Exception("串口未初始化");

            string cmd = $"@{unit.ToString("00")}FA0000000000402FFFF";

            // 计算 FCS 校验
            var fcs = FCS(cmd);
            cmd += fcs;
            cmd += "*\r";

            byte[] req = Encoding.ASCII.GetBytes(cmd);

            byte[] resp = this.SendAndReceive(req);

            this.CheckResponse(resp);
        }

        public DateTime GetPlcClock(int unit)
        {
            if (SerialPort == null || !SerialPort.IsOpen) throw new Exception("串口未初始化");

            string cmd = $"@{unit.ToString("00")}FA0000000000701";

            // 计算 FCS 校验
            var fcs = FCS(cmd);
            cmd += fcs;
            cmd += "*\r";

            byte[] req = Encoding.ASCII.GetBytes(cmd);

            byte[] resp = this.SendAndReceive(req);

            this.CheckResponse(resp);

            // 解析时间数据
            string resp_str = Encoding.Default.GetString(resp);
            string date_str = resp_str.Substring(23, 14);
            byte[] date_bytes = Convert.FromHexString(date_str);

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

        public void SetPlcClock(int unit, DateTime time)
        {
            if (SerialPort == null || !SerialPort.IsOpen) throw new Exception("串口未初始化");

            string cmd = $"@{unit.ToString("00")}FA0000000000702";

            cmd += time.ToString("yyMMddHHmm");

            // 计算 FCS 校验
            var fcs = FCS(cmd);
            cmd += fcs;
            cmd += "*\r";

            byte[] req = Encoding.ASCII.GetBytes(cmd);

            byte[] resp = this.SendAndReceive(req);

            this.CheckResponse(resp);
        }


        private string FCS(string cmd)
        {
            byte[] b = Encoding.ASCII.GetBytes(cmd);
            byte xorResult = b[0];
            for (int i = 1; i < b.Length; i++)
            {
                xorResult ^= b[i];
            }

            return xorResult.ToString("X2");
        }

        private byte[] SendAndReceive(byte[] reqBytes)
        {
            SerialPort.Write(reqBytes, 0, reqBytes.Length);

            // @   \r
            List<byte> resp = new List<byte>();
            bool is_fill = false;
            while (true)
            {
                byte rb = (byte)SerialPort.ReadByte();
                if (!is_fill && (char)rb == '@') // 判断前5个字节是不是@号FA
                    is_fill = true;

                if (!is_fill) continue;

                resp.Add(rb);

                // 报文结束的条件满足
                if (rb == 0x0D && (char)resp[resp.Count - 2] == '*') // \r     // *\r结尾
                {
                    break;
                }
            }

            return resp.ToArray();
        }

        protected override void CheckResponse(byte[] resp)
        {
            //串口通信是通过传递字符格式，而不是字节格式，一个字节2个字符（如果某个字节有对应的字符，如@，就占一个字符，否则占2个字符）
            //所以要将字节转换成字符串后再进行解析
            string resp_str = Encoding.Default.GetString(resp);
            // 检查FCS校验码，先提取所有响应的内容（FCS校验码和结束码除外）
            string check_str = resp_str.Substring(0, resp_str.Length - 4);
            string fcs = this.FCS(check_str);
            if (fcs != resp_str.Substring(check_str.Length, 2))
            {
                throw new Exception("FCS校验异常，无法确认响应数据");
            }

            //检查状态码
            //状态码前有19个字符，例如响应码是：@06FA 00(响应等待时间) 40(ICF) 00(DNA) 00(SA2) 00(SID) 0101(CMD) 0000(End Code)
            string status = resp_str.Substring(19, 4);
            if (status != "0000")
            {
                if (FINS_ResponseErrors.Errors.ContainsKey(status))
                    throw new Exception(FINS_ResponseErrors.Errors[status]);
                else
                    throw new Exception("未知错误!");
            }
        }

        public byte[] Read(int unit, string variable, ushort count)
        {
            FINS_Parameter finsParameter = this.GetAddress(variable);
            finsParameter.Count = count;
            return this.Read(unit, finsParameter);
        }

        public void Write(int unit, string variable, byte[] dataBytes)
        {
            FINS_Parameter finsParameter = this.GetAddress(variable);
            finsParameter.Data = dataBytes;
            this.Write(unit, finsParameter);
        }

        public void Write(int unit, FINS_Parameter finsParameter)
        {
            this.Write(finsParameter.Data, unit, finsParameter.Area,
                finsParameter.WordAddr,
                finsParameter.BitAddr, finsParameter.DataType);
        }

        private SerialDataReceivedEventHandler _reciveDataEventHandler = null;


        // 关于委托参数：这里可以用一个类进行封装，将时间戳、执行结果状态以及数据字节封装返回
        // 自行支持
        // 这是异步处理的基本思路，未做测试 
        public void ReadAsync(int unit, FINS_Parameter param, Action<byte[]> callback)
        {
            if (SerialPort == null || !SerialPort.IsOpen) throw new Exception("串口未初始化");

            byte a = (byte)param.Area;
            if (param.DataType == DataTypes.WORD)
            {
                a += 0x80;
            }
            string cmd = $"@{unit.ToString("00")}FA0000000000101{a.ToString("X2")}" +
                $"{param.WordAddr.ToString("X4")}{param.BitAddr.ToString("X2")}{param.Count.ToString("X4")}";
            // 计算 FCS 校验
            var fcs = FCS(cmd);
            cmd += fcs;
            cmd += "*\r";

            byte[] req = Encoding.ASCII.GetBytes(cmd);

            if (_reciveDataEventHandler != null)
            {
                SerialPort.DataReceived -= _reciveDataEventHandler;
            }
            _reciveDataEventHandler = (se, ev) =>
            {
                List<byte> resp = new List<byte>();
                bool is_fill = false;
                while (true)
                {
                    byte rb = (byte)SerialPort.ReadByte();
                    if (!is_fill && (char)rb == '@') // 判断前5个字节是不是@号FA
                        is_fill = true;

                    if (!is_fill) continue;

                    resp.Add(rb);

                    // 报文结束的条件满足
                    if (rb == 0x0D && (char)resp[resp.Count - 2] == '*') // \r     // *\r结尾
                    {
                        break;
                    }
                }
                this.CheckResponse(resp.ToArray());

                string resp_str = Encoding.Default.GetString(resp.ToArray());
                int len = param.Count * 2;
                if (param.DataType == DataTypes.WORD)
                    len = param.Count * 4;
                string data_str = resp_str.Substring(23, len);

                //  这个方法在.NET环境下有效，Framework环境下需要循环，每两个字节转一个字节
                byte[] data_bytes = Convert.FromHexString(data_str);

                callback?.Invoke(data_bytes);
            };
            SerialPort.DataReceived += _reciveDataEventHandler;

            SerialPort.Write(req, 0, req.Length);
        }
    }
}
