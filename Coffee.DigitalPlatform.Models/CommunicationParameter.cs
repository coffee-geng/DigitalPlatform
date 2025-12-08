using Coffee.DigitalPlatform.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Models
{
    public class CommunicationParameter
    {
        // 用来显示  "串口名称" 
        public string Label { get; set; }
        // 用来保存  "PortName"
        public string ParameterName { get; set; }

        // 通信参数值的输入方式   0表示键盘输入   1表示下拉选择
        public ValueInputTypes ValueInputType { get; set; }

        //如果输入方式是Selector，则ValueOptions为待选选项
        public List<string> ValueOptions { get; set; }

        //如果输入方式是Selector，则DefaultOptionIndex为待选选项默认选中的索引
        public int DefaultOptionIndex { get; set; }
    }
}
