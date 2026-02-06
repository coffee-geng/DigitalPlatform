using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.ModbusLib
{
    public class ReadWriteModbusCallbackResult
    {
        public byte[] ResultData { get; set; }

        public bool IsCompleted {  get; set; }

        public Exception Error { get; set; }
    }
}
