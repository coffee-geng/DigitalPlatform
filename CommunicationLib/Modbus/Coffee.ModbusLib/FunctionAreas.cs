using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.ModbusLib
{
    public enum FunctionAreas
    {
        CoilsState = 0,
        InputCoils = 1,
        InputRegister = 3,
        HoldingRegister = 4
    }

    public enum FunctionCodes
    {
        ReadCoilsState = 0x01,
        ReadInputCoils = 0x02,
        ReadHoldingRegister = 0x03,
        ReadInputRegister = 0x04,
        WriteSingleCoilsState = 0x05,
        WriteSingleHoldingRegister = 0x06,
        WriteCoilsState = 0x0F,
        WriteHoldingRegister = 0X10
    }

    public enum Functions
    {
        Read,
        Write, //写入一个或多个数据
        WriteSingle, //只写入一个数据
    }
}
