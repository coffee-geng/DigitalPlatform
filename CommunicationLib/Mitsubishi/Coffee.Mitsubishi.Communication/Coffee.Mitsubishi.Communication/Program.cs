using System.Net.Sockets;
using Coffee.Mitsubishi.Base;

namespace Coffee.Mitsubishi.Communication
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello, World!");

            CustomLibTest();

            Console.ReadLine();
        }

        // 第三方通信库演示：检验仿真环境是否可用
        static void MCProtocolLibTest()
        {
            MCProtocol.Mitsubishi.McProtocolTcp mc
                = new MCProtocol.Mitsubishi.McProtocolTcp(
                    "192.168.174.128",
                    6000,
                    MCProtocol.Mitsubishi.McFrame.MC3E);
            mc.Open().GetAwaiter().GetResult();

            int[] datas = new int[3];
            byte[] resp = mc.ReadDeviceBlock(MCProtocol.Mitsubishi.PlcDeviceType.D,
                 10, 3, datas).GetAwaiter().GetResult();

            mc.WriteDeviceBlock(MCProtocol.Mitsubishi.PlcDeviceType.D,
                20, 2, new int[] { datas[0], datas[1] });

            Console.WriteLine(string.Join(" ", resp.Select(r => r.ToString("X2"))));
            Console.WriteLine(string.Join(" ", datas.Select(d => d)));
        }

        static void CustomLibTest()
        {
            Mc3E mc3E = new Mc3E("192.168.174.128", 6000);
            mc3E.Open();

            // 批量读取测试--------------------------------------------
            //byte[] result = mc3E.Read(Base.Areas.D, "100", 10);
            // 007B  

            //byte[] result = mc3E.Read(Base.Areas.X, "100", 10, Base.RequestType.BIT);
            //byte[] result = mc3E.Read(Base.Areas.X, "1A0", 3, Base.RequestType.BIT);
            // 10个状态

            //Console.WriteLine(string.Join(" ", result.Select(b => b.ToString("X2"))));

            // 批量写入测试----------------------------------------------
            //mc3E.Write(new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 },
            //    Base.Areas.X, "8", Base.RequestType.BIT);
            //mc3E.Write(new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 },
            //    Base.Areas.D, "8", Base.RequestType.WORD);
            //mc3E.Write(new byte[] { 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 },
            //    Base.Areas.Y, "abde", Base.RequestType.WORD);

            // 随机读取测试----------------------------------------------
            //List<DataParameter> w = new List<DataParameter>
            //{
            //    new DataParameter{ Area=Areas.D,Address="100"},
            //    new DataParameter{ Area=Areas.X,Address="10"},// 地址按照16的倍数进行指定
            //};
            //List<DataParameter> dw = new List<DataParameter>
            //{
            //    new DataParameter{ Area=Areas.D,Address="120"},
            //    new DataParameter{ Area=Areas.M,Address="16"},// 地址按照16的倍数进行指定
            //};
            //mc3E.RandomRead(w, dw);


            // 随机位写入测试----------------------------------------------
            //List<DataParameter> bit = new List<DataParameter>
            //{
            //    new DataParameter{ Area=Areas.X,Address="1A",Datas=new List<byte>{ 0x01} },
            //    new DataParameter{ Area=Areas.M,Address="10",Datas=new List<byte>{ 0x01} },// 地址按照16的倍数进行指定
            //};
            //mc3E.RandomWriteBit(bit);

            // 随机字写入测试----------------------------------------------
            //List<DataParameter> word = new List<DataParameter>
            //{
            //    new DataParameter{ Area=Areas.D,Address="10",Datas=new List<byte>{ 0x01,0x02} },
            //    new DataParameter{ Area=Areas.M,Address="32",Datas=new List<byte>{ 0x01,0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 } },// 地址按照16的倍数进行指定
            //};
            //List<DataParameter> dword = new List<DataParameter>
            //{
            //    new DataParameter{ Area=Areas.D,Address="20",Datas=new List<byte>{ 0x01,0x02,0x03,0x04} },
            //    new DataParameter{ Area=Areas.X,Address="20",Datas=new List<byte>{ 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01, 0x01 } },// 地址按照16的倍数进行指定
            //};

            //mc3E.RandomWrite(word, dword);


            // 多块成批读取测试
            //List<DataParameter> word = new List<DataParameter>
            //{
            //    new DataParameter{ Area=Areas.D,Address="10",Count=3},
            //    new DataParameter{ Area=Areas.M,Address="0",Count=2},// 地址按照16的倍数进行指定
            //};
            //List<DataParameter> bit = new List<DataParameter>
            //{
            //    new DataParameter{ Area=Areas.Y,Address="0",Count=2 },
            //    new DataParameter{ Area=Areas.X,Address="20",Count=1},// 地址按照16的倍数进行指定
            //};
            //mc3E.MultiBlockRead(word, bit);

            //List<DataParameter> word = new List<DataParameter>
            //{
            //    new DataParameter{ Area=Areas.D,Address="10",Datas=new List<byte>{ 0x01,0x02,0x03,0x04} },
            //    new DataParameter{ Area=Areas.M,Address="0",Datas=new List<byte>{ 0xFF,0xFF} },// 地址按照16的倍数进行指定
            //};
            //List<DataParameter> bit = new List<DataParameter>
            //{
            //    new DataParameter{ Area=Areas.Y,Address="0",Datas=new List<byte>{ 0xFF,0xFF} },
            //    new DataParameter{ Area=Areas.X,Address="20",Datas=new List<byte>{ 0xFF,0xFF} },// 地址按照16的倍数进行指定
            //};
            //mc3E.MultiBlockWrite(word, bit);


            // 数据转换测试
            //List<ushort> values = new List<ushort>() {
            //    111,222,333,444
            //};

            //byte[] v_bytes = mc3E.GetBytes<ushort>(values.ToArray());
            //mc3E.Write(v_bytes, Areas.D, "100");


            //byte[] v_bytes = mc3E.Read(Areas.D, "100", 4);
            //byte[] v_bytes = mc3E.Read("D100", 4);
            //byte[] v_bytes = mc3E.Read("X1A", 16, RequestType.BIT);
            //var vs = mc3E.GetDatas<bool>(v_bytes);



            //mc3E.PlcRun(cm: CleanMode.All);
            //mc3E.PlcStop();
        }
    }
}
