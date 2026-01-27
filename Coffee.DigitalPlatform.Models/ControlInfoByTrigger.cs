using Coffee.DigitalPlatform.Controls.FilterBuilder;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Models
{
    public class ControlInfoByTrigger : ObservableObject, IReceiveFilterScheme
    {
        public ControlInfoByTrigger()
        {
            SelectDeviceCommand = new RelayCommand<Device>(doSelectDeviceCommand);
        }

        public int Index { get; set; }

        // 联动控制选项的编号
        public string LinkageNum { get; set; }

        //触发设备联动的条件，可以是单个表达式条件，也可以是多个表达式组合成的条件链
        private ICondition _condition;
        public ICondition Condition
        {
            get { return _condition; }
            set { SetProperty(ref _condition, value); }
        }

        //联动触发时间
        private DateTime? _triggerTime;
        public DateTime? TriggerTime
        {
            get { return _triggerTime; }
            set { SetProperty(ref _triggerTime, value); }
        }

        private string _formattedCondition;
        public string FormattedCondition
        {
            get { return _formattedCondition; }
            set { SetProperty(ref _formattedCondition, value); }
        }

        private Device _conditionDevice;
        public Device ConditionDevice
        {
            get { return _conditionDevice; }
            set { SetProperty(ref _conditionDevice, value); }
        }

        // 联动控制设备的编码
        private Device _device;
        public Device Device
        {
            get { return _device; }
            set { SetProperty(ref _device, value); }
        }

        private string _header;
        public string Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        private Variable _variable;
        public Variable Variable
        {
            get => _variable;
            set => SetProperty(ref _variable, value);
        }

        private object _value;
        public object Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        #region 联动控制选项的编辑状态的属性

        //是否是新建联动控制信息
        public bool IsFirstEditing { get; set; } = true;

        //如果正在编辑，则展开Expander以便显示编辑区域
        //否则，收缩Expander以便隐藏编辑区域
        private bool _isEditing = false;
        public bool IsEditing
        {
            get { return _isEditing; }
            set { SetProperty(ref _isEditing, value); }
        }

        private FilterSchemeEditInfo _conditionTemplate;
        public FilterSchemeEditInfo ConditionTemplate
        {
            get { return _conditionTemplate; }
            set { SetProperty(ref _conditionTemplate, value); }
        }

        private Device _newDevice;
        public Device NewDevice
        {
            get { return _newDevice; }
            set { SetProperty(ref _newDevice, value); }
        }

        private string _newHeader;
        public string NewHeader
        {
            get => _newHeader;
            set => SetProperty(ref _newHeader, value);
        }

        private Variable _newVariable;
        public Variable NewVariable
        {
            get => _newVariable;
            set => SetProperty(ref _newVariable, value);
        }

        private object _newValue;
        public object NewValue
        {
            get => _newValue;
            set => SetProperty(ref _newValue, value);
        }

        public RelayCommand<Device> SelectDeviceCommand { get; set; }

        private void doSelectDeviceCommand(Device device)
        {
            if (device == null)
                return;
            this.NewDevice = device;
        }
        #endregion
    }
}
