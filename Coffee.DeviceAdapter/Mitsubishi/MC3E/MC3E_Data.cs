using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DeviceAdapter
{
    public class MC3E_Data : ProtocolData
    {
    }

    public class MC3E_Data<T> : ProtocolData<T>
    {
    }

    public enum ExecuteType
    {
        Normal = 0x01,
        Force = 0x03
    }

    public enum CleanMode
    {
        Normal = 0x00,
        WithoutLock = 0x01,
        All = 0x02
    }
}
