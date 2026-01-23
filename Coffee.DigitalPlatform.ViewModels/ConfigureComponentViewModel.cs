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
using static Dapper.SqlMapper;

namespace Coffee.DigitalPlatform.ViewModels
{
    public class ConfigureComponentViewModel : ObservableObject
    {
        private ILocalDataAccess _localDataAccess;

        public ConfigureComponentViewModel(ILocalDataAccess localDataAccess)
        {
            _localDataAccess = localDataAccess;

            SelectDeviceCommand = new RelayCommand<Device>(doSelectDevice);
            CloseErrorMessageBoxCommand = new RelayCommand<object>(doCloseErrorMessageBox);

            LoadComponentsCommand = new RelayCommand(loadComponentsFromDatabase);
            UnloadComponentsCommand = new RelayCommand(unloadComponents);
            CreateComponentByDragCommand = new RelayCommand<DragEventArgs>(doCreateComponentByDrag);

            AlarmConditionCommand = new RelayCommand(doAlarmConditionCommand, canDoAlarmConditionCommand);

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
                        AlarmMessage = alarmEntity.AlarmMessage,
                        AlarmTag = alarmEntity.AlarmTag,
                        AlarmLevel = alarmEntity.AlarmLevel,
                        AlarmTime = !string.IsNullOrWhiteSpace(alarmEntity.AlarmTime) ? DateTime.Parse(alarmEntity.AlarmTime) : (DateTime?)null
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
                    try
                    {
                        Type valueType = TypeUtils.GetTypeFromAssemblyQualifiedName(controlInfoEntity.ValueType);
                        var controlInfo = new ControlInfoByManual()
                        {
                            CNum = controlInfoEntity.CNum,
                            DeviceNum = controlInfoEntity.DeviceNum,
                            Header = controlInfoEntity.Header,
                            Address = controlInfoEntity.Address,
                            ValueType = valueType,
                            Value = ObjectToStringConverter.ConvertFromString(controlInfoEntity.Value, valueType)
                        };
                        controlInfos.Add(controlInfo);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"加载手动控制选项{controlInfoEntity.Header} 的值失败，当前值不符合类型{controlInfoEntity.ValueType}的格式！");
                    }
                }

                device.ControlInfosByManual.Clear();
                foreach (var controlInfo in controlInfos)
                {
                    device.ControlInfosByManual.Add(controlInfo);
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
        private bool isSaved = false;

        public RelayCommand<object> CloseCommand { get; set; }

        public RelayCommand<object> SaveDeviceConfigurationCommand { get; set; }

        private void doCloseCommand(object owner)
        {
            VisualStateManager.GoToElementState(owner as Window, "NormalToSuccess", true);
            VisualStateManager.GoToElementState(owner as Window, "NormalToFailure", true);

            (owner as Window).DialogResult = isSaved;
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

                // 保存设备预警信息
                Dictionary<string, IList<AlarmEntity>> deviceAlarmDict = new Dictionary<string, IList<AlarmEntity>>();
                // 保存条件选项字典，键是顶级条件选项编号，值是该顶级条件选项及其子条件选项实体集合
                Dictionary<string, IList<ConditionEntity>> conditionDict = new Dictionary<string, IList<ConditionEntity>>();
                foreach (var device in DeviceList)
                {
                    IList<AlarmEntity> alarmEntities = new List<AlarmEntity>();
                    foreach (var alarm in device.Alarms)
                    {
                        if (alarm.Condition == null)
                            continue;
                        
                        DateTime? solvedTime = null;
                        if (alarm.AlarmState != null && alarm.AlarmState.Status == AlarmStatus.Solved && alarm.AlarmState.SolvedTime.HasValue)
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
                            ConditionNum = alarm.Condition.ConditionNum,
                            DeviceNum = device.DeviceNum,
                            State = alarm.AlarmState != null ? Enum.GetName(typeof(AlarmStatus), alarm.AlarmState.Status) : null,
                        };
                        alarmEntities.Add(alarmEntity);
                    }
                    if (alarmEntities.Any())
                    {
                        deviceAlarmDict.Add(device.DeviceNum, alarmEntities);
                    }
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
                            DeviceNum = device.DeviceNum,
                            Header = controlInfo.Header,
                            Address = controlInfo.Address,
                            ValueType = controlInfo.ValueType.AssemblyQualifiedName,
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

                VisualStateManager.GoToElementState(owner as Window, "ShowSuccess", true);
            }
            catch(Exception ex)
            {
                FailureMessageOnSaving = ex.Message;
                VisualStateManager.GoToElementState(owner as Window, "ShowFailure", true);
            }
        }
        #endregion

        #region 条件选项
        /// <summary>
        /// 递归创建条件选项（包含子条件）实体，用于数据库操作
        /// </summary>
        /// <param name="condition">条件项</param>
        /// <param name="conditionParent">条件项的父级</param>
        /// <param name="conditionEntities">创建的条件项实体都存入这个集合</param>
        private void createConditionEntitiesBy(ICondition condition, ConditionChain conditionParent, IList<ConditionEntity> conditionEntities)
        {
            if (condition == null)
                return;
            if (conditionEntities == null)
            {
                conditionEntities = new List<ConditionEntity>();
            }

            if (condition is Coffee.DigitalPlatform.Models.Condition expCondition)
            {
                var conditionEntity = new ConditionEntity()
                {
                    CNum = condition.ConditionNum,
                    ConditionNodeTypes = ConditionNodeTypes.ConditionExpression,
                    VarNum = expCondition.Source.VarNum,
                    Operator = Enum.GetName(typeof(ConditionOperators), expCondition.Operator.Operator),
                    CNum_Parent = conditionParent?.ConditionNum ?? null,
                    Value = expCondition.TargetValue
                };
                conditionEntities.Add(conditionEntity);
            }
            else if (condition is Coffee.DigitalPlatform.Models.ConditionChain conditionGroup) // ConditionChain
            {
                var conditionGroupEntity = new ConditionEntity()
                {
                    CNum = conditionGroup.ConditionNum,
                    ConditionNodeTypes = ConditionNodeTypes.ConditionGroup,
                    Operator = Enum.GetName(typeof(ConditionChainOperators), conditionGroup.Operator),
                    CNum_Parent = conditionParent?.ConditionNum ?? null
                };
                conditionEntities.Add(conditionGroupEntity);

                if (conditionGroup.ConditionItems.Any())
                {
                    foreach (var conditionItem in conditionGroup.ConditionItems)
                    {
                        createConditionEntitiesBy(conditionItem, conditionGroup, conditionEntities);
                    }
                }
            }
        }

        /// <summary>
        /// 根据实体创建条件项（包含子条件项）
        /// </summary>
        /// <param name="conditionEntity">待创建条件项的实体</param>
        /// <param name="conditionEntities">所有条件项实体</param>
        /// <param name="variableNumDict">字典保存当前设备的变量名及点位信息</param>
        /// <returns>返回条件项</returns>
        private ICondition createConditionByEntity(ConditionEntity conditionEntity, IEnumerable<ConditionEntity> conditionEntities, Dictionary<string, Variable> variableNumDict)
        {
            if (conditionEntity == null)
                return null;
            if (conditionEntity.ConditionNodeTypes == ConditionNodeTypes.ConditionGroup)
            {
                var conditionGroup = new ConditionChain((ConditionChainOperators)Enum.Parse(typeof(ConditionChainOperators), conditionEntity.Operator), conditionEntity.CNum);
                var childConditions = createChildConditionsByEntity(conditionGroup, conditionEntities, variableNumDict);
                //将当前条件的子条件添加给条件条件组
                if (childConditions != null)
                {
                    foreach (var childCondition in childConditions)
                    {
                        conditionGroup.ConditionItems.Add(childCondition);
                    }
                }
                return conditionGroup;
            }
            else if (conditionEntity.ConditionNodeTypes == ConditionNodeTypes.ConditionExpression)
            {
                if (variableNumDict != null && variableNumDict.TryGetValue(conditionEntity.VarNum, out Variable variable))
                {
                    var @operator = new ConditionOperator((ConditionOperators)Enum.Parse(typeof(ConditionOperators), conditionEntity.Operator));
                    var conditionExp = new Coffee.DigitalPlatform.Models.Condition(variable, conditionEntity.Value, @operator, conditionEntity.CNum);
                    return conditionExp;
                }
                else
                {
                    return null;
                }
            }
            else
                return null;
        }

        //递归调用，返回根据指定条件实体下的所有子条件项实体创建的条件项集合
        private IEnumerable<ICondition> createChildConditionsByEntity(ICondition parentCondition, IEnumerable<ConditionEntity> conditionEntities, Dictionary<string, Variable> variableNumDict)
        {
            if (conditionEntities == null || !conditionEntities.Any())
                return Enumerable.Empty<ICondition>();
            if (parentCondition == null)
                return Enumerable.Empty<ICondition>();
            //获取当前条件实体的所有子条件实体集合
            var childConditionEntities = conditionEntities.Where(c => !string.IsNullOrWhiteSpace(c.CNum_Parent) && string.Equals(c.CNum_Parent, parentCondition.ConditionNum));
            IList<ICondition> childConditions = new List<ICondition>();
            foreach (var childConditionEntity in childConditionEntities)
            {
                if (childConditionEntity.ConditionNodeTypes == ConditionNodeTypes.ConditionGroup)
                {
                    var conditionGroup = new ConditionChain((ConditionChainOperators)Enum.Parse(typeof(ConditionChainOperators), childConditionEntity.Operator), childConditionEntity.CNum);
                    //将当前子条件的后代条件添加给子条件组
                    var descendantConditions = createChildConditionsByEntity(conditionGroup, conditionEntities, variableNumDict);
                    if (descendantConditions != null)
                    {
                        foreach (var desCondition in descendantConditions)
                        {
                            conditionGroup.ConditionItems.Add(desCondition);
                        }
                    }
                }
                else if (childConditionEntity.ConditionNodeTypes == ConditionNodeTypes.ConditionExpression)
                {
                    if (variableNumDict != null && variableNumDict.TryGetValue(childConditionEntity.VarNum, out Variable variable))
                    {
                        var @operator = new ConditionOperator((ConditionOperators)Enum.Parse(typeof(ConditionOperators), childConditionEntity.Operator));
                        var conditionExp = new Coffee.DigitalPlatform.Models.Condition(variable, childConditionEntity.Value, @operator, childConditionEntity.CNum);
                        childConditions.Add(conditionExp);
                    }
                }
            }
            return childConditions;
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
    }
}
