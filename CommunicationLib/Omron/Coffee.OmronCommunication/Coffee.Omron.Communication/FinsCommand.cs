using Coffee.Omron.Communication.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.Omron.Communication
{
    public class FinsCommand : OmronBase
    {
        // CIO0.15    CIO10
        // IR10
        // DR10
        // TK10
        // D100       D10.15
        // H0.5       H10
        // W0.5       W10
        // A0.5       A10
        protected FINS_Parameter GetAddress(string variable)
        {
            Area area = Area.DM;
            ushort word_addr = 0;
            byte bit_addr = 0;
            DataTypes dataType = DataTypes.BIT;

            string area_str = variable.Substring(0, 3).ToUpper();
            if (area_str == "CIO") //字和位操作
            {
                dataType = DataTypes.WORD;
                area = Area.CIO;// 相关的存储区按照Word进行处理
                string addr_str = variable.Substring(3).ToUpper();
                string[] addrs = addr_str.Split(".");

                if (!ushort.TryParse(addrs[0], out word_addr))
                {
                    throw new Exception($"地址格式不正确：{variable}");
                }
                if (addrs.Length > 2)
                {
                    throw new Exception($"地址格式不正确：{variable}");
                }
                else if (addrs.Length == 2)
                {
                    if (!byte.TryParse(addrs[1], out bit_addr))
                    {
                        throw new Exception($"地址{variable}是非法格式！");
                    }
                    dataType = DataTypes.BIT;
                }
            }
            else if (new string[] { "IR", "DR", "TK" }.Contains(variable.Substring(0, 2).ToUpper()))
            {
                var a_str = variable.Substring(0, 2).ToUpper();
                area = (Area)Enum.Parse(typeof(Area), a_str);
                if (!ushort.TryParse(variable.Substring(2), out word_addr))
                {
                    throw new Exception($"地址格式不正确：{variable}");
                }
            }
            else if ("ADHW".Contains(variable.Substring(0, 1).ToUpper()))
            {
                var a_str = variable.Substring(0, 1).ToUpper();
                if (a_str == "D")
                    a_str = a_str + "M";
                else
                    a_str = a_str + "R";

                // 默认情况下使用字请求
                area = (Area)Enum.Parse(typeof(Area), a_str);
                dataType = DataTypes.WORD;

                string[] addrs = variable.Substring(1).Split(".");
                if (!ushort.TryParse(addrs[0], out word_addr))
                {
                    throw new Exception($"地址格式不正确：{variable}");
                }

                if (addrs.Length > 2)
                {
                    throw new Exception($"地址格式不正确：{variable}");
                }
                else if (addrs.Length == 2)
                {
                    if (!byte.TryParse(addrs[1], out bit_addr))
                    {
                        throw new Exception($"地址{variable}是非法格式！");
                    }
                    dataType = DataTypes.BIT;
                }
            }
            else
                throw new Exception($"地址格式不正确：{variable}");

            return new FINS_Parameter
            {
                Area = area,
                WordAddr = word_addr,
                BitAddr = bit_addr,
                DataType = dataType
            };
        }
    }
}
