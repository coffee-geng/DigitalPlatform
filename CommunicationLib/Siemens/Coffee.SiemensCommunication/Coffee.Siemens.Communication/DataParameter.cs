using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.Siemens.Communication
{
    public class DataParameter
    {
        public DataParameter(S7_Functions function)
        {
            _function = function;
        }

        private readonly S7_Functions _function;

        /// <summary>
        /// 数据存储区域。
        /// </summary>
        public S7_Areas Area { get; set; }

        /// <summary>
        /// 第几个DB存储区域。V区总是指向第一个DB存储区域。
        /// </summary>
        public ushort DBNumber { get; set; } = 0;

        private S7_ParameterVarType _parameterVarType = S7_ParameterVarType.BYTE;
        /// <summary>
        /// 指定读取数据的数据类型。
        /// </summary>
        public S7_ParameterVarType ParameterVarType
        {
            get { return _parameterVarType; }
            set
            {
                _parameterVarType = value;
                switch (value)
                {
                    case S7_ParameterVarType.BYTE:
                        DataVarType = S7_DataVarType.BYTE;
                        break;
                    case S7_ParameterVarType.WORD:
                        DataVarType = S7_DataVarType.WORD;
                        break;
                    case S7_ParameterVarType.DWORD:
                        DataVarType = S7_DataVarType.DWORD;
                        break;
                    case S7_ParameterVarType.BIT:
                        DataVarType = S7_DataVarType.BIT;
                        break;
                    //不确定下面这些ParameterVarType和DataVarType的对应关系
                    case S7_ParameterVarType.INT:
                    case S7_ParameterVarType.DINT:
                        DataVarType = S7_DataVarType.INTERGER;
                        break;
                    case S7_ParameterVarType.REAL:
                        DataVarType = S7_DataVarType.REAL;
                        break;
                    default:
                        DataVarType = S7_DataVarType.NULL;
                        break;
                }
            }
        }

        /// <summary>
        /// 当读取时，表示响应返回数据的数据类型。与ParameterVarType有区别。
        /// 当写入时，表示写入数据的数据类型。与ParameterVarType有区别。
        /// </summary>
        public S7_DataVarType DataVarType { get; private set; }

        /// <summary>
        /// 读取或写入数据的目标地址（字节地址部分，18-3位）。
        /// </summary>
        public int ByteAddress { get; set; }
        /// <summary>
        /// 读取或写入数据的目标地址（位地址部分，2-0位）。
        /// </summary>
        public byte BitAddress { get; set; }

        /// <summary>
        /// 从指定区域连续读取多少个ParameterVarType指定数据类型的数据。
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// 当读取时，用于存储读取的字节数据。
        /// 当写入时，用于传输写入的字节数据。
        /// </summary>
        public byte[] DataBytes { get; set; }
    }
}
