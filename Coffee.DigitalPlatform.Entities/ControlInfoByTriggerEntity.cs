using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Entities
{
    public class ControlInfoByTriggerEntity
    {
        //联动控制选项的编码
        //注意：这个编码在同一个联动控制设备中是唯一的
        [Column(name: "u_num")]
        public string LinkageNum { get; set; }

        //联动控制条件触发源设备的编码
        [Column(name: "c_d_num")]
        public string ConditionDeviceNum { get; set; }

        //联动控制的条件编码
        [Column(name: "c_num")]
        public string ConditionNum { get; set; }

        [Column(name: "c_header")]
        public string Header { get; set; }

        //联动控制设备的编码
        [Column(name: "u_d_num")]
        public string LinkageDeviceNum { get; set; }

        //点位信息的变量编码
        [Column(name: "v_num")]
        public string VarNum { get; set; }

        [Column(name: "c_value")]
        public string Value { get; set; }
    }
}
