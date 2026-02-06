using System.Data;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;

namespace Coffee.Siemens.Communication
{
    public class S7Client
    {
        private Socket _socke = new Socket(SocketType.Stream, ProtocolType.Tcp);

        private int _pduSize = 0;

        /// <summary>
        /// 上位机主动和设备建立通信连接。一个西门子设备可以有多个机架和插槽组成。
        /// </summary>
        /// <param name="ip">设备的IP地址</param>
        /// <param name="rack">机架号</param>
        /// <param name="slot">插槽号</param>
        /// <param name="timeout">上位机等待设备回应的超时时间</param>
        public void Connect(string ip, byte rack, byte slot, int timeout = 5000)
        {
            _socke.ReceiveTimeout = timeout;
            _socke.Connect(ip, 102); //端口号始终是102

            //创建COTP报文
            byte[] bytesToCOTP = new byte[]
            {
                //TPKT报文
                0x03, //Version，版本默认3
                0x00, //Reserved，保留默认0
                0x00, //整个请求字节数 [Hi]
                0x16, // [Lo]
                //COTP报文
                0x11, //当前字节以后的字节数
                (byte)S7_PDUTypes.ConnectRequest, //0xe0 PDU Type
                0x00, 0x00, //DST reference
                0x00, 0x01, //SRC reference
                0X00, //0000 --00 [0000: Class, Extended formats, No explilcit flow control ]

                0xc1, //Parameter-Code：src-tsap     上位机
                0x02, //Parameter-Len
                0x10, //Source TSAP:01->PG;02->OP;03->S7单边（服务器模式）;0x10->S7双边通信
                0x00, //机架号与插槽号为0  0010 0011  0-4插槽     5-7机架 上位机 机架号和插槽号始终为0

                0xc2, //Parameter-code: dst-tsap      PLC
                0x02, //Parameter len
                0x03, //Destination TSAP
                //0x01, //机架与插槽号：0，1->200Smart/1200/1500;0,2->300/400
                (byte)(rack * 32 + slot),

                0xc0, //Parameter code:tpdu-size
                0x01, //Parameter length
                0x0a  //TPDU size
            };
            SendAndReceive(bytesToCOTP);

            //创建Setup Communication报文
            byte[] bytesToSetup = new byte[]
            {
                //TPKT报文
                0x03, //Version，版本默认3
                0x00, //Reserved，保留默认0
                0x00, //整个请求字节数 [Hi]
                0x19, // [Lo]
                //COTP报文
                0x02, //在COTP这个环节中当前字节以后的字节数    
                0xf0, //PDU Type
                //-000 0000 TPDU number
                //1--- ---- Last data unit:Yes
                0x80,
                //S7-Header
                0x32, //Protocol Id，默认
                (byte)S7_ROSCTR.JobRequest, //ROSCTR:  JOB
                0x00,0x00, //Redundancy Identification (Reserved)
                0x00,0x00, //Protocol Data Unit Reference累加序号
                0x00, //Parameter length [Hi]
                0x08, //[Lo]
                0x00, //Data length [Hi]
                0x00, //[Lo]
                //S7-Parameter
                (byte)S7_Functions.SetCommunication, //Function:Setup communication
                0x00, //Reserved
                0x00, 0x03, //Max AmQ(parallel jobs with ack) calling
                0x00, 0x03, //Max AmQ(parallel jobs with ack) called
                0x03, //PDU length [Hi] PDU 长度，根据PLC社保的反馈设置，可以是240、480、960，默认960
                0xc0  //[Lo]
            };
            //返回排除TPKT的响应内容
            byte[] bytesToResponse = SendAndReceive(bytesToSetup);
            if (bytesToResponse[14] != 0x00)
                throw new Exception("连接异常：Setup Communication失败");

            // 返回通信方PLC的PDU长度
            byte[] bytesToPudSize = new byte[] { bytesToResponse[21], bytesToResponse[22] };
            if (BitConverter.IsLittleEndian)
            {
                bytesToPudSize = bytesToPudSize.Reverse().ToArray();
            }
            _pduSize = BitConverter.ToUInt16(bytesToPudSize);
        }

        /// <summary>
        /// 上位机主动和设备断开连接。
        /// </summary>
        public void Disconnect()
        {
            if (_socke == null)
                return;
            _socke.Disconnect(true);
            _socke.Close();
            _socke = null;
        }

        /// <summary>
        /// 从设备的指定地址读取指定类型的数据。
        /// 通过参数variables数组中的请求参数项可以一次连续读取多个不同存储区域的数据。每个请求项可以请求读取一个Count属性指定的连续区域。
        /// 注意：每个请求项只能读取一个Bit数据，不能连续读取。如果要读取多个Bit数据，可以增加多个请求项。
        /// </summary>
        /// <param name="variables">读取的请求参数对象。可以传递多个请求对象，用于指定读取多个数据区域，多个连续地址等。同时，读取的响应内容最终保存在请求对象的DataBytes属性中。</param>
        public void Read(params DataParameter[] variables)
        {
            byte[] bytesToParameters = GetS7Parameters(variables);

            byte[] bytesToCOTP = new byte[]
            {
                //COTP
                0x02, //当前字节以后的字节数
                0xf0, //PDU Type
                //-000 0000 TPDU number
                //1--- ---- Last data unit:Yes
                0x80,
                //S7-Header
                0x32, //Protocol Id，默认
                (byte)S7_ROSCTR.JobRequest, //ROSCTR:JOB
                0x00, 0x00, //edundancy Identification (Reserved)
                0x00, 0x00, //Protocol Data Unit Reference
                //参数总长度 = 参数报文字节总数 + 2字节（S7-Parameter）
                (byte)((bytesToParameters.Length + 2)/256%256), //Parameter length [Hi] 注意：所有Parameter Item字节个数 + S7-Paramter功能码和Item个数(2个字节）
                (byte)((bytesToParameters.Length + 2)%256), //[Lo]
                //读取请求时，没有Data报文
                0x00,0x00, //Data length
                //S7-Parameter
                (byte)S7_Functions.ReadVariable, // 向PLC发送一个读变量的请求
                (byte)variables.Length, // Parameter Item count  Item中包含了请求的地址以及类型相关信息
            };

            List<byte> bytesToRequest = new List<byte>
            {
                //TPKT
                0x03, //Version，版本默认3
                0x00, //Reserved，保留默认0
                //整个请求字节数 = (COTP字节数 + S7-Header字节数 + S7-Parameter字节数) + 所有参数报文字节数 + 4字节TPKT报文
                //注意：读取请求时，没有S7-Data部分
                (byte)((bytesToCOTP.Length + bytesToParameters.Length + 4) / 256 % 256),
                (byte)((bytesToCOTP.Length + bytesToParameters.Length + 4) % 256),
            };
            bytesToRequest.AddRange(bytesToCOTP);
            bytesToRequest.AddRange(bytesToParameters);

            //返回的响应字节排除TPKP4个字节
            byte[] bytesToResponse = SendAndReceive(bytesToRequest.ToArray());

            // 检查响应是否异常。其中响应字节数组中元素13是Error class: No error (0x00)， 元素14是Error code: 0x00
            ushort header_error_code = (ushort)(bytesToResponse[13] << 8 + bytesToResponse[14]); //元素13,14组成错误码
            if (header_error_code != 0)
            {
                if (ResponseStatus.ErrorCodes.ContainsKey(header_error_code))
                {
                    throw new Exception(ResponseStatus.ErrorCodes[header_error_code]);
                }
                else
                {
                    throw new Exception("未知错误！");
                }
            }

            //开始遍历响应中的数据部分，从第17个字节开始，因为前面是17个字节的固定长度报文
            int offset = 17;
            List<byte> responseByteList = bytesToResponse.ToList();
            for (int i = 0; i < variables.Length; i++)
            {
                if (bytesToResponse[offset] != 0xff) //如果响应数据状态不正确。Return code: Success (0xff)
                {
                    if (ResponseStatus.DataReturnCodes.ContainsKey(bytesToResponse[offset]))
                    {
                        throw new Exception(ResponseStatus.DataReturnCodes[bytesToResponse[offset]]);
                    }
                    else
                    {
                        throw new Exception("未知错误！");
                    }
                }
                offset++; //跳过一个字节Transport size: BYTE/WORD/DWORD (0x04)
                // 接下来2个元素保存的是数据响应长度。注意：这个长度不是字节数，而是多少个位。所以响应长度 = 总数据字节数 * 8（如果保存的是Bit，不需要乘8）
                // 后续有多少个字节 位数 / 8 = 字节数
                // 如果读取的是BIT，则始终返回一个位（即便请求了多个位读取）。因为当前PLC仅支持在一个读取块中读取一个位信息，所以返回的是后续有一个字节，不需要除8。
                int data_bytes_len = bytesToResponse[offset + 1] * 256 + bytesToResponse[offset + 2];
                if (bytesToResponse[offset] == (byte)S7_DataVarType.BYTE || bytesToResponse[offset] == (byte)S7_DataVarType.WORD || bytesToResponse[offset] == (byte)S7_DataVarType.DWORD)
                {
                    data_bytes_len /= 8;
                }
                byte[] bytesToData = responseByteList.GetRange(offset + 3, data_bytes_len).ToArray();
                variables[i].DataBytes = bytesToData;

                //通常一次读取响应可以返回多个响应数据，其中一个响应数据由Return code，Transport size，2字节数据响应长度和相应的数据内容组成
                offset += (2 + 1); //由于offset已经定位到Return code，所以先偏移Transport size，2字节数据响应长度
                offset += data_bytes_len; //再偏移相应的数据内容所占字节数
                offset += bytesToData.Length % 2; //为了确保读取的Item的字节数是偶数，在读取奇数个数据字节数时，要添加一个FillByte字节
            }
        }

        /// <summary>
        /// 从设备的指定地址读取指定类型的数据。
        /// 注意：不能连续读取多个Bit数据。
        /// </summary>
        /// <param name="variable">包含读取的目标存储区域及地址</param>
        /// <param name="count">连续读取的数据个数。系统仅支持读取1个位数据，当count大于1，会抛异常</param>
        /// <returns>返回读取的数据字节数组</returns>
        public byte[] Read(string variable, int count = 1)
        {
            DataParameter parameter = ParseRequestAddress(variable, count, S7_Functions.ReadVariable);
            if (parameter.ParameterVarType == S7_ParameterVarType.BIT && count != 1)
            {
                throw new Exception("系统仅支持读取1个位数据！");
            }
            Read(parameter);
            return parameter.DataBytes;
        }

        /// <summary>
        /// 从设备的指定地址读取指定类型的数据。
        /// 注意：不能连续读取多个Bit数据。
        /// </summary>
        /// <typeparam name="T">泛型指定读取的数据类型</typeparam>
        /// <param name="variable">包含读取的目标存储区域及地址</param>
        /// <param name="count">连续读取的数据个数。系统仅支持读取1个位数据，当count大于1，会抛异常</param>
        /// <returns>返回读取的泛型指定类型的数据集</returns>
        public T[] Read<T>(string variable, int count = 1)
        {
            byte[] bytes = Read(variable, count);
            return BytesToData<T>(bytes).ToArray();
        }

        /// <summary>
        /// 从设备的指定地址写入指定类型的数据。
        /// 通过参数variables数组中的请求参数项可以一次连续写入多个不同存储区域的数据。每个请求项可以请求写入一个Count属性指定的连续区域。
        /// 注意：请求项只能写入一个Bit数据，不能连续读取。系统不支持增加多个写入Bit数据的请求项。
        /// </summary>
        /// <param name="variables">写入的请求参数对象。可以传递多个请求对象，用于指定写入多个数据区域，多个连续地址等。同时，写入的内容通过请求对象的DataBytes属性传递给设备。</param>
        public void Write(params DataParameter[] variables)
        {
            byte[] bytesToParameters = GetS7Parameters(variables);
            byte[] bytesToDatas = GetS7Datas(variables);

            byte[] bytesToCOTP = new byte[]
            {
                //COTP
                0x02, //当前字节以后的字节数
                0xf0, //PDU Type
                //-000 0000 TPDU number
                //1--- ---- Last data unit:Yes
                0x80,
                //S7-Header
                0x32, //Protocol Id，默认
                (byte)S7_ROSCTR.JobRequest, //ROSCTR:JOB
                0x00, 0x00, //edundancy Identification (Reserved)
                0x00, 0x00, //Protocol Data Unit Reference
                //参数总长度 = 参数报文字节总数 + 2字节（S7-Parameter中的Function和ItemCount）
                (byte)((bytesToParameters.Length + 2) / 256 % 256), //Parameter length [Hi]
                (byte)((bytesToParameters.Length + 2) % 256), //[Lo]
                //写入请求时的Data报文
                (byte)(bytesToDatas.Length / 256 % 256), //Data length [Hi]
                (byte)(bytesToDatas.Length % 256), //[Lo]
                //S7-Parameter
                (byte)S7_Functions.WriteVariable, // 向PLC发送一个读变量的请求
                (byte)variables.Length, // Parameter Item count  Item中包含了请求的地址以及类型相关信息
            };

            List<byte> bytesToRequest = new List<byte>
            {
                //TPKT
                0x03, //Version，版本默认3
                0x00, //Reserved，保留默认0
                //整个请求字节数 = (COTP字节数 + S7-Header字节数 + S7-Parameter字节数) + 所有参数报文字节数 + 4字节TPKT报文
                (byte)((bytesToCOTP.Length + bytesToParameters.Length + bytesToDatas.Length + 4) / 256 % 256),
                (byte)((bytesToCOTP.Length + bytesToParameters.Length + bytesToDatas.Length + 4) % 256),
            };
            bytesToRequest.AddRange(bytesToCOTP);
            bytesToRequest.AddRange(bytesToParameters);
            bytesToRequest.AddRange(bytesToDatas);

            //返回的响应字节排除TPKP4个字节
            byte[] bytesToResponse = SendAndReceive(bytesToRequest.ToArray());

            // 检查响应是否异常。其中响应字节数组中元素13是Error class: No error (0x00)， 元素14是Error code: 0x00
            ushort header_error_code = (ushort)(bytesToResponse[13] << 8 + bytesToResponse[14]); //元素13,14组成错误码
            if (header_error_code != 0)
            {
                throw new Exception(ResponseStatus.ErrorCodes[header_error_code]);
            }

            //开始遍历响应中的数据部分，从第17个字节开始，因为前面是17个字节的固定长度报文
            int offset = 17;
            for (int i = 0; i < variables.Length; i++)
            {
                if (bytesToResponse[offset] != 0xff) //如果响应数据状态不正确。Return code: Success (0xff)
                {
                    var aa = ResponseStatus.DataReturnCodes;
                    if (ResponseStatus.DataReturnCodes.ContainsKey(bytesToResponse[offset]))
                    {
                        throw new Exception(ResponseStatus.DataReturnCodes[bytesToResponse[offset]]);
                    }
                    else
                    {
                        throw new Exception("未知错误！");
                    }
                }
                offset++; //通常一次写入响应可以返回多个响应数据，但是其中一个响应数据仅由Return code一个字节组成
            }
        }

        /// <summary>
        /// 从设备的指定地址写入数据。
        /// 注意：不能连续写入多个Bit数据。
        /// </summary>
        /// <param name="requestAddress">包含写入的目标存储区域及地址</param>
        /// <param name="datas">写入数据的字节数组</param>
        public void Write(string requestAddress, byte[] datas)
        {
            if (datas == null || datas.Length == 0)
            {
                throw new ArgumentNullException("写入数据不能为空！");
            }
            //系统仅支持写入一个Bit的数据，但可以写入多个Byte/Word/DWord的数据。
            //因为解析写入地址前，不知道写入数据类型，默认是写Bit数据，所以传入参数1
            DataParameter reqParameter = ParseRequestAddress(requestAddress, 1, S7_Functions.WriteVariable);
            if (reqParameter.ParameterVarType == S7_ParameterVarType.BYTE)
            {
                reqParameter.Count = datas.Length;
            }
            else if (reqParameter.ParameterVarType == S7_ParameterVarType.WORD)
            {
                if (datas.Length % 2 > 0)
                {
                    throw new Exception("写入数据格式有误，提供的字节数不符号'一个WORD类型占用2个字节'的要求！");
                }
                reqParameter.Count = datas.Length / 2;
            }
            else if (reqParameter.ParameterVarType == S7_ParameterVarType.DWORD)
            {
                if (datas.Length % 4 > 0)
                {
                    throw new Exception("写入数据格式有误，提供的字节数不符号'一个DWORD类型占用4个字节'的要求！");
                }
                reqParameter.Count = datas.Length / 4;
            }
            reqParameter.DataBytes = datas;

            Write(reqParameter);
        }

        /// <summary>
        /// 从设备的指定地址写入数据。
        /// 注意：不能连续写入多个Bit数据。
        /// </summary>
        /// <typeparam name="T">泛型指定写入的数据类型</typeparam>
        /// <param name="requestAddress">包含写入的目标存储区域及地址</param>
        /// <param name="datas">写入泛型指定类型的数据集</param>
        public void Write<T>(string requestAddress, T[] datas)
        {
            byte[] bytes = DataToBytes<T>(datas);
            Write(requestAddress, bytes);
        }

        public void Run()
        {
            byte[] bytesToRequest = new byte[] {
                // TPKT
                0x03, //Version，版本默认3
                0x00, //Reserved，保留默认0
                0x00,0x25, //整个请求字节数

                // COTP
                0x02, //当前字节以后的字节数
                0xf0, //PDU Type，数据传输
                //-000 0000 TPDU number
                //1--- ---- Last data unit:Yes
                0x80,

                // S7-Header
                0x32, //Protocol Id，默认
                0x01, //ROSCTR:JOB
                0x00,0x00, //Redundancy Identification (Reserved)
                0x00,0x00, //Protocol Data Unit Reference

                0x00,0x14, // Parameter Length
                0x00,0x00, // Data Length

                // S7-Parameter
                0x28,//Function:PI-Service 控制PLC启动
                0x00,0x00,0x00,0x00,0x00,0x00,0xfd, //Unknown bytes

                0x00,0x00, //Parameter block length
                0x09, //String length
                // PI (program invocation) Service: P_PROGRAM [PI-Service P_PROGRAM (PLC Start / Stop)]
                0x50,0x5F,0x50,0x52,0x4f,0x47,0x52,0x41,0x4d //保存的就是字符串"P_PROGRAM"
            };

            //返回的响应字节排除TPKP4个字节
            byte[] bytesToResponse = SendAndReceive(bytesToRequest);

            // 检查响应是否异常。其中响应字节数组中元素13是Error class: No error (0x00)， 元素14是Error code: 0x00
            ushort header_error_code = (ushort)(bytesToResponse[13] << 8 + bytesToResponse[14]); //元素13,14组成错误码
            if (header_error_code != 0)
            {
                throw new Exception(ResponseStatus.ErrorCodes[header_error_code]);
            }
        }

        public void Stop()
        {
            byte[] bytesToRequest = new byte[] {
                // TPKT
                0x03, //Version，版本默认3
                0x00, //Reserved，保留默认0
                0x00,0x21, //整个请求字节数

                // COTP
                0x02, //当前字节以后的字节数
                0xf0, //PDU Type，数据传输
                //-000 0000 TPDU number
                //1--- ---- Last data unit:Yes
                0x80,

                // S7-Header
                0x32, //Protocol Id，默认
                0x01, //ROSCTR:JOB
                0x00,0x00, //Redundancy Identification (Reserved)
                0x00,0x00, //Protocol Data Unit Reference

                0x00,0x10, // Parameter Length
                0x00,0x00, // Data Length

                // S7-Parameter
                0x29,//Function:PI-Service 控制PLC停止
                0x00,0x00,0x00,0x00,0x00, //Unknown bytes

                0x09, //String length
                // PI(program invocation) Service:P_PROGRAM
                0x50,0x5F,0x50,0x52,0x4f,0x47,0x52,0x41,0x4d //保存的就是字符串"P_PROGRAM"
            };

            //返回的响应字节排除TPKP4个字节
            byte[] bytesToResponse = SendAndReceive(bytesToRequest);

            // 检查响应是否异常。其中响应字节数组中元素13是Error class: No error (0x00)， 元素14是Error code: 0x00
            ushort header_error_code = (ushort)(bytesToResponse[13] << 8 + bytesToResponse[14]); //元素13,14组成错误码
            if (header_error_code != 0)
            {
                throw new Exception(ResponseStatus.ErrorCodes[header_error_code]);
            }
        }

        public DateTime GetTime(out DayOfWeek? week)
        {
            byte[] bytesToRequest = new byte[] {
                // TPKT
                0x03, //Version，版本默认3
                0x00, //Reserved，保留默认0
                0x00,0x1d, //整个请求字节数

                // COTP
                0x02, //当前字节以后的字节数
                0xf0, //PDU Type，数据传输
                //-000 0000 TPDU number
                //1--- ---- Last data unit:Yes
                0x80,

                // S7-Header
                0x32, //Protocol Id，默认
                0x07, //ROSCTR:Userdata
                0x00,0x00, //Redundancy Identification (Reserved)
                0x00,0x00, //Protocol Data Unit Reference

                0x00,0x08, // Parameter Length
                0x00,0x04, // Data Length

                // S7-Parameter
                0x00,0x01,0x12,//Parameter head
                0x04, //Parameter length
                0x11, //Method (Request/Response): Req (0x11)
                //0100 ----   Type:Requet(4)
                //---- 0111   Function group:Time functions(7)
                0x47,
                0x01, //Subfunction: Read clock (1)
                0x00, //Sequence number:0

                //Data
                0x0a, //Return code: Object does not exist (0x0a)
                0x00, //ransport size: NULL (0x00)
                0x00, 0x00 //Length
            };

            //返回的响应字节排除TPKP4个字节
            byte[] bytesToResponse = SendAndReceive(bytesToRequest);

            // 检查响应是否异常。其中响应字节数组中元素13是Error class: No error (0x00)， 元素14是Error code: 0x00
            ushort header_error_code = (ushort)(bytesToResponse[23] << 8 + bytesToResponse[24]); //元素13,14组成错误码
            if (header_error_code != 0)
            {
                throw new Exception(ResponseStatus.ErrorCodes[header_error_code]);
            }
            //返回响应的第25个字节指出是否返回正确的值，如果Return code = 0xff, 返回Success
            else if (bytesToResponse[25] != 0xff)
            {
                throw new Exception(ResponseStatus.DataReturnCodes[bytesToResponse[25]]);
            }
            else
            {
                //从返回响应的第29个字节开始，保存的是S7 Timestamp信息。
                //0x00    S7 Timestamp -Reserved: 0x00  （第29字节）
                //20  S7 Timestamp -Year 1、Year 2: 2个字节表示年 （第30、31字节）
                //5   S7 Timestamp -Month: 1-12月：01-12 （第32字节）
                //8   S7 Timestamp -Day: 1-31日：01-31 （第33字节）
                //20  S7 Timestamp -Hour: 0-23时：00-23 （第34字节）
                //0   S7 Timestamp -Minute: 0-59分：00-59 （第35字节）
                //0   S7 Timestamp -Second: 0-59秒：00-59 （第36字节）
                // （第37、38字节）保存的是毫秒和星期几 15-8位是毫秒的百位十位，7-4是毫秒的个位，3-0位是星期几
                //0000 0000 0100----  Milliseconds '---- ---- ---- 0111 Weekday:Saturday(7)
                // 年
                byte b_year = bytesToResponse[31];//高序字节表示低位，即个十位（注意：如果值小于90则为20XX年，否则位19XX年）
                //值虽然是16进制保存的，但是提取出来不能用16进制转10进制，而是应该转换为字符串再直接输出其数字
                //例如：0x25 就是数字25，因为其小于90，所以是最终获得的是2025年
                //例如：0x97 就是数字97，因为其大于90，所以是最终获得的是1997年
                if (!int.TryParse(b_year.ToString("X2"), out int year))
                {
                    throw new Exception("时间转换失败");
                }
                year = year > 90 ? 1900 + year : 2000 + year;
                // 月
                byte m_byte = bytesToResponse[32];
                if (!int.TryParse(m_byte.ToString("X2"), out int month))
                {
                    throw new Exception("时间转换失败");
                }
                // 日
                byte dayByte = bytesToResponse[33];
                if (!int.TryParse(dayByte.ToString("X2"), out int day))
                {
                    throw new Exception("时间转换失败");
                }
                // 时
                byte hourByte = bytesToResponse[34];
                if (!int.TryParse(hourByte.ToString("X2"), out int hour))
                {
                    throw new Exception("时间转换失败");
                }
                // 分
                byte minuteByte = bytesToResponse[35];
                if (!int.TryParse(minuteByte.ToString("X2"), out int minute))
                {
                    throw new Exception("时间转换失败");
                }
                // 秒
                byte secondByte = bytesToResponse[36];
                if (!int.TryParse(secondByte.ToString("X2"), out int second))
                {
                    throw new Exception("时间转换失败");
                }
                //毫秒和星期几
                List<byte> bytesToMsWeek = new List<byte>
                {
                    bytesToResponse[37], bytesToResponse[38]
                };
                if (BitConverter.IsLittleEndian)
                {
                    bytesToMsWeek.Reverse();
                }
                ushort ms_week = BitConverter.ToUInt16(bytesToMsWeek.ToArray(), 0);
                if (ms_week % 8 == 7)
                {
                    week = null;
                }
                else
                {
                    week = (DayOfWeek)(ms_week % 8);
                }
                ushort ms = (ushort)(ms_week >> 4);
                DateTime dt = new DateTime(year, month, day, hour, minute, second, ms);
                return dt;
            }
        }

        public void SetTime(DateTime time)
        {
            if (time.Year < 1990 || time.Year > 2089)
            {
                throw new Exception($"设置日期时间范围必须在{1990} - {2089}");
            }
            byte year = Convert.FromHexString((time.Year - 2000 < 0 ? time.Year - 1900 : time.Year - 2000).ToString("00"))[0];
            byte month = Convert.FromHexString(time.Month.ToString("00"))[0];
            byte day = Convert.FromHexString(time.Day.ToString("00"))[0];
            byte hour = Convert.FromHexString(time.Hour.ToString("00"))[0];
            byte minute = Convert.FromHexString(time.Minute.ToString("00"))[0];
            byte second = Convert.FromHexString(time.Second.ToString("00"))[0];

            //因为DateTime结构中的毫秒是4个字节的整形，而在S7设备中仅用2个字节中的第15-4位表示毫秒，所以仅保留整形的11-0位值
            ushort ms = (ushort)(time.Millisecond & 0x00000FFF);
            byte week = (byte)time.DayOfWeek;
            ushort ms_week = (ushort)((ms << 4) | week);

            List<byte> bytesToRequest = new List<byte> {
                // TPKT
                0x03, //Version，版本默认3
                0x00, //Reserved，保留默认0
                0x00,0x27, //整个请求字节数

                // COTP
                0x02, //当前字节以后的字节数
                0xf0, //PDU Type，数据传输
                //-000 0000 TPDU number
                //1--- ---- Last data unit:Yes
                0x80,

                // S7-Header
                0x32, //Protocol Id，默认
                0x07, //ROSCTR:Userdata
                0x00,0x00, //Redundancy Identification (Reserved)
                0x00,0x00, //Protocol Data Unit Reference

                0x00,0x08, // Parameter Length
                0x00,0x0e, // Data Length

                // S7-Parameter
                0x00,0x01,0x12,//Parameter head
                0x04, //Parameter length
                0x11, //Method (Request/Response): Req (0x11)
                //0100 ----   Type:Requet(4)
                //---- 0111   Function group:Time functions(7)
                0x47,
                0x02, //Subfunction: Set clock (1)
                0x00, //Sequence number:0

                //Data
                0xff, //Return code: Success (0xff)
                0X09, //Transport size: OCTET STRING (0x09)
                0x00, 0x0a, //Length
                //S7 Timestamp相关信息
                0x00,
                0x19,
                year, // Year:25
                month, // Month:10
                day, // Day:01
                hour, // Hour:20
                minute, // Minute:05
                second, // Second:00
            };
            //毫秒和星期几 占用2个字节，其中15-8位是毫秒的百位十位，7-4是毫秒的个位，3-0位是星期几
            byte[] bytes_ms_week = BitConverter.GetBytes(ms_week);
            if (!BitConverter.IsLittleEndian)
            {
                bytes_ms_week = bytes_ms_week.Reverse().ToArray();
            }
            bytesToRequest.AddRange(bytes_ms_week);

            //返回的响应字节排除TPKP4个字节
            byte[] bytesToResponse = SendAndReceive(bytesToRequest.ToArray());

            // 检查响应是否异常。其中响应字节数组中元素13是Error class: No error (0x00)， 元素14是Error code: 0x00
            ushort header_error_code = (ushort)(bytesToResponse[23] << 8 + bytesToResponse[24]); //元素13,14组成错误码
            if (header_error_code != 0)
            {
                throw new Exception(ResponseStatus.ErrorCodes[header_error_code]);
            }
            //返回响应的第25个字节指出是否返回正确的值，如果Return code = 0xff, 返回Success
            //注意：可能
            else if (bytesToResponse[25] != 0xff)
            {
                throw new Exception(ResponseStatus.DataReturnCodes[bytesToResponse[25]]);
            }
        }

        private DataParameter ParseRequestAddress(string requestAddress, int count, S7_Functions function)
        {
            if (string.IsNullOrWhiteSpace(requestAddress) || requestAddress.Length < 2)
            {
                throw new ArgumentException("请求地址格式错误，无法解析！");
            }
            DataParameter reqParameter = new DataParameter(function);
            reqParameter.Count = count;

            string str1 = requestAddress.Substring(0, 2);
            if (str1.ToUpper() == "DB")
            {
                reqParameter.Area = S7_Areas.DB;
                //西门子S7设备的地址格式一般为：DB1DBW1 或 DB1DBX1.0
                //为了提供更好的兼容性，可以省略中间的DB，例如：DB1W1
                string[] sections = requestAddress.Split('.'); //拆分出多个地址片段
                //解析出DB Number
                if (!ushort.TryParse(sections[0].Substring(2), out ushort db_num))
                {
                    throw new ArgumentException("请求地址格式错误，无法解析！");
                }
                reqParameter.DBNumber = db_num;

                if (sections.Length < 2)
                {
                    throw new ArgumentException("请求地址格式错误，无法解析！");
                }
                //解析出请求的数据类型
                if (sections[1].Length != sections[1].Trim().Length) //当数据区域前后有空格
                {
                    throw new ArgumentException("请求地址格式错误，无法解析！");
                }
                //请求数据类型在地址片段中的索引
                int dataTypeIndex = sections[1].StartsWith("DB", StringComparison.OrdinalIgnoreCase) ? 2 : 0;
                string paramTypeStr = paramTypeStr = sections[1].Substring(dataTypeIndex, 1);
                //如果省略数据类型X，即DB后没跟表示数据类型的字符，而是直接跟字节地址（一般X表示Bit，B表示Byte，W表示Word，D表示DWord）
                bool isReadBit = int.TryParse(paramTypeStr, out int val) && sections.Length == 3; //省略数据类型X，直接按byteAddr.bitAddr读取

                if (!ParamVarDict.TryGetValue(paramTypeStr, out S7_ParameterVarType paraVarType) && !isReadBit)
                {
                    throw new ArgumentException("请求地址格式错误，无法解析！");
                }
                reqParameter.ParameterVarType = isReadBit ? S7_ParameterVarType.BIT : paraVarType;

                //请求数据的字节地址在地址片段中的索引
                int byteAddrIndex = isReadBit ? dataTypeIndex : dataTypeIndex + 1;
                if (!int.TryParse(sections[1].Substring(byteAddrIndex), out int byteAddr))
                {
                    throw new ArgumentException("请求地址格式错误，无法解析！");
                }
                reqParameter.ByteAddress = byteAddr;

                //请求的地址包含位地址
                if (sections.Length == 3)
                {
                    if (!byte.TryParse(sections[2], out byte bitAddr))
                    {
                        throw new ArgumentException("请求地址格式错误，无法解析！");
                    }
                    reqParameter.BitAddress = bitAddr;
                }
                else
                {
                    throw new ArgumentException("请求地址格式错误，无法解析！");
                }
            }
            else if ("IQMV".Contains(requestAddress[0], StringComparison.OrdinalIgnoreCase))
            {
                //请求数据的存储区域
                if (!Enum.TryParse(requestAddress[0].ToString(), true, out S7_Areas area))
                {
                    throw new ArgumentException("请求地址格式错误，无法解析！");
                }
                reqParameter.Area = area;
                if (area == S7_Areas.V)
                {
                    reqParameter.DBNumber = 1;
                }

                string[] sections = requestAddress.Split('.'); //拆分出多个地址片段
                if (sections.Length > 2)
                {
                    throw new ArgumentException("请求地址格式错误，无法解析！");
                }

                if (sections.Length == 2) //请求的地址包含位地址
                {
                    if (!int.TryParse(sections[0].Substring(1), out int byteAddr))
                    {
                        throw new ArgumentException("请求地址格式错误，无法解析！");
                    }
                    if (!byte.TryParse(sections[1], out byte bitAddr))
                    {
                        throw new ArgumentException("请求地址格式错误，无法解析！");
                    }
                    reqParameter.ByteAddress = byteAddr;
                    reqParameter.BitAddress = bitAddr;
                }
                else
                {
                    //提取请求数据类型
                    string paramTypeStr = paramTypeStr = sections[0].Substring(1, 1);
                    if (!ParamVarDict.TryGetValue(paramTypeStr, out S7_ParameterVarType paraVarType))
                    {
                        throw new ArgumentException("请求地址格式错误，无法解析！");
                    }
                    reqParameter.ParameterVarType = paraVarType;
                    // 提取请求数据的字节地址
                    if (!int.TryParse(sections[0].Substring(2), out int byteAddr))
                    {
                        throw new ArgumentException("请求地址格式错误，无法解析！");
                    }
                    reqParameter.ByteAddress = byteAddr;
                }
            }
            else
            {
                throw new ArgumentException("请求地址格式错误，无法解析！");
            }

            return reqParameter;
        }

        /// <summary>
        /// 从参数传入的变量中提取并构建出S7-Parameter结构的报文数据。
        /// </summary>
        /// <param name="variables">用于构建S7-Parameter报文的元数据</param>
        /// <returns>返回S7-Parameter结构的报文字节数据</returns>
        private byte[] GetS7Parameters(DataParameter[] variables)
        {
            List<byte> bytes = new List<byte>();
            int sum = 0;
            foreach (DataParameter parameter in variables)
            {
                int byteNum = 1; //当前请求数据类型所占字节数
                if (parameter.ParameterVarType == S7_ParameterVarType.WORD)
                    byteNum = 2;
                else if (parameter.ParameterVarType == S7_ParameterVarType.DWORD)
                    byteNum = 4;
                byteNum *= parameter.Count; //多个连续数据块所占字节数
                byteNum += (byteNum % 2); //如果所占字节数是奇数，则需要补偿一个字节的FillByte
                sum += byteNum;

                byte[] bytesToParam = new byte[]
                {
                    0x12, //结构标识，一般默认0x12
                    0x0a, //此字节往后的字节长度
                    (byte)S7_SyntaxIds.S7ANY, //Syntax Id: S7ANY (0x10)
                    (byte)parameter.ParameterVarType, //Transport size: BYTE (2)
                    (byte)(parameter.Count / 256 % 256), //数据长度 [Hi]
                    (byte)(parameter.Count % 256), //[Lo]
                    (byte)(parameter.DBNumber / 256 % 256), //数据块编号 [Hi]     DB1.DBX100.0
                    (byte)(parameter.DBNumber % 256), //[Lo]
                    (byte)parameter.Area, //Area
                     //读取或写入数据的目标地址（占用3个字节，其中18-3位是字节地址部分，2-0位是位地址部分）。
                    (byte)(((parameter.ByteAddress << 3) + parameter.BitAddress) / 256 / 256 % 256), //23-16位
                    (byte)(((parameter.ByteAddress << 3) + parameter.BitAddress) / 256 % 256), //15-8位
                    (byte)(((parameter.ByteAddress << 3) + parameter.BitAddress) % 256) //7-0位
                };

                bytes.AddRange(bytesToParam);
            }

            // 报异常/自动分组
            if (sum > (_pduSize - 50))
                throw new Exception("请确认请求数据量");

            return bytes.ToArray();
        }

        /// <summary>
        /// 从参数传入的变量中提取并构建出S7-Data结构的报文数据。
        /// </summary>
        /// <param name="variables">用于构建S7-Data报文的元数据</param>
        /// <returns>返回S7-Data结构的报文字节数据</returns>
        private byte[] GetS7Datas(DataParameter[] variables)
        {
            List<byte> datas = new List<byte>();
            for (int i = 0; i < variables.Length; i++)
            {
                DataParameter parameter = variables[i];
                //数据响应长度是按位计算的，所以DataLength = 字节数 * 8
                int bit = 1;
                if (parameter.DataVarType == S7_DataVarType.BYTE || parameter.DataVarType == S7_DataVarType.WORD || parameter.DataVarType == S7_DataVarType.DWORD)
                {
                    bit = 8;
                }
                else if (parameter.DataVarType == S7_DataVarType.BIT)
                {
                    if (parameter.Count != 1 || parameter.DataBytes.Length != 1)
                    {
                        throw new Exception("当前PLC不支持写入多个连续的位数据！");
                    }
                }
                List<byte> bytes_dataItem = new List<byte>()
                {
                    0xff, //Return code: Success
                    (byte)parameter.DataVarType,
                    (byte)(parameter.DataBytes.Length * bit / 256), //数据响应长度 [Hi]
                    (byte)(parameter.DataBytes.Length * bit % 256)  //[Lo]
                };
                bytes_dataItem.AddRange(parameter.DataBytes);
                //如果数据响应长度是奇数，要补偿一个字节的FillByte。例外情况是：如果是最后一个元素，则不需要补偿。
                if ((parameter.DataBytes.Length % 2) > 0 && i < variables.Length - 1)
                {
                    bytes_dataItem.Add(0x00);
                }
                datas.AddRange(bytes_dataItem);
            }
            return datas.ToArray();
        }

        /// <summary>
        /// 将字节数组按照泛型传入的数据类型，进行数据转换。
        /// 注意：此方法仅支持转换一个数据类型，不支持对包含多个数据类型的字节数组转换。
        /// </summary>
        /// <typeparam name="T">泛型指定要转换的数据类型</typeparam>
        /// <param name="bytes">数据转换源</param>
        /// <param name="count">指定转换数据的个数。如果转换布尔类型，必须指定Count。其他类型可以不指定。</param>
        /// <returns>返回转换的数据值</returns>
        private List<T> BytesToData<T>(byte[] bytes, int count = 0)
        {
            List<T> data = new List<T>();
            if (typeof(T) == typeof(bool))
            {
                if (count <= 0)
                {
                    throw new Exception($"字节转换数据类{typeof(T).Name}失败，必须指定转换布尔值的个数！");
                }
                foreach (byte byteVal in bytes)
                {
                    //for (int i = 0; i < 8; i++)
                    //{
                    //    dynamic bit = (byteVal & (1 << i)) > 0;
                    //    data.Add(bit);
                    //    if (data.Count >= count)
                    //    {
                    //        return data;
                    //    }
                    //}
                    //上面代码注释掉，因为西门子S7只能写入一个布尔值，并且一个布尔值占一个字节，所以不需要使用位操作合并多个布尔值到一个字节
                    dynamic b = byteVal == 0x01 ? true : false;
                    data.Add(b);
                }
                throw new Exception($"字节转换数据类{typeof(T).Name}失败");
            }
            // 字符串在S7设备中按如下格式存储： __XXXXXXXXXXX
            // 其中第一个字节表示字符串的有效空间，即一个报文最多可以存储多少个字符
            // 第二个字节表示有效字符的字节数，即已经存了多少个字符
            // 注意：这个处理逻辑不支持200Smart的字符串处理
            else if (typeof(T) == typeof(string))
            {
                byte[] str_bytes = bytes.ToList().GetRange(2, bytes[1]).ToArray();
                dynamic d = Encoding.UTF8.GetString(str_bytes);
                data.Add(d);
                return data;
            }
            else
            {
                int typeSize = Marshal.SizeOf<T>();

                //通过反射，得到类BitConverter提供的指定T返回值的数据类型转换方法
                MethodInfo[] methods = typeof(BitConverter).GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                MethodInfo method = methods.FirstOrDefault(m => m.ReturnType == typeof(T) && m.GetParameters().Count() == 2);
                if (method == null)
                {
                    throw new Exception("数据类型转换出错！未找到匹配的数据转换方法！");
                }
                //根据泛型指定类型的字节长度对bytes字节数组提取出每个泛型的字节
                List<byte> byteList = bytes.ToList();
                try
                {
                    for (int i = 0; i < bytes.Length; i += typeSize)
                    {
                        var bytesToT = byteList.GetRange(i, typeSize).ToArray();
                        if (BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(bytesToT);
                        }
                        //进行数据类型转换

                        T value = (T)method.Invoke(null, new object[] { bytesToT, 0 });
                        data.Add(value);
                    }
                }
                catch
                {
                    throw new Exception("数据类型转换出错！未找到匹配的数据转换方法！");
                }
                return data;
            }
        }

        /// <summary>
        /// 将泛型指定数据类型的数据转换为字节数组。
        /// </summary>
        /// <typeparam name="T">泛型指定要转换的数据类型</typeparam>
        /// <param name="values">数据转换源</param>
        /// <returns>返回转换后的字节数组</returns>
        private byte[] DataToBytes<T>(params T[] values)
        {
            List<byte> bytes = new List<byte>();
            if (typeof(T) == typeof(bool))
            {
                //byte newByte = 0x00;
                for (int i = 0; i < values.Length; i++)
                {
                    bool b = bool.Parse(values[i].ToString());
                    //if (b)
                    //{
                    //    newByte |= (byte)(1 << (i % 8));
                    //}
                    //if (i % 8 == 7)
                    //{
                    //    bytes.Add(newByte);
                    //    newByte = 0x00;
                    //}
                    //上面代码注释掉，因为西门子S7只能写入一个布尔值，并且一个布尔值占一个字节，所以不需要使用位操作合并多个布尔值到一个字节
                    byte @byte = (byte)(b ? 0x01 : 0x00);
                    bytes.Add(@byte);
                }
                //if (values.Length % 8 != 0)
                //{
                //    bytes.Add(newByte);
                //}
                return bytes.ToArray();
            }
            else if (typeof(T) == typeof(string))
            {
                string str = string.Join("", values);
                if (str.Length > 254)
                {
                    throw new Exception("转换字符串长度不能超过254！");
                }
                byte[] str_bytes = Encoding.UTF8.GetBytes(str);
                //虽然PLC存储字符串时，需要在目标字符数据前添加2个字节（字符串有效空间和有效字符字节数），但是因为写数据的方法中会添加这2个字节，所以在此转换方法中不需要添加，否则会出现重复添加。
                //bytes.Add(0xff); //表示字符串的有效空间，即一个报文最多可以存储多少个字符
                //bytes.Add((byte)str_bytes.Length); //表示有效字符的字节数，即要存多少个字符
                bytes.AddRange(str_bytes);
            }
            else
            {
                try
                {
                    foreach (var val in values)
                    {
                        dynamic d = val;
                        byte[] bytesToValue = BitConverter.GetBytes(d);
                        if (BitConverter.IsLittleEndian)
                        {
                            Array.Reverse(bytesToValue);
                        }
                        bytes.AddRange(bytesToValue);
                    }
                }
                catch
                {
                    throw new Exception("数据类型转换出错！");
                }
            }
            return bytes.ToArray();
        }

        /// <summary>
        /// 向设备发送请求。
        /// </summary>
        /// <param name="bytesToRequest">请求的字节数组</param>
        /// <returns>返回响应的字节数组</returns>
        private byte[] SendAndReceive(byte[] bytesToRequest)
        {
            _socke.Send(bytesToRequest);

            byte[] bytesToResponse = new byte[4]; //TPKT 4个字节
            _socke.Receive(bytesToResponse);

            //整个请求字节数
            int totalLength = bytesToResponse[2] * 256 + bytesToResponse[3];
            //排除TPKT头的其他字节数
            int len = totalLength - 4;
            bytesToResponse = new byte[len];

            _socke.Receive(bytesToResponse, 0, bytesToResponse.Length, SocketFlags.None);

            return bytesToResponse;
        }

        // 只对一个地址进行处理  
        // DB1.DBW100
        // Read("DB1.DBW100",2)
        /// I      I0.0         IB0        IW0        ID0
        /// Q      Q0.0         QB.....
        /// M
        /// V      V10.5        VB10       VW10       VD10
        /// DB     DB1.DBX0.0   DB1.DBB0   DB1.DBW0   DB1.DBD0
        private Dictionary<string, S7_ParameterVarType> ParamVarDict = new Dictionary<string, S7_ParameterVarType>
        {
            { "X",S7_ParameterVarType.BIT },
            { "B",S7_ParameterVarType.BYTE },
            { "W",S7_ParameterVarType.WORD },
            { "D",S7_ParameterVarType.DWORD },
        };
    }
}
