using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.Omron.Communication
{
    public abstract class OmronBase
    {
        public virtual void Open(int timeout = 3000) { }
        public virtual void Close() { }

        protected virtual void CheckResponse(byte[] bytes) { }

        // 从字节到数据的转换
        public List<T> GetDatas<T>(byte[] bytes)
        {
            List<T> datas = new List<T>();
            if (typeof(T) == typeof(bool))
            {
                foreach (byte b in bytes)
                {
                    dynamic d = (b == 0x01);
                    datas.Add(d);
                }
            }
            else if (typeof(T) == typeof(string))
            {
                dynamic d = Encoding.UTF8.GetString(bytes);
                datas.Add(d);
            }
            else
            {
                int size = Marshal.SizeOf<T>();

                Type tBitConverter = typeof(BitConverter);
                MethodInfo[] mis = tBitConverter.GetMethods(BindingFlags.Public | BindingFlags.Static);
                if (mis.Count() <= 0) return datas;
                MethodInfo mi = mis.FirstOrDefault(m => m.ReturnType == typeof(T) &&
                                            m.GetParameters().Count() == 2)!;

                for (int i = 0; i < bytes.Length; i += size)
                {
                    byte[] data_bytes = bytes.ToList().GetRange(i, size).ToArray();

                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(data_bytes);
                    }
                    dynamic v = mi.Invoke(tBitConverter, new object[] { data_bytes, 0 })!;
                    datas.Add(v);
                }
            }

            return datas;
        }
        // 从数据到字节的转换
        public byte[] GetBytes<T>(params T[] values)
        {
            List<byte> bytes = new List<byte>();
            if (typeof(T) == typeof(bool))
            {
                foreach (var v in values)
                {
                    bytes.Add((byte)(bool.Parse(v.ToString()) ? 0x01 : 0x00));
                }
            }
            else if (typeof(T) == typeof(string))
            {
                byte[] str_bytes = Encoding.UTF8.GetBytes(values[0].ToString());
                bytes.AddRange(str_bytes);
            }
            else
            {
                foreach (var v in values)
                {
                    dynamic d = v;
                    byte[] v_bytes = BitConverter.GetBytes(d);
                    if (BitConverter.IsLittleEndian)
                    {
                        Array.Reverse(v_bytes);
                    }
                    bytes.AddRange(v_bytes);
                }

            }

            return bytes.ToArray();
        }
    }
}
