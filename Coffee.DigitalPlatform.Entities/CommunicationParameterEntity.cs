using Coffee.DigitalPlatform.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Entities
{
    public class CommunicationParameterEntity
    {
        [Column(name: "prop_name")]
        public string PropName { get; set; }

        [Column(name: "prop_value")]
        public string PropValue { get; set; }

        [Column(name: "prop_type")]
        public string PropValueType { get; set; }
    }

    public class CommunicationParameterDefinitionEntity
    {
        [Column(name: "id")]
        public int Id { get; set; }

        // 用来显示  "串口名称" 
        [Column(name: "p_header")]
        public string Label { get; set; }

        // 用来保存  "PortName"
        [Column(name: "p_name")]
        public string ParameterName { get; set; }

        // 通信参数值的输入方式   0表示键盘输入   1表示下拉选择
        [Column(name: "p_input_type")]
        public int ValueInputType { get; set; }

        [Column(name: "p_data_type")]
        public Type ValueDataType { get; set; }

        // 通信参数的默认值
        [Column(name: "p_default")]
        public string DefaultValueOption { get; set; }

        //如果输入方式是Selector，则DefaultOptionIndex为待选选项默认选中的索引
        public int DefaultOptionIndex { get; set; }

        //是否为默认参数
        [Column(name: "is_default")]
        public bool IsDefaultParameter { get; set; }

        //指示当前通信参数属于通信协议（某个通信参数既可以应用于Modbus协议，也可以应用于西门子S7协议）
        [NotMapped]
        public IList<string> ProtocolNames { get; set; }
    }

    public class CommunicationParameterOptionEntity
    {
        [Column(name: "id")]
        public int Id { get; set; }

        [Column(name: "prop_name")]
        public string PropName { get; set; }

        [Column(name: "prop_option_value")]
        public string PropOptionValue { get; set; }

        [Column(name: "prop_option_label")]
        public string PropOptionLabel { get; set; }
    }
}
