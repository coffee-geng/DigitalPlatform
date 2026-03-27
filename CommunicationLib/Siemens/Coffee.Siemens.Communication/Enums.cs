using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.Siemens.Communication
{
    public enum S7_ROSCTR
    {
        JobRequest = 0x01, //主站发送请求
        Ack = 0x02, //从站响应请求不带数据（没有专门的Data部分）
        Ack_Data = 0x03, //从站响应请求并带有数据（带专门的Data部分报文）
        Userdata = 0x07  //原始协议的扩展。读取编程/调试、SZL读取、安全功能、时间设置等
    }

    public enum S7_PDUTypes
    {
        ConnectRequest = 0xe0, //连接请求
        ConnectConfirm = 0xd0, //连接确认
        ConnectCancel = 0x08, //断开请求
        CancelConfirm = 0x0c, //断开确认
        AccessReject = 0x05, //拒绝访问
        ExpeditedData = 0x01, //加急数据
        ExpeditedDataConfirm = 0x02, //加急数据确认
        Userdata = 0x04, //用户数据
        TPDU_Error = 0x07, //TPDU错误
        DataTransfer = 0xf0 //数据传输
    }

    public enum S7_Areas
    {
        SystemInfo_200Family = 0x03, //System info of 200 family | 200系列系统信息
        SystemFlag_200Family = 0x05, //System flags of 200 family | 200系列系统标志
        AI = 0x06, //Analog inputs of 200 family | 200系列模拟量输入
        AQ = 0x07, //Analog outputs of 200 family | 200系列模拟量输出
        P = 0x80, //Direct peripheral access (P) | 直接访问外设
        I = 0x81, //Inputs (I) | 输入
        Q = 0x82, //Outputs (Q) | 输出（Q）
        M = 0x83, //M
        DB = 0x84, //Data blocks (DB) | 数据块（DB）  V
        DI = 0x85, //Instance data blocks (DI) | 背景数据块（DI）
        L = 0x86,   //Local data (L) | 局部变量（L）
        V = 0x87,   //Unknown yet (V) | 全局变量（V）
        C = 0x1c,   //S7 counters (C) | S7计数器（C）
        T = 0x1d,   //S7 timers (T) | S7定时器（T）
        IEC_Counters, //IEC counters (200 family) | IEC计数器（200系列）
        IEC_Timers  //IEC timers (200 family) | IEC定时器（200系列）
    }

    public enum S7_ParameterVarType
    {
        BIT = 0x01,
        BYTE = 0x02,
        CHAR = 0x03,
        WORD = 0x04,
        INT = 0x05,
        DWORD = 0x06,
        DINT = 0x07,
        REAL = 0x08,
        DATE = 0x09,
        TOD = 0x0A, //Time of day 32位
        TIME = 0x0B, //IEC时间32位
        S5TIME = 0x0C, //Simatic时间16位
        DATE_AND_TIME = 0x0F,
        COUNTER = 0x1C,
        TIMER = 0x1D,
        IEC_TIMER = 0x1E,
        IEC_COUNTER = 0x1F,
        HS_COUNTER = 0x20
    }

    public enum S7_DataVarType
    {
        NULL = 0x00,
        BIT = 0x03,
        BYTE = 0x04,
        WORD = 0x04,
        DWORD = 0x04,
        INTERGER = 0x05,
        REAL = 0x07,
        OCTETSTRING = 0x09
    }

    public enum S7_Functions
    {
        CPU_Service = 0x00,
        SetCommunication = 0xF0,
        ReadVariable = 0x04,
        WriteVariable = 0x05,
        RequestDownload = 0x1A,
        DownloadFileBlock = 0x1B,
        EndDownload = 0x1C,
        StartUpload = 0x1D,
        Upload = 0x1E,
        EndUpload = 0x1F,
        PLC_Start = 0x28,
        PLC_Stop = 0x29
    }

    public enum S7_SyntaxIds
    {
        S7ANY = 0x10,
        PBC_R_ID = 0x13,
        ALARM_LOCKFREE = 0x15,
        ALARM_IND = 0x16,
        ALARM_ACK = 0x19,
        ALARM_QUERYREQ = 0x1a,
        NOTIFY_IND = 0x1c,
        DRIVEESANY = 0xa2,
        _1200SYM = 0xb2,
        DBREAD = 0xb0,
        NCK = 0x82
    }

    public enum S7_Userdatas
    {
        Mode_Transition = 0x00, //转换工作模式
        Programmer_Commands = 0x01, //工程师命令调度
        Cyclic_Data = 0x02, //循环读取
        Block_Functions = 0x03, //块功能
        CPU_Functions = 0x04, //CPU功能
        Security = 0x05, //安全功能
        PBC = 0X06, //BSEND/BRECV
        Time_Functions = 0x07, //时间功能
        NC_Programming = 0x0f //NC编程
    }

    public enum S7_PIServiceNames
    {
        _INSE, //PI-Service_INSE(Activates a PLC module)。激活设备上下载的块，参数是块的名称
        _DELE, //工程师命令调度（Programmer commands）。从设备的文件系统中删除一个块，该参数是块的名称
        P_PROGRAM, //循环读取（Cyclic data）。设置设备的运行状态（启动、停止、复位）
        _MODU, //块功能（Block functions）。压缩PLC内存
        _GARB  //CPU功能（CPU functions）。将RAM复制到ROM，参数包含文件系统标识符（A/E/P）
    }

    public enum S7_FileSystemModule
    {
        P, //被动模块。Passive （copied,but not chained) module
        A, //有源嵌入式模块。Active embedded module
        B //有源和无源模块。Active as well as passive module
    }
}
