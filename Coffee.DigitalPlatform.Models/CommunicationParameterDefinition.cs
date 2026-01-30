using Coffee.DigitalPlatform.Common;
using Coffee.DigitalPlatform.CommWPF;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Models
{
    public class CommunicationParameter : ObservableObject, ISaveState
    {
        private string _propName;
        public string PropName
        {
            get { return _propName; }
            set
            {
                if (SetProperty(ref _propName, value))
                {
                    _isDirty = true;
                }
            }
        }

        private string _propValue;
        public string PropValue
        {
            get { return _propValue; }
            set
            {
                if (SetProperty(ref _propValue, value))
                {
                    _isDirty = true;
                }
            }
        }

        private Type _propValueType;
        public Type PropValueType
        {
            get { return _propValueType; }
            set
            {
                if (SetProperty(ref _propValueType, value))
                {
                    _isDirty = true;
                }
            }
        }

        #region ISaveState 接口实现
        private bool _isDirty = false;
        public bool IsDirty
        {
            get { return _isDirty; }
        }

        public void Save()
        {
            _isDirty = false;
        }
        #endregion
    }

    public class CommunicationParameterDefinition
    {
        // 用来显示  "串口名称" 
        public string Label { get; set; }
        // 用来保存  "PortName"
        public string ParameterName { get; set; }

        // 通信参数值的输入方式   0表示键盘输入   1表示下拉选择
        public ValueInputTypes ValueInputType { get; set; }

        //通信参数值的数据类型
        public Type ValueDataType { get; set; }

        //如果输入方式是Selector，则ValueOptions为待选选项
        public List<CommunicationParameterOption> ValueOptions { get; set; }

        //通信参数的默认值
        public string DefaultValueOption { get; set; }

        //如果输入方式是Selector，则DefaultOptionIndex为待选选项默认选中的索引
        public int DefaultOptionIndex { get; set; }
    }

    public class CommunicationParameterOption
    {
        public string PropName {  get; set; }

        public string PropOptionValue { get; set; }

        public string PropOptionLabel { get; set; }
    }
}
