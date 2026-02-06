using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.Siemens.Communication
{
    public static class ResponseStatus
    {
        public static Dictionary<byte, string> ErrorClasses = new Dictionary<byte, string>()
        {
            { 0x00, "无错误" },
            { 0x00, "应用程序关系错误" },
            { 0x00, "对象定义错误" },
            { 0x00, "无资源可用错误" },
            { 0x00, "服务处理错误" },
            { 0x00, "请求错误（如果有错，此码较多）" },
            { 0x00, "访问错误" }
        };

        public static Dictionary<ushort, string> ErrorCodes = new Dictionary<ushort, string>()
        {
            { 0x0110,"无效块类型编号" },
            { 0x0112,"无效参数" },
            { 0x011A,"PG资源错误" },
            { 0x011B,"PLC重新外包错误" },
            { 0x011C,"协议错误" },
            { 0x011F,"用户缓冲区太短" },
            { 0x0141,"请求错误" },
            { 0x01C0,"版本不匹配" },
            { 0x01F0,"末实施" },
            { 0x8001,"L7无效CPU状态" },
            { 0x8500,"L7PDU大小错误" },
            { 0xD401,"L7无效SZL ID" },
            { 0xD402,"L7无效索引" },
            { 0xD403,"L7 DGS连接已宣布" },
            { 0xD404,"L7 最大用户NB" },
            { 0xD405,"L7 DGS功能参数语法错误" },
            { 0xD406,"L7无信息" },
            { 0xD601,"L7 PRT 函数参数语法错误" },
            { 0xD801,"L7 无效变量地址" },
            { 0xD802,"L7 未知请求" },
            { 0xD803,"L7 无效请求状态" },
        };

        public static Dictionary<byte, string> DataReturnCodes = new Dictionary<byte, string>()
        {
            { 0xff,"请求成功"},
            { 0x00, "未定义，预留" },
            { 0x01,"硬件错误"},
            { 0x03,"对象不允许访问"},
            { 0x05,"地址越界，所需的地址超出此PLC的极限"},
            { 0x06,"请求的数据类型与存储类型不一致"},
            { 0x07,"日期类型不一致"},
            { 0x0a,"对象不存在"}
        };
    }
}
