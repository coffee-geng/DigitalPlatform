using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.ModbusLib
{
    public enum EndianTypes
    {
        ABCD, 
        CDAB, 
        BADC, 
        DCBA,
        ABCDEFGH, 
        GHEFCDAB, 
        BADCFEHG, 
        HGFEDCBA
    }
}
