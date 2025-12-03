using Coffee.DigitalPlatform.Common;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Models
{
    public class Variable : ObservableObject
    {
        // 设备编码
        public string DeviceNum { get; set; }
        // 变量编码
        public string VarNum { get; set; }
        // 变量名称
        public string VarName { get; set; }
        // 变量地址
        public string VarAddress { get; set; }
        // 变量类型
        public VariableType VariableType { get; set; }

        // 变量值
        private object _value;
        public object Value
        {
            get { return _value; }
            set
            {
                if (_value == value)
                    return;
                SetProperty(ref _value, value);
            }
        }

        // 偏移量
        public double Offset { get; set; }
        // 系数
        public double Factor { get; set; } = 1;
    }
}
