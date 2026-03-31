using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.Omron.Communication.Base
{
    public class FINS_Parameter
    {
        public Area Area { get; set; }
        public int WordAddr { get; set; }
        public byte BitAddr { get; set; } = 0;
        public DataTypes DataType { get; set; } = DataTypes.WORD;
        public ushort Count { get; set; } = 1;

        public byte[] Data { get; set; }
    }
}
