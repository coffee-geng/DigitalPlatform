using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.Omron.Communication.Base
{
    //枚举中定义的值都是数据类型是Bit，如果数据类型是Word，则是当前枚举中 + 0x80
    public enum Area
    {
        CIO = 0x30, //CIO Area
        WR = 0x31,  //Work Area
        HR = 0x32,  //Holding Bit Area
        AR = 0x33,  //Auxiliary Bit Area
        DM = 0x02,  //DM Area
        TK = 0x06,  //Task Flag
        IR = 0xDC,  //Index Register
        DR = 0xBC   //Data Register
    }
}
