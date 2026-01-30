using Coffee.DigitalPlatform.Common;
using Coffee.DigitalPlatform.CommWPF;
using Coffee.DigitalPlatform.Controls.FilterBuilder;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Models
{
    public class ControlInfoByTrigger : ObservableObject, IReceiveFilterScheme, ISaveState
    {
        public ControlInfoByTrigger()
        {
            SelectDeviceCommand = new RelayCommand<Device>(doSelectDeviceCommand);
            AddLinkageActionCommand = new RelayCommand<LinkageAction>((action) =>
            {
                if (action != null)
                {
                    NewLinkageActions.Add(action);
                }
                else
                {
                    NewLinkageActions.Add(new LinkageAction());
                }
            }, (action) =>
            {
                return NewDevice != null;
            });
            RemoveLinkageActionCommand = new RelayCommand<LinkageAction>((action) =>
            {
                if (action != null)
                {
                    NewLinkageActions.Remove(action);
                }
            });

            LinkageActions.CollectionChanged += (s, e) =>
            {
                _isDirty = true;
            };
        }

        public int Index { get; set; }

        // 联动控制选项的编号
        public string LinkageNum { get; set; }

        //触发设备联动的条件，可以是单个表达式条件，也可以是多个表达式组合成的条件链
        private ICondition _condition;
        public ICondition Condition
        {
            get { return _condition; }
            set
            {
                if (SetProperty(ref _condition, value))
                {
                    _isDirty = true;
                }
            }
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
            set
            {
                if (SetProperty(ref _formattedCondition, value))
                {
                    _isDirty = true;
                }
            }
        }

        private Device _conditionDevice;
        public Device ConditionDevice
        {
            get { return _conditionDevice; }
            set
            {
                if (SetProperty(ref _conditionDevice, value))
                {
                    _isDirty = true;
                }
            }
        }

        // 联动控制设备的编码
        private Device _linkageDevice;
        public Device LinkageDevice
        {
            get { return _linkageDevice; }
            set
            {
                if (SetProperty(ref _linkageDevice, value))
                {
                    _isDirty = true;
                }
            }
        }

        private string _header;
        public string Header
        {
            get => _header;
            set
            {
                if (SetProperty(ref _header, value))
                {
                    _isDirty = true;
                }
            }
        }

        // 定义了当满足联控条件时，如何控制属性Device指定的设备
        private ObservableCollection<LinkageAction> _linkageActions = new ObservableCollection<LinkageAction>();
        public ObservableCollection<LinkageAction> LinkageActions
        {
            get => _linkageActions;
            private set => SetProperty(ref _linkageActions, value);
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
            set 
            { 
                if (SetProperty(ref _newDevice, value))
                {
                    if (AddLinkageActionCommand != null)
                        AddLinkageActionCommand.NotifyCanExecuteChanged();
                }
            }
        }

        private string _newHeader;
        public string NewHeader
        {
            get => _newHeader;
            set => SetProperty(ref _newHeader, value);
        }

        private ObservableCollection<LinkageAction> _newLinkageActions = new ObservableCollection<LinkageAction>();
        public ObservableCollection<LinkageAction> NewLinkageActions
        {
            get => _newLinkageActions;
            private set => SetProperty(ref _newLinkageActions, value);
        }

        public RelayCommand<Device> SelectDeviceCommand { get; set; }

        private void doSelectDeviceCommand(Device device)
        {
            if (device == null)
                return;
            this.NewDevice = device;
        }
        #endregion

        public RelayCommand<LinkageAction> AddLinkageActionCommand { get; private set; }

        public RelayCommand<LinkageAction> RemoveLinkageActionCommand { get; private set; }

        #region ISaveState 接口实现
        private bool _isDirty = false;
        public bool IsDirty
        {
            get
            {
                if (_isDirty)
                    return true;
                foreach (var action in LinkageActions)
                {
                    if (action.IsDirty)
                        return true;
                }
                return false;
            }
        }

        public void Save()
        {
            _isDirty = false;
            foreach (var action in LinkageActions)
            {
                action.Save();
            }
        }
        #endregion
    }

    public class LinkageAction : ObservableObject, ICloneable, ISaveState
    {
        private Variable _variable;
        public Variable Variable
        {
            get => _variable;
            set
            {
                if (SetProperty(ref _variable, value))
                {
                    _isDirty = true;
                }
            }
        }

        private object _value;
        public object Value
        {
            get => _value;
            set
            {
                if (SetProperty(ref _value, value))
                {
                    _isDirty = true;
                }
            }
        }

        public object Clone()
        {
            return new LinkageAction()
            {
                Variable = this.Variable,
                Value = this.Value
            };
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
}
