using Coffee.DigitalPlatform.Common;
using Coffee.DigitalPlatform.CommWPF;
using Coffee.DigitalPlatform.Entities;
using Coffee.DigitalPlatform.IDataAccess;
using Coffee.DigitalPlatform.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using static Dapper.SqlMapper;

namespace Coffee.DigitalPlatform.ViewModels
{
    public class ConfigureComponentViewModel : AbstractComponentViewModel, ISaveState
    {
        private ILocalDataAccess _localDataAccess;

        public ConfigureComponentViewModel(ILocalDataAccess localDataAccess) : base()
        {
            _localDataAccess = localDataAccess;

            SelectDeviceCommand = new RelayCommand<Device>(doSelectDevice);
            CloseErrorMessageBoxCommand = new RelayCommand<object>(doCloseErrorMessageBox);

            LoadComponentsCommand = new RelayCommand(loadComponentsFromDatabase);
            UnloadComponentsCommand = new RelayCommand(unloadComponents);
            CreateComponentByDragCommand = new RelayCommand<DragEventArgs>(doCreateComponentByDrag);

            AlarmConditionCommand = new RelayCommand(doAlarmConditionCommand, canDoAlarmConditionCommand);
            ControlInfoByTriggerConditionCommand = new RelayCommand(doControlInfoByTriggerConditionCommand, canDoControlInfoByTriggerConditionCommand);

            SaveDeviceConfigurationCommand = new RelayCommand<object>(doSaveDeviceConfigurationCommand);
            CloseCommand = new RelayCommand<object>(doCloseCommand);

            PopupClosingCommand = new RelayCommand<DependencyObject>(doClosingPopupCommand);

            if (!DesignTimeHelper.IsInDesignMode)
            {
                var componentInstances = localDataAccess.GetComponentsForCreate();
                ComponentGroups = componentInstances.GroupBy(c => c.Category).Select(g => new ComponentGroup()
                {
                    GroupName = g.Key,
                    Children = g.Select(entity => new Component()
                    {
                        Label = entity.Label,
                        Icon = $"pack://application:,,,/Coffee.DigitalPlatform.Assets;component/Images/Thumbs/{entity.Icon}",
                        TargetType = entity.TargetType,
                        Width = entity.Width,
                        Height = entity.Height
                    }).ToList()
                });
            }

            DeviceList.CollectionChanged += (s, e) =>
            {
                _isDirty = true;
            };
        }

        #region 设备实例
        private IEnumerable<ComponentGroup> _componentGroups;
        public IEnumerable<ComponentGroup> ComponentGroups 
        {
            get { return _componentGroups; }
            set { SetProperty(ref _componentGroups, value); }
        }

        public ObservableCollection<Device> DeviceList { get; set; } = new ObservableCollection<Device>();

        private Device _currentDevice;
        public Device CurrentDevice
        {
            get { return _currentDevice; }
            set 
            { 
                if (SetProperty(ref _currentDevice, value))
                {
                    if (AlarmConditionCommand != null)
                    {
                        AlarmConditionCommand.NotifyCanExecuteChanged();
                    }
                    if (ControlInfoByTriggerConditionCommand != null)
                    {
                        ControlInfoByTriggerConditionCommand.NotifyCanExecuteChanged();
                    }
                }
            }
        }

        public RelayCommand LoadComponentsCommand { get; set; }

        public RelayCommand UnloadComponentsCommand { get; set; }

        public RelayCommand<DragEventArgs> CreateComponentByDragCommand { get; set; }

        public RelayCommand<Device> SelectDeviceCommand { get; set; }

        private void doCreateComponentByDrag(DragEventArgs e)
        {
            var data = (Component)e.Data.GetData(typeof(Component));
            var point = e.GetPosition((IInputElement)e.Source);
            
            var device = new Device(_localDataAccess)
            {
                Name = data.Label,
                DeviceNum = "d_" + DateTime.Now.ToString("yyyyMMddHHmmssFFF"),
                DeviceType = data.TargetType,
                Width = data.Width,
                Height = data.Height,
                X = point.X - data.Width / 2,
                Y = point.Y - data.Height / 2,

                DeleteCommand = new RelayCommand<Device>(model => {
                    if (model != null)
                        DeviceList.Remove(model);
                    }),
                GetDevices = () => DeviceList.Where(d => d is IComponentContext).Cast<IComponentContext>().ToList(),
                GetAuxiliaryLines = () => DeviceList.Where(d => d is IAuxiliaryLineContext).Cast<IAuxiliaryLineContext>().ToList()
            };
            device.InitContextMenu();
            DeviceList.Add(device);
        }

        private void doSelectDevice(Device device)
        {
            // 对当前组件进行选中
            // 进行属性、点位编辑
            if (CurrentDevice != null)
            {
                CurrentDevice.IsSelected = false; //任意时候仅有一个能选中
            }
            if (device != null)
            {
                device.IsSelected = true;
            }
            CurrentDevice = device;

            // 设备选中后，通知属性面板切换
            // 如果当前设备没有添加任何通信参数，则只提供通信协议参数供用户选择
            // 必须确定通信协议后，才可添加其他相关通信参数
            if (device != null)
            {
                var commParamsByDevice = _localDataAccess.GetCommunicationParametersByDevice(device.DeviceNum);
                if (commParamsByDevice == null || !commParamsByDevice.Any())
                {
                    //var protocolParamDef = _localDataAccess.GetProtocolParamDefinition();
                    //if (protocolParamDef != null)
                    //{
                    //    device.CommunicationParameterDefinitions.Clear();
                    //    device.CommunicationParameterDefinitions.Add(new CommunicationParameterDefinition
                    //    {
                    //        Label = protocolParamDef.Label,
                    //        ParameterName = protocolParamDef.ParameterName,
                    //        ValueInputType = (ValueInputTypes)protocolParamDef.ValueInputType,
                    //        ValueDataType = protocolParamDef.ValueDataType,
                    //        DefaultOptionIndex = protocolParamDef.DefaultOptionIndex,
                    //    });
                    //}
                }
                else
                {
                    string protocolName = commParamsByDevice.Where(p => p.PropName == "Protocol").Select(p => p.PropValue).FirstOrDefault();
                    if (!string.IsNullOrWhiteSpace(protocolName))
                    {
                        IList<CommunicationParameterDefinitionEntity> paramDefEntities = _localDataAccess.GetCommunicationParamDefinitions(protocolName);
                        if (paramDefEntities != null && paramDefEntities.Any())
                        {
                            paramDefEntities = paramDefEntities.OrderBy(entity => entity.IsDefaultParameter ? 0 : 1).ToList();
                            foreach (var paramDefEntity in paramDefEntities)
                            {
                                //不重复添加用户已经添加的通信参数到下拉框
                                if (device.CommunicationParameterDefinitions.Any(paramDef => paramDef.ParameterName == paramDefEntity.ParameterName))
                                {
                                    continue;
                                }
                                device.CommunicationParameterDefinitions.Add(new CommunicationParameterDefinition()
                                {
                                    Label = paramDefEntity.Label,
                                    ParameterName = paramDefEntity.ParameterName,
                                    ValueInputType = (ValueInputTypes)paramDefEntity.ValueInputType,
                                    ValueDataType = paramDefEntity.ValueDataType,
                                    DefaultValueOption = paramDefEntity.DefaultValueOption,
                                    DefaultOptionIndex = paramDefEntity.DefaultOptionIndex,
                                    ValueOptions = _localDataAccess.GetCommunicationParameterOptions(paramDefEntity)?.Select(o => new CommunicationParameterOption
                                    {
                                        PropName = o.PropName,
                                        PropOptionValue = o.PropOptionValue,
                                        PropOptionLabel = o.PropOptionLabel
                                    }).ToList()
                                });
                            }
                        }
                    }

                    device.CommunicationParameters.Clear();
                    commParamsByDevice.OrderBy(entity => entity.PropName == "Protocol" ? 0 : 1).Select(paramEntity => new CommunicationParameter()
                    {
                        PropName = paramEntity.PropName,
                        PropValue = paramEntity.PropValue,
                        PropValueType = TypeUtils.GetTypeFromAssemblyQualifiedName(paramEntity.PropValueType)
                    }).ToList().ForEach(param => device.CommunicationParameters.Add(param));
                }
            }
        }

        private void loadComponentsFromDatabase()
        {
            IList<DeviceEntity> deviceEntities = _localDataAccess.ReadDevices();
            Dictionary<string, Device> deviceNumDict = new Dictionary<string, Device>();
            foreach (var deviceEntity in deviceEntities)
            {
                var device = new Device(_localDataAccess)
                {
                    DeviceNum = deviceEntity.DeviceNum,
                    Name = deviceEntity.Label,
                    DeviceType = deviceEntity.DeviceTypeName,
                    X = double.Parse(deviceEntity.X),
                    Y = double.Parse(deviceEntity.Y),
                    Z = int.Parse(deviceEntity.Z),
                    Width = double.Parse(deviceEntity.Width),
                    Height = double.Parse(deviceEntity.Height),
                    FlowDirection = (FlowDirections)Enum.Parse(typeof(FlowDirections), deviceEntity.FlowDirection),
                    Rotate = double.Parse(deviceEntity.Rotate),
                    DeleteCommand = new RelayCommand<Device>(model =>
                    {
                        if (model != null)
                            DeviceList.Remove(model);
                    }),
                    GetDevices = () => DeviceList.Where(d => d is IComponentContext).Cast<IComponentContext>().ToList(),
                    GetAuxiliaryLines = () => DeviceList.Where(d => d is IAuxiliaryLineContext).Cast<IAuxiliaryLineContext>().ToList()
                };
                device.InitContextMenu();
                // 加载通信参数
                if (deviceEntity.CommunicationParameters != null)
                {
                    foreach (var commParamEntity in deviceEntity.CommunicationParameters)
                    {
                        device.CommunicationParameters.Add(new CommunicationParameter()
                        {
                            PropName = commParamEntity.PropName,
                            PropValue = commParamEntity.PropValue,
                            PropValueType = TypeUtils.GetTypeFromAssemblyQualifiedName(commParamEntity.PropValueType)
                        });
                    }
                }
                // 加载变量点位
                if (deviceEntity.Variables != null)
                {
                    foreach(var variableEntity in deviceEntity.Variables)
                    {
                        device.Variables.Add(new Variable()
                        {
                            DeviceNum = device.DeviceNum,
                            VarNum = variableEntity.VarNum,
                            VarName = variableEntity.Label,
                            VarAddress = variableEntity.Address,
                            VarType = TypeUtils.GetTypeFromAssemblyQualifiedName(variableEntity.VarType),
                            Offset = variableEntity.Offset,
                            Factor = variableEntity.Factor
                        });
                    }
                }
                DeviceList.Add(device);

                if (!deviceNumDict.ContainsKey(device.DeviceNum))
                {
                    deviceNumDict.Add(device.DeviceNum, device);
                }
            }

            //字典的键是设备编码，值是该设备的预警实体集合
            Dictionary<string, IList<AlarmEntity>> deviceAlarmDict = _localDataAccess.ReadAlarms();
            IEnumerable<ConditionEntity> topConditionEntities = _localDataAccess.GetTopConditions(); //预加载所有顶级条件选项
            IEnumerable<ConditionEntity> conditionEntities = _localDataAccess.GetConditions(); //预加载所有顶级条件选项

            foreach (var pair in deviceAlarmDict)
            {
                string deviceNum = pair.Key;
                IList<AlarmEntity> alarmList = pair.Value;
                if (!deviceNumDict.TryGetValue(deviceNum, out Device? device))
                {
                    throw new InvalidOperationException($"没有找到对应编码{deviceNum}的设备");
                }
                IList<Alarm> alarms = new List<Alarm>();
                foreach (var alarmEntity in alarmList)
                {
                    var alarm = new Alarm()
                    {
                        AlarmNum = alarmEntity.AlarmNum,
                        AlarmDevice = device,
                        AlarmMessage = alarmEntity.AlarmMessage,
                        AlarmTag = alarmEntity.AlarmTag,
                        AlarmLevel = alarmEntity.AlarmLevel,
                        AlarmTime = !string.IsNullOrWhiteSpace(alarmEntity.AlarmTime) ? DateTime.Parse(alarmEntity.AlarmTime) : (DateTime?)null,
                        UserId = alarmEntity.UserId
                    };
                    if (!string.IsNullOrWhiteSpace(alarmEntity.State) && Enum.TryParse<AlarmStatus>(alarmEntity.State, out AlarmStatus alarmStatus))
                    {
                        alarm.AlarmState = new AlarmState(alarmStatus);
                        if (!string.IsNullOrWhiteSpace(alarmEntity.SolvedTime) && DateTime.TryParse(alarmEntity.SolvedTime, out DateTime solvedTime))
                        {
                            alarm.AlarmState.SolvedTime = solvedTime;
                        }
                    }

                    var topConditionEntity = topConditionEntities?.FirstOrDefault(c => string.Equals(c.CNum, alarmEntity.ConditionNum));
                    if (topConditionEntity != null)
                    {
                        //预警信息仅使用顶级条件项作为触发预警的条件
                        ICondition topCondition = createConditionByEntity(topConditionEntity, conditionEntities, device.Variables.ToDictionary(v => v.VarNum, v => v));
                        alarm.Condition = topCondition;
                    }
                    alarms.Add(alarm);
                }

                device.Alarms.Clear();
                foreach(var alarm in alarms)
                {
                    device.Alarms.Add(alarm);
                }
            }

            //字典的键是设备编码，值是该设备的手动控制选项实体集合
            Dictionary<string, IList<ControlInfoByManualEntity>> deviceControlInfoByManualDict = _localDataAccess.ReadControlInfosByManual();
            foreach(var pair in deviceControlInfoByManualDict)
            {
                string deviceNum = pair.Key;
                IList<ControlInfoByManualEntity> controlInfoList = pair.Value;
                if (!deviceNumDict.TryGetValue(deviceNum, out Device? device))
                {
                    throw new InvalidOperationException($"没有找到对应编码{deviceNum}的设备");
                }
                IList<ControlInfoByManual> controlInfos = new List<ControlInfoByManual>();
                foreach (var controlInfoEntity in controlInfoList)
                {
                    Variable variable = device.Variables.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v.VarNum) && v.VarNum == controlInfoEntity.VarNum);
                    try
                    {
                        var controlInfo = new ControlInfoByManual()
                        {
                            CNum = controlInfoEntity.CNum,
                            DeviceNum = controlInfoEntity.DeviceNum,
                            Header = controlInfoEntity.Header,
                            Variable = variable
                        };
                        if (variable != null)
                        {
                            controlInfo.Value = ObjectToStringConverter.ConvertFromString(controlInfoEntity.Value, variable.VarType);
                        }
                        controlInfos.Add(controlInfo);
                    }
                    catch (Exception ex)
                    {
                        if (variable != null)
                        {
                            throw new Exception($"加载手动控制选项{controlInfoEntity.Header} 的值失败，当前值不符合类型{variable.VarType.Name}的格式！");
                        }
                        else
                        {
                            throw new Exception($"加载手动控制选项{controlInfoEntity.Header} 的值失败，当前值格式不正确！");
                        }
                    }
                }

                device.ControlInfosByManual.Clear();
                foreach (var controlInfo in controlInfos)
                {
                    device.ControlInfosByManual.Add(controlInfo);
                }
            }

            //字典的键是设备编码，值是该设备的联动控制选项实体集合
            Dictionary<string, IList<ControlInfoByTriggerEntity>> deviceControlInfoByTriggerDict = _localDataAccess.ReadControlInfosByTrigger();
            foreach(var pair in deviceControlInfoByTriggerDict)
            {
                string conditionDeviceNum = pair.Key;
                IList<ControlInfoByTriggerEntity> controlInfoEntities = pair.Value;
                if (!deviceNumDict.TryGetValue(conditionDeviceNum, out Device? conditionDevice))
                {
                    throw new InvalidOperationException($"没有找到对应编码{conditionDeviceNum}的设备");
                }
                IList<ControlInfoByTrigger> controlInfos = new List<ControlInfoByTrigger>();
                foreach (var controlInfoEntity in controlInfoEntities)
                {
                    string linkageDeviceNum = controlInfoEntity.LinkageDeviceNum;
                    if (!deviceNumDict.TryGetValue(linkageDeviceNum, out Device? linkageDevice))
                    {
                        throw new InvalidOperationException($"加载手动控制选项{controlInfoEntity.Header} 的值失败，没有找到对应编码{linkageDeviceNum}的设备！");
                    }
                    Type? valueType = null;
                    object @value;
                    try
                    {
                        var @var = linkageDevice.Variables.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v.VarNum) && v.VarNum == controlInfoEntity.VarNum);
                        valueType = @var.VarType;
                        @value = ObjectToStringConverter.ConvertFromString(controlInfoEntity.Value, valueType);
                    }
                    catch (Exception ex)
                    {
                        if (valueType != null)
                        {
                            throw new Exception($"加载手动控制选项{controlInfoEntity.Header} 的值失败，当前值不符合类型{valueType.Name}的格式！");
                        }
                        else
                        {
                            throw new Exception($"加载手动控制选项{controlInfoEntity.Header} 的值失败，当前值格式不正确！");
                        }
                    }

                    Variable variable = linkageDevice.Variables.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v.VarNum) && v.VarNum == controlInfoEntity.VarNum);
                    var targetDevice = DeviceList?.FirstOrDefault(d => !string.IsNullOrWhiteSpace(linkageDeviceNum) && d.DeviceNum == linkageDeviceNum);

                    //在当前联控条件触发下，可以对同一个联控设备操控多个点位信息
                    var controlInfo = controlInfos.FirstOrDefault(p => !string.IsNullOrWhiteSpace(linkageDeviceNum) && p.LinkageDevice != null && p.LinkageDevice.DeviceNum == linkageDeviceNum);
                    if (controlInfo == null)
                    {
                        //因为ControlInfoByTrigger对象表示一个联动控制选项实体，但该实体可以对同一个联动设备操控多个点位信息
                        //而数据表trigger_controls的每一条记录表示对联动设备的一个点位信息的控制，即多个记录对应同一个ControlInfoByTrigger对象
                        //所以这里需要将ControlInfoByTrigger对象的属性LinkageNum从实体对象ControlInfoByTriggerEntity的属性LinkageNum提取出来
                        //转换规则是：ControlInfoByTriggerEntity.LinkageNum = ControlInfoByTrigger.LinkageNum + "_" + 数字XXX
                        string linkageNum = controlInfoEntity.LinkageNum;
                        if (controlInfoEntity.LinkageNum.LastIndexOf('_') > 0)
                        {
                           linkageNum = controlInfoEntity.LinkageNum.Substring(0, controlInfoEntity.LinkageNum.LastIndexOf('_'));
                        }
                        controlInfo = new ControlInfoByTrigger()
                        {
                            ConditionDevice = conditionDevice,
                            LinkageNum = linkageNum,
                            LinkageDevice = targetDevice,
                            Header = controlInfoEntity.Header,
                        };
                        controlInfo.LinkageActions.Add(new LinkageAction()
                        {
                            Variable = variable,
                            Value = @value
                        });
                        var topConditionEntity = topConditionEntities?.FirstOrDefault(c => string.Equals(c.CNum, controlInfoEntity.ConditionNum));
                        if (topConditionEntity != null)
                        {
                            //联控选项信息仅使用顶级条件项作为触发联动控制的条件
                            ICondition topCondition = createConditionByEntity(topConditionEntity, conditionEntities, conditionDevice.Variables.ToDictionary(v => v.VarNum, v => v));
                            controlInfo.Condition = topCondition;
                        }
                        controlInfos.Add(controlInfo);
                    }
                    else
                    {
                        var action = new LinkageAction()
                        {
                            Variable = variable,
                            Value = value
                        };
                        controlInfo.LinkageActions.Add(action);
                    }
                }

                conditionDevice.ControlInfosByTrigger.Clear();
                foreach (var controlInfo in controlInfos)
                {
                    conditionDevice.ControlInfosByTrigger.Add(controlInfo);
                }
            }
        }

        private void unloadComponents()
        {
            DeviceList.Clear();
        }
        #endregion

        #region 通信参数
        private void initCommunicationParameters()
        {
            _localDataAccess.GetProtocolParamDefinition();
        }

        #endregion

        #region 变量点位信息
        public ICommand PopupClosingCommand { get; set; }
        private void doClosingPopupCommand(DependencyObject sender)
        {
            var focusedElements = FocusHelper.GetAllKeyboardFocusedElements(sender);
            foreach (var element in focusedElements)
            {
                element.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }
            Keyboard.ClearFocus();
        }
        #endregion

        #region 提示消息
        private string _failureMessageOnSaving;

        public string FailureMessageOnSaving
        {
            get { return _failureMessageOnSaving; }
            set { SetProperty(ref _failureMessageOnSaving, value); }
        }

        public RelayCommand<object> CloseErrorMessageBoxCommand { get; set; }

        private void doCloseErrorMessageBox(object obj)
        {
            VisualStateManager.GoToElementState(obj as Window, "HideFailure", true);
        }
        #endregion

        #region 读写操作及下方按钮栏
        private int saveCount = 0; //自从打开配置窗口后，保存操作的次数。当保存次数大于0时，关闭窗口时需要重新加载数据。

        public RelayCommand<object> CloseCommand { get; set; }

        public RelayCommand<object> SaveDeviceConfigurationCommand { get; set; }

        private void doCloseCommand(object owner)
        {
            VisualStateManager.GoToElementState(owner as Window, "NormalToSuccess", true);
            VisualStateManager.GoToElementState(owner as Window, "NormalToFailure", true);

            (owner as Window).DialogResult = saveCount > 0;
            saveCount = 0;
        }

        private void doSaveDeviceConfigurationCommand(object owner)
        {
            VisualStateManager.GoToElementState(owner as Window, "NormalToSuccess", true);
            VisualStateManager.GoToElementState(owner as Window, "NormalToFailure", true);

            try
            {
                if (DeviceList != null && DeviceList.Any())
                {
                    var deviceWithVariableError = DeviceList.Where(d => d.Variables != null && d.Variables.Any(v => !string.IsNullOrWhiteSpace(v.Error))).FirstOrDefault();
                    if (deviceWithVariableError != null)
                    {
                        throw new Exception($"设备 {deviceWithVariableError.Name} 存在点位配置错误，无法保存，请检查后重试。");
                    }

                    var deviceWithControlByManualError = DeviceList.Where(d => d.ControlInfosByManual != null && d.ControlInfosByManual.Any(c => !string.IsNullOrWhiteSpace(c.Error))).FirstOrDefault();
                    if (deviceWithControlByManualError != null)
                    {
                        throw new Exception($"设备 {deviceWithControlByManualError.Name} 存在手动控制选项配置错误，无法保存，请检查后重试。");
                    }
                }

                IList<DeviceEntity> deviceEntities = new List<DeviceEntity>();
                foreach (var device in DeviceList)
                {
                    if (string.IsNullOrWhiteSpace(device.DeviceNum))
                    {
                        throw new Exception($"设备 {device.Name} 未设置设备编号，无法保存，请设置后重试。");
                    }
                    var deviceEntity = new DeviceEntity
                    {
                        DeviceNum = device.DeviceNum,
                        DeviceTypeName = device.DeviceType,
                        Label = device.Name,
                        X = device.X.ToString(),
                        Y = device.Y.ToString(),
                        Z = device.Z.ToString(),
                        Width = device.Width.ToString(),
                        Height = device.Height.ToString(),
                        FlowDirection = Enum.GetName(typeof(FlowDirections), device.FlowDirection),
                        Rotate = device.Rotate.ToString()
                    };
                    //通信参数
                    foreach(var commParam in device.CommunicationParameters)
                    {
                        if (deviceEntity.CommunicationParameters == null)
                        {
                            deviceEntity.CommunicationParameters = new List<CommunicationParameterEntity>();
                        }
                        deviceEntity.CommunicationParameters.Add(new CommunicationParameterEntity
                        {
                            PropName = commParam.PropName,
                            PropValue = commParam.PropValue,
                            PropValueType = commParam.PropValueType.AssemblyQualifiedName
                        });
                    }
                    //变量点位
                    foreach (var variable in device.Variables)
                    {
                        if (deviceEntity.Variables == null)
                        {
                            deviceEntity.Variables = new List<VariableEntity>();
                        }
                        deviceEntity.Variables.Add(new VariableEntity
                        {
                            VarNum = variable.VarNum,
                            Label = variable.VarName,
                            Address = variable.VarAddress,
                            VarType = variable.VarType.AssemblyQualifiedName,
                            Offset = variable.Offset,
                            Factor = variable.Factor
                        });
                    }

                    deviceEntities.Add(deviceEntity);
                }

                _localDataAccess.SaveDevices(deviceEntities);

                var oldTopConditions = _localDataAccess.GetTopConditions();
                //得到当前还在使用的顶级条件编号集合（包括预警条件和联控条件）
                var aliveConditionNumList = new HashSet<string>();

                // 保存设备预警信息
                Dictionary<string, IList<AlarmEntity>> deviceAlarmDict = new Dictionary<string, IList<AlarmEntity>>();
                // 保存用于预警信息的条件选项字典，键是顶级条件选项编号，值是该顶级条件选项及其子条件选项实体集合
                Dictionary<string, IList<ConditionEntity>> conditionDict = new Dictionary<string, IList<ConditionEntity>>();
                foreach (var device in DeviceList)
                {
                    IList<AlarmEntity> alarmEntities = new List<AlarmEntity>();
                    foreach (var alarm in device.Alarms)
                    {
                        if (alarm.Condition == null)
                            continue;
                        
                        DateTime? solvedTime = null;
                        if (alarm.AlarmState != null && (alarm.AlarmState.Status == AlarmStatus.SolvedByManual || alarm.AlarmState.Status == AlarmStatus.SolvedBySystem) && alarm.AlarmState.SolvedTime.HasValue)
                        {
                            solvedTime = alarm.AlarmState.SolvedTime.Value;
                        }
                        //创建条件项及其子条件项实体
                        var conditionEntities = new List<ConditionEntity>();
                        createConditionEntitiesBy(alarm.Condition, null, conditionEntities);
                        var topCondition = conditionEntities.Where(c => string.IsNullOrEmpty(c.CNum_Parent)).FirstOrDefault();

                        if (topCondition != null && !conditionDict.ContainsKey(topCondition.CNum))
                        {
                            conditionDict.Add(topCondition.CNum, conditionEntities);
                        }

                        var alarmEntity = new AlarmEntity
                        {
                            AlarmNum = alarm.AlarmNum,
                            AlarmMessage = alarm.AlarmMessage,
                            AlarmTag = alarm.AlarmTag,
                            AlarmLevel = alarm.AlarmLevel,
                            AlarmTime = alarm.AlarmTime.HasValue ? alarm.AlarmTime.Value.ToString("yyyy/MM/dd HH:mm:ss") : null,
                            SolvedTime = solvedTime.HasValue ? solvedTime.Value.ToString("yyyy/MM/dd HH:mm:ss") : null,
                            ConditionNum = alarm.Condition?.ConditionNum,
                            DeviceNum = device.DeviceNum,
                            State = alarm.AlarmState != null ? Enum.GetName(typeof(AlarmStatus), alarm.AlarmState.Status) : null,
                            UserId = alarm.UserId
                        };
                        alarmEntities.Add(alarmEntity);
                    }
                    if (alarmEntities.Any())
                    {
                        deviceAlarmDict.Add(device.DeviceNum, alarmEntities);
                    }
                }

                //得到当前还在使用的一级预警条件编号集合
                var alarmConditionNumList = deviceAlarmDict.Select(p => p.Value).SelectMany(a => a)
                               .Select(a => a.ConditionNum)
                               .Distinct();
                foreach (var item in alarmConditionNumList)
                {
                    aliveConditionNumList.Add(item);
                }

                _localDataAccess.SaveAlarms(deviceAlarmDict, conditionDict);

                // 保存手动控制选项信息
                Dictionary<string, IList<ControlInfoByManualEntity>> deviceControlInfoByManualDict = new Dictionary<string, IList<ControlInfoByManualEntity>>();
                foreach (var device in DeviceList)
                {
                    IList<ControlInfoByManualEntity> controlInfoEntities = new List<ControlInfoByManualEntity>();
                    foreach (var controlInfo in device.ControlInfosByManual)
                    {
                        var controlInfoEntity = new ControlInfoByManualEntity
                        {
                            CNum = controlInfo.CNum,
                            DeviceNum = controlInfo.DeviceNum,
                            Header = controlInfo.Header,
                            VarNum = controlInfo.Variable?.VarNum,
                            Value = ObjectToStringConverter.ConvertToString(controlInfo.Value)
                        };
                        controlInfoEntities.Add(controlInfoEntity);
                    }
                    if (controlInfoEntities.Any())
                    {
                        deviceControlInfoByManualDict.Add(device.DeviceNum, controlInfoEntities);
                    }
                }
                _localDataAccess.SaveControlInfosByManual(deviceControlInfoByManualDict);

                // 保存联动控制选项信息
                Dictionary<string, IList<ControlInfoByTriggerEntity>> deviceControlInfoByTriggerDict = new Dictionary<string, IList<ControlInfoByTriggerEntity>>();
                conditionDict = new Dictionary<string, IList<ConditionEntity>>(); //保存用于联控信息的条件选项字典
                foreach (var device in DeviceList)
                {
                    IList<ControlInfoByTriggerEntity> controlInfoEntities = new List<ControlInfoByTriggerEntity>();
                    foreach (var controlInfo in device.ControlInfosByTrigger)
                    {
                        if (controlInfo.Condition == null)
                            continue;

                        //创建条件项及其子条件项实体
                        var conditionEntities = new List<ConditionEntity>();
                        createConditionEntitiesBy(controlInfo.Condition, null, conditionEntities);
                        var topCondition = conditionEntities.Where(c => string.IsNullOrEmpty(c.CNum_Parent)).FirstOrDefault();

                        if (topCondition != null && !conditionDict.ContainsKey(topCondition.CNum))
                        {
                            conditionDict.Add(topCondition.CNum, conditionEntities);
                        }

                        int digitalSuffixIdx = 1;
                        foreach(var action in controlInfo.LinkageActions)
                        {
                            //因为ControlInfoByTrigger对象表示一个联动控制选项实体，但该实体可以对同一个联动设备操控多个点位信息
                            //而数据表trigger_controls的每一条记录表示对联动设备的一个点位信息的控制，即多个记录对应同一个ControlInfoByTrigger对象
                            //换句话说，ControlInfoByTriggerEntity实体中的属性LinkageNum仅表示一个联动控制选项中某一个点位操作的编号；而ControlInfoByTrigger对象的属性LinkageNum表示整个联动控制选项（多个操作点位）的编号
                            //所以这里需要将ControlInfoByTriggerEntity中的属性LinkageNum与ControlInfoByTrigger对象的属性LinkageNum进行转换
                            //ControlInfoByTriggerEntity.LinkageNum = ControlInfoByTrigger.LinkageNum + "_" + 数字XXX
                            string linkageNum = controlInfo.LinkageNum;
                            if (linkageNum.LastIndexOf('_') >= 0)
                            {
                                string suffix = linkageNum.Substring(linkageNum.LastIndexOf('-') + 1);
                                if (suffix.Length == 3 && int.TryParse(suffix, out int digitalSuffix))
                                {
                                    //如果后缀是数字XXX格式，则说明这个是数据库已经存在的记录，不做处理
                                }
                                else
                                {
                                    //否则在ControlInfoByTrigger对象的属性LinkageNum的基础上添加后缀数字格式XXX，在同一个ControlInfoByTrigger对象中后缀数字递增
                                    linkageNum += "_" + digitalSuffixIdx;
                                    digitalSuffixIdx++;
                                }
                            }
                            
                            var controlInfoEntity = new ControlInfoByTriggerEntity
                            {
                                LinkageNum = linkageNum,
                                ConditionDeviceNum = controlInfo.ConditionDevice?.DeviceNum,
                                ConditionNum = controlInfo.Condition?.ConditionNum,
                                Header = controlInfo.Header,
                                LinkageDeviceNum = controlInfo.LinkageDevice?.DeviceNum,
                                VarNum = action.Variable?.VarNum,
                                Value = ObjectToStringConverter.ConvertToString(action.Value)
                            };
                            controlInfoEntities.Add(controlInfoEntity);
                        }
                    }
                    if (controlInfoEntities.Any())
                    {
                        deviceControlInfoByTriggerDict.Add(device.DeviceNum, controlInfoEntities);
                    }
                }

                //得到当前还在使用的一级联控条件编号集合
                var linkageConditionNumList = deviceControlInfoByTriggerDict.Select(p => p.Value).SelectMany(a => a)
                               .Select(a => a.ConditionNum)
                               .Distinct();
                foreach (var item in linkageConditionNumList)
                {
                    aliveConditionNumList.Add(item);
                }

                _localDataAccess.SaveControlInfosByTrigger(deviceControlInfoByTriggerDict, conditionDict);

                //删除那些不再使用的条件（包括顶级条件及其子条件）
                _localDataAccess.CleanUpOutdatedConditions(aliveConditionNumList.ToList(), oldTopConditions);

                VisualStateManager.GoToElementState(owner as Window, "ShowSuccess", true);

                Save(); //标记为已保存状态
            }
            catch(Exception ex)
            {
                FailureMessageOnSaving = ex.Message;
                VisualStateManager.GoToElementState(owner as Window, "ShowFailure", true);
            }
        }
        #endregion

        #region 设备条件提醒
        public RelayCommand AlarmConditionCommand { get; set; }

        private void doAlarmConditionCommand()
        {
            if (CurrentDevice == null)
                return;
            ActionManager.Execute("AlarmCondition", CurrentDevice);
        }

        private bool canDoAlarmConditionCommand()
        {
            return CurrentDevice != null;
        }
        #endregion

        #region 手动控制选项

        #endregion

        #region 联动控制选项
        public RelayCommand ControlInfoByTriggerConditionCommand { get; set; }

        private void doControlInfoByTriggerConditionCommand()
        {
            if (CurrentDevice == null)
                return;
            ActionManager.Execute("ControlInfoByTriggerCondition", CurrentDevice);
        }

        private bool canDoControlInfoByTriggerConditionCommand()
        {
            return CurrentDevice != null;
        }
        #endregion

        #region ISaveState 接口实现
        private bool _isDirty = false;
        public bool IsDirty
        {
            get
            {
                if (_isDirty) return true;
                foreach (var device in DeviceList)
                {
                    if (device.IsDirty) return true;
                }
                return false;
            }
        }

        public void Save()
        {
            _isDirty = false;
            foreach (var device in DeviceList)
            {
                device.Save();
            }
            saveCount++;
        }

        private void SaveOnInitialized()
        {
            _isDirty = false;
            foreach (var device in DeviceList)
            {
                device.Save();
            }
        }

        // 设备配置数据是否已修改。需要定时跟踪保存状态，以便提示用户保存。
        // IsDirty是ISaveState接口的属性，不能直接用于数据绑定
        private SaveStatus _saveState = SaveStatus.None;
        public SaveStatus SaveState
        {
            get { return _saveState; }
            set { SetProperty(ref _saveState, value); }
        }

        private DispatcherTimer _saveTrackTimer = null; //存盘状态跟踪定时器

        public void StartSaveTrackTimer()
        {
            if (_saveTrackTimer == null)
            {
                _saveTrackTimer = new DispatcherTimer();
                _saveTrackTimer.Interval = TimeSpan.FromSeconds(1);
                _saveTrackTimer.Tick += (s, e) =>
                {
                    if (saveCount == 0)
                    {
                        SaveState = IsDirty ? SaveStatus.Dirty : SaveStatus.None;
                    }
                    else
                    {
                        SaveState = IsDirty ? SaveStatus.Dirty : SaveStatus.Saved;
                    }
                };
            }
            //第一次的通知属性并不是因为用户操作更改的，而是组件配置页面打开后的初始化调用的，所以第一次的通知属性更改不因为影响_isDirty值。
            //所以这里需要延迟直到初始化完成后，将_isDirty值重置，再启动定时器跟踪_isDirty属性。
            Task.Delay(1000).ContinueWith(t =>
            {
                SaveOnInitialized();
                _saveTrackTimer.Start();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void StopSaveTrackTimer()
        {
            if (_saveTrackTimer != null)
            {
                _saveTrackTimer.Stop();
            }
        }
        #endregion
    }

    public enum SaveStatus
    {
        None, //未更改
        Dirty, //已更改，未保存
        Saved, //已保存
        FailToSave //保存失败
    }
}
