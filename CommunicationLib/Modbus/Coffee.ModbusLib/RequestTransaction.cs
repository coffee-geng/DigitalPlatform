using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.ModbusLib
{
    internal class RequestTransaction
    {
        public int TransactionId {  get; set; }

        public byte[] RequestBytes { get; set; }

        public int ResponseLength {  get; set; }

        public Functions RequestFunction { get; set; }

        public TimeSpan OperationTime {  get; set; }

        public Action<ReadWriteModbusCallbackResult> Completed { get; set; }
    }
}
