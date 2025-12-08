using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Models
{
    public class DeviceProperty
    {
        // 属性的名称  通信协议（Header）  Protocol（Name）    -》 对接通信异构平台   名称 进行对比
        public string PropName { get; set; }
        public string PropValue { get; set; }
    }
}
