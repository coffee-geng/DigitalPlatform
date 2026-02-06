using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.Omron.Communication.Base
{
    //FinsTCP  Header错误码
    internal class FINS_TcpHeaderErrors
    {
        public static Dictionary<byte, string> Errors = new Dictionary<byte, string>
        {
            { 0x00,"正常"},
            { 0x01,"数据头不是FINS或ASCII格式"},
            { 0x02,"数据长度过长" },
            { 0x03,"命令（Header功能码）错误" },
            { 0x20,"连接/通信被占用" },
            { 0x21,"指定的节点已经被连接" },
            { 0x22,"尝试从未描写的IP地址访问受保护节点" },
            { 0x23,"客户端FINS节点地址超出范围" },
            { 0x24,"客户端和服务器正在使用相同的FINS节点地址" },
            { 0x25,"所有可供分配的节点地址都已被使用" }
        };
    }

    //FinsTCP Header功能码
    public enum FinsTcpHeaderCodes
    {
        ClientToServer = 0x00, //客户端到服务端通信
        ServerToClient = 0x01, //服务端到客户端通信
        Fins_Send = 0x02, //FINS帧发送命令
        Fins_SendError = 0x03, //FINS帧发送错误通知命令
        Connection = 0x06 //确立通信连接
    }
}
