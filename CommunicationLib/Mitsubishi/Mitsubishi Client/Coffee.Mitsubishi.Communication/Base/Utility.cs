using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.Mitsubishi.Base
{
    internal class Utility
    {
    }

    public static class OctalConverter
    {
        public static byte[] OctalStringToByteArray(string octalString)
        {
            // 验证输入
            if (string.IsNullOrWhiteSpace(octalString))
                throw new ArgumentException("输入字符串不能为空");

            // 验证八进制格式
            foreach (char c in octalString)
            {
                if (c < '0' || c > '7')
                    throw new FormatException($"无效的八进制字符: {c}");
            }

            // 使用 BigInteger 进行转换
            BigInteger bigInt = BigInteger.Zero;

            foreach (char c in octalString)
            {
                int digit = c - '0';
                bigInt = bigInt * 8 + digit;
            }

            // 转换为字节数组（大端序）
            byte[] bytes = bigInt.ToByteArray();

            // 如果结果是负数（由于符号位），需要处理
            if (bytes.Length > 0 && bytes[bytes.Length - 1] == 0)
            {
                Array.Resize(ref bytes, bytes.Length - 1);
            }

            // 反转数组为大端序（如果需要小端序，可以省略这一步）
            Array.Reverse(bytes);

            return bytes;
        }
    }
}
