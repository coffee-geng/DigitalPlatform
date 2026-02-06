using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.Omron.Communication.Base
{
    internal class CIP_Errors
    {
        internal static Dictionary<byte, string> HeaderErrors = new Dictionary<byte, string>
        {
            {0x00,"Success" },
            {0x01,"发件人发出了无效或不受支持的封装命令" },
            {0x02,"接收方中的内存资源不足，无法处理该命令。这不是应用程序错误。相反，仅当封装层无法获取所需的内存资源时，才会出现这种情况" },
            {0x03,"封装消息的数据部分中的数据格式不正确或不正确" },
            {0x64,"发起方在向目标发送封装消息时使用无效的会话句柄" },
            {0x65,"目标收到长度无效的消息" },
            {0x69,"不支持的封装协议修订版" },
        };

        //响应错误码
        public static Dictionary<string, string> RespErrors = new Dictionary<string, string>()
        {
            {"0000","Success" },
            {"0001","Connection failure" },
            {"0002","Resource unavailable" },
            {"0003","Invalid parameter value" },
            {"0004","Path segment error" },
            {"0005","Path destination unknown" },
            {"0006","Partial transfer" },
            {"0007","Connection ID is not valid" },
            {"0008","Service not supported" },
            {"0009","Invalid attribute value" },
            {"000A","Attribute list error" },
            {"000B","Already in requested mode/state" },
            {"000C","Object state conflict" },
            {"000D","Object already exists" },
            {"000E","Attribute not settable" },
            {"000F","Privilege violation" },
            {"0010","Device state conflict" },
            {"0011","Reply data too large" },
            {"0012","Fragmentation of a primitive value" },
            {"0013","Not enough data" },
            {"0014","Attribute not supported" },
            {"0015","Too Much Data" },
            {"0016","Object does not exist" },
            {"0017","Service fragmentation sequence not in progress" },
            {"0018","No stored attribute data" },
            {"0019","Store operation failure" },
            {"001A","Routing failure: request packet too large" },
            {"001B","Routing failure: response packet too large" },
            {"001C","Missing attribute list entry data" },
            {"001D","Invalid attribute value list" },
            {"001E","Embedded service error" },
            {"001F","Vendor specific error" },
            {"0020","Invalid parameter" },

            {"0021","Write-once value or medium already written" },
            {"0022","Invalid Reply Received" },
            {"0025","Key Failure in path" },
            {"0026","Path Size Invalid" },
            {"0027","Unexpected attribute in list" },
            {"0028","Invalid Member ID" },
            {"0029","Member not settable" },
            {"002A","The Transaction has Timed Out" },
            {"002B","The Current Operation has Timed Out" },
            {"002C","Session Registration Timed Out" },
            {"002D","Forward Open Command Failed" },
            {"0064","Invalid Session" },
            {"0065","Invalid Length" },
            {"0069","Unsupported Protocol Version" },
            {"006A","Stale Connection" },
            {"00FF","Unknown Error Response" },
            {"0100","Bad Address Structure" },
            {"0101","Invalid Data Type" },
            {"0102","Get Attribute Failed" },
            {"0103","PLC Config Changed: Tags Uploading" },
            {"0104","PLC Tag Upload Not Complete" },
            {"0105","Transaction Timeout Count Exceeded" },
            {"0106","Reading PLC Tag Information" },
            {"0107","Reading PLC Structure Information" },
            {"0108","Requested Port Semaphore Timed Out" },
            {"0109","Command Response Mismatch" },
            {"010A","No Port Tag defined" },
            {"010B","Port Disconnect delay < Session Timeout" },
            {"0209","Attempting To Send Invalid Read Msg To PLC" },
            {"020A","Attempting To Send Invalid Write Msg To PLC" },
            {"0301","No Connection Buffer Memory Available" },
            {"031C","Miscellaneous connection error" }
        };
    }
}
