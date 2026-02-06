using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.Omron.Communication.Base
{
    internal class CIP_TypeCode
    {
        public static Dictionary<string, int> TypeLength
            = new Dictionary<string, int>
            {
                { "C1",8},
                { "C2",8},
                { "C3",16},
                { "C4",32},
                { "C5",64},
                { "C6",8},
                { "C7",16},
                { "C8",32},
                { "C9",64},
                { "CA",32},
                { "CB",64},
                { "D0",0},
                { "D1",8},
                { "D2",16},
                { "D3",32},
                { "D4",64},
            };
    }

    //CIP 数据类型
    public enum CIP_DataTypes
    {
        BOOL = 0xC1, //Logical Boolean with values TRUE and FALSE
        SINT = 0xC2, //Signed 8-bit integer value
        INT = 0xC3, //Signed 16-bit integer value
        DINT = 0xC4, //Signed 32-bit integer value
        LINT = 0xC5, //Signed 64-bit integer value
        USINT = 0xC6, //Unsigned 8-bit integer value
        UINT = 0xC7, //Unsigned 16-bit integer value
        UDINT = 0xC8, //Unsigned 32-bit integer value
        ULINT = 0xC9, //Unsigned 64-bit integer value
        REAL = 0xCA, //32-bit floating point value
        LREAL = 0xCB, //64-bit floating point value
        STIME = 0xCC, //Synchrononous time information
        DATE = 0xCD, //Date information
        TIME_OF_DAY = 0xCE, //Time of day
        DATE_AND_TIME = 0xCF, //Date and time of day
        STRING = 0xD0, //character string (1 byte per character)
        BYTE = 0xD1, //bit string - 8-bits
        WORD = 0xD2, //bit string - 16-bits
        DWORD = 0xD3, //bit string - 32-bits
        LWORD = 0xD4, //bit string - 64-bits
        STRING2 = 0xD5, //character string (2 bytes per character)
        FTIME = 0xD6, //Duration (high resolution)
        LTIME = 0xD7, //Duration (long)
        ITIME = 0xD8, //Duration (short)
        STRINGN = 0xD9, //character string (N bytes per character)
        SHORT_STRING = 0xDA, //character string (1 byte per character, 1 byte length indicator)
        TIME = 0xDB, //Duration (milliseconds)
        EPATH = 0xDC, //CIP path segments
        ENGUNIT = 0xDD, //Engineering Units
        STRINGI = 0xDE, //International Character String
    }
}
