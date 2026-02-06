using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.Omron.Communication.Base
{
    public class CIP_Parameter
    {
        public string Tag { get; set; }
        public byte[] Data { get; set; }
        public Exception Error { get; set; }
    }
}
