using Coffee.DeviceAccess;
using Coffee.DeviceAdapter;
using Coffee.DigitalPlatform.Common;
using Coffee.DigitalPlatform.CommWPF;
using Coffee.DigitalPlatform.Entities;
using Coffee.DigitalPlatform.IDataAccess;
using Coffee.DigitalPlatform.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using models = Coffee.DigitalPlatform.Models;
using serv = Coffee.DeviceAccess;

namespace Coffee.DigitalPlatform.ViewModels
{
    public class MonitorComponentViewModel : AbstractComponentViewModel, INavigationService
    {
        MainViewModel _mainViewModel;
        ILocalDataAccess _localDataAccess;
        ICommunicationService _communicationService;
        ILogger<MonitorComponentViewModel> _logger;

        public MonitorComponentViewModel(MainViewModel mainViewModel, ILocalDataAccess localDataAccess, ICommunicationService communicationService, ILoggerFactory loggerFactory) : base()
        {
            _mainViewModel = mainViewModel;
            _localDataAccess = localDataAccess;
            _communicationService = communicationService;
            _logger = loggerFactory.CreateLogger<MonitorComponentViewModel>();

            initDataForMonitor();

            ConfigureComponentCommand = new RelayCommand(showConfigureComponentDialog);
            ResetPopupWithVariableListCommand = new RelayCommand(doResetPopupWithVariableListCommand);
            ResetPopupWithManualListCommand = new RelayCommand(doResetPopupWithManualListCommand);

            AlarmDetailCommand = new RelayCommand<Device>(doAlarmDetailCommand, canAlarmDetailCommand);
            ManualControlCommand = new RelayCommand<ControlInfoByManual>(doManualControlCommand);
        }

        #region 设备状态统计
        public models.Variable Temperature { get; set; }
        public models.Variable Humidity { get; set; }
        public models.Variable PM { get; set; }
        public models.Variable Pressure { get; set; }
        public models.Variable FlowRate { get; set; }

        public List<RankingItem> RankingList { get; set; }

        //用气排行
        private void initGasRankList()
        {
            Random random = new Random();

            string[] quality = new string[] { "车间-1", "车间-2", "车间-3", "车间-4",
                "车间-5" };
            RankingList = new List<RankingItem>();
            foreach (var q in quality)
            {
                RankingList.Add(new RankingItem()
                {
                    Header = q,
                    PlanValue = random.Next(100, 200),
                    FinishedValue = random.Next(10, 150),
                    TotalValue = 240
                });
            }
        }
        #endregion

        #region 设备提醒
        public List<MonitorWarnning> WarnningList { get; set; }

        public RelayCommand<Device> AlarmDetailCommand { get; set; }

        private void doAlarmDetailCommand(Device device)
        {
            var alarmMenu = _mainViewModel.Menus.FirstOrDefault(m => m.TargetView == "AlarmPage");
            if (alarmMenu != null)
            {
                _mainViewModel.ShowPage(alarmMenu, context =>
                {
                    StopMonitor();
                    StopAlarmHistoryPersistence();
                }, context =>
                {
                    var dev = context.Parameters["Device"];
                    if (dev != null && dev is Device)
                    {
                        //找到当前设备所在的页
                        var alarmViewModel = ViewModelLocator.Instance.AlarmViewModel;
                        alarmViewModel.GoToPageWithAlarmDetailOnDevice(device);
                    }
                }, new Dictionary<string, object>
                {
                    { "Device", device }
                });
                alarmMenu.CheckState = true;
            }
        }

        private bool canAlarmDetailCommand(Device device)
        {
            return device != null && device.Alarms.Count > 0;
        }

        private void initAlarmList()
        {
            WarnningList = new List<MonitorWarnning>()
                {
                  new MonitorWarnning{Message= "朝夕PLT-01：保养到期",
                      DateTime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                  new MonitorWarnning{Message= "朝夕PLT-01：故障",
                      DateTime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                  new MonitorWarnning{Message= "朝夕PLT-01：保养到期",
                      DateTime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                  new MonitorWarnning{Message= "朝夕PLT-01：保养到期",
                      DateTime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                  new MonitorWarnning{Message= "朝夕PLT-01：保养到期",
                      DateTime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                  new MonitorWarnning{Message= "朝夕PLT-01：保养到期",
                      DateTime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") },
                  new MonitorWarnning{Message= "朝夕PLT-01：保养到期",
                      DateTime=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
                };
        }
        #endregion

        #region 设备运行状态及手工控制
        public RelayCommand ResetPopupWithVariableListCommand { get; private set; }

        public RelayCommand ResetPopupWithManualListCommand { get; private set; }

        public RelayCommand<ControlInfoByManual> ManualControlCommand { get; private set; }

        private void initAlarmOptions(Device device, IList<AlarmEntity> alarmEntities, IEnumerable<ConditionEntity> topConditionEntities, IEnumerable<ConditionEntity> conditionEntities)
        {
            if (device == null || string.IsNullOrWhiteSpace(device.DeviceNum))
                throw new ArgumentNullException(nameof(device));
            if (alarmEntities == null)
                return;
            IList<Alarm> alarms = new List<Alarm>();
            foreach (var alarmEntity in alarmEntities)
            {
                var alarm = new Alarm()
                {
                    AlarmNum = alarmEntity.AlarmNum,
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
            foreach (var alarm in alarms)
            {
                device.Alarms.Add(alarm);
            }
        }

        private void initControlInfoByManualOptions(Device device, IList<ControlInfoByManualEntity> manualEntities)
        {
            if (device == null || string.IsNullOrWhiteSpace(device.DeviceNum))
                throw new ArgumentNullException(nameof(device));
            if (manualEntities == null)
                return;
            IList<ControlInfoByManual> controlInfos = new List<ControlInfoByManual>();
            foreach (var manualEntity in manualEntities)
            {
                models.Variable variable = device.Variables.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v.VarNum) && v.VarNum == manualEntity.VarNum);
                if (variable == null)
                    throw new Exception($"加载手动控制选项{manualEntity.Header} 的值失败，找不到编码为{manualEntity.VarNum}的点位信息！");
                if (!string.Equals(device.DeviceNum, manualEntity.DeviceNum))
                    throw new Exception($"加载手动控制选项{manualEntity.Header} 的值失败，找不到编码为{manualEntity.DeviceNum}设备！");
                try
                {
                    var controlInfo = new ControlInfoByManual()
                    {
                        CNum = manualEntity.CNum,
                        DeviceNum = manualEntity.DeviceNum,
                        Header = manualEntity.Header,
                        Variable = variable
                    };
                    controlInfo.ValueType = variable.VarType;
                    controlInfo.Value = ObjectToStringConverter.ConvertFromString(manualEntity.Value, variable.VarType);
                    controlInfos.Add(controlInfo);
                }
                catch (Exception ex)
                {
                    if (variable != null)
                    {
                        throw new Exception($"加载手动控制选项{manualEntity.Header} 的值失败，当前值不符合类型{variable.VarType.Name}的格式！");
                    }
                    else
                    {
                        throw new Exception($"加载手动控制选项{manualEntity.Header} 的值失败，当前值格式不正确！");
                    }
                }
            }

            device.ControlInfosByManual.Clear();
            foreach (var controlInfo in controlInfos)
            {
                device.ControlInfosByManual.Add(controlInfo);
            }
        }

        private void initControlInfoByTriggerOptions(Device conditionDevice, IList<ControlInfoByTriggerEntity> linkageEntities, Dictionary<string, Device> deviceNumDict, IEnumerable<ConditionEntity> topConditionEntities, IEnumerable<ConditionEntity> conditionEntities)
        {
            if (conditionDevice == null || string.IsNullOrWhiteSpace(conditionDevice.DeviceNum))
                throw new ArgumentNullException(nameof(conditionDevice));
            if (deviceNumDict == null)
                return;
            if (linkageEntities == null)
                return;
            IList<ControlInfoByTrigger> controlInfos = new List<ControlInfoByTrigger>();
            foreach (var controlInfoEntity in linkageEntities)
            {
                string linkageDeviceNum = controlInfoEntity.LinkageDeviceNum;
                if (!deviceNumDict.TryGetValue(linkageDeviceNum, out Device? linkageDevice))
                {
                    throw new InvalidOperationException($"加载联动控制选项{controlInfoEntity.Header} 的值失败，没有找到对应编码{linkageDeviceNum}的设备！");
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
                        throw new Exception($"加载联动控制选项{controlInfoEntity.Header} 的值失败，当前值不符合类型{valueType.Name}的格式！");
                    }
                    else
                    {
                        throw new Exception($"加载联动控制选项{controlInfoEntity.Header} 的值失败，当前值格式不正确！");
                    }
                }

                models.Variable variable = linkageDevice.Variables.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v.VarNum) && v.VarNum == controlInfoEntity.VarNum);
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

        private void doResetPopupWithVariableListCommand()
        {
            if (DeviceList == null || !DeviceList.Any())
                return;
            foreach (var device in DeviceList)
            {
                device.IsShowingVariableListPopup = false; //隐藏点位信息菜单
            }
        }

        private void doResetPopupWithManualListCommand()
        {
            if (DeviceList == null || !DeviceList.Any())
                return;
            foreach (var device in DeviceList)
            {
                device.IsShowingManualListPopup = false; //隐藏手工控制选项菜单
            }
        }

        private async void doManualControlCommand(ControlInfoByManual controlInfo)
        {
            if (controlInfo == null || DeviceList == null)
                return;
            var device = DeviceList.FirstOrDefault(d => d.DeviceNum == controlInfo.DeviceNum);
            if (device == null)
                return;
            var variable = controlInfo.Variable;
            if (variable == null || string.IsNullOrWhiteSpace(variable.VarAddress))
                return;

            //根据提供的通信参数尝试连接设备
            var propDict = new Dictionary<string, ProtocolParameter>();
            foreach (var parameter in device.CommunicationParameters)
            {
                propDict.TryAdd(parameter.PropName, new ProtocolParameter
                {
                    PropName = parameter.PropName,
                    PropValue = parameter.PropValue,
                    PropValueType = parameter.PropValueType
                });
            }
            device.ConnectionState = DeviceConnectionStates.Connecting;
            if (!await _communicationService.ConnectDeviceAsync(device.DeviceNum, propDict))
            {
                device.ConnectionState = DeviceConnectionStates.Disconnected;
                _logger.LogInformation($"The device {device.Name} is not connected");
            }
            else
            {
                device.ConnectionState = DeviceConnectionStates.Connected;
            }

            if (device.ConnectionState == DeviceConnectionStates.Connected)
            {
                try
                {
                    var value = Convert.ChangeType(controlInfo.Value, controlInfo.ValueType);
                    bool b = await _communicationService.WriteAsyn(device.DeviceNum, new serv.Variable()
                    {
                        VarNum = variable.VarNum,
                        VarAddress = variable.VarAddress,
                        VarType = controlInfo.ValueType,
                        VarValue = value
                    });
                    if (!b)
                    {
                        throw new Exception("写入地址失败！");
                    }
                }
                catch(Exception ex)
                {
                    _logger.LogInformation($"Failed to write value to device {device.Name} for variable {variable.VarAddress}");
                }
            }
        }
        #endregion

        void initDataForMonitor()
        {
            initGasRankList();

            initAlarmList();

            loadComponentsFromDatabase();
        }

        public RelayCommand ConfigureComponentCommand { get; set; }

        private void showConfigureComponentDialog()
        {
            if (_mainViewModel.GlobalUserInfo == null || _mainViewModel.GlobalUserInfo.UserType == Common.UserTypes.Operator)
            {
                // 提示没有权限操作
                _mainViewModel.ShowNonPermission();
                return;
            }
            else
            {
                // 打开组件编辑窗口前，停止组件状态监视并开启存盘状态监视
                var configViewModel = ViewModelLocator.Instance.ConfigureComponentViewModel;
                configViewModel.StartSaveTrackTimer();
                StopMonitor();
                StopAlarmHistoryPersistence();

                if (ActionManager.ExecuteAndResult<object>("ShowConfigureComponentDialog", configViewModel))
                {
                    // 添加一个等待页面（预留）

                    // 刷新   配置文件/数据库
                    initDataForMonitor();
                    BeginMonitor();
                    BeginAlarmHistoryPersistence();
                }
            }
        }

        #region 设备监控
        private ObservableCollection<Device> _deviceList;
        public ObservableCollection<Device> DeviceList
        {
            get { return _deviceList; }
            set { SetProperty(ref _deviceList, value); }
        }

        private void loadComponentsFromDatabase()
        {
            var devices = new List<Device>();
            Dictionary<string, Device> deviceNumDict = new Dictionary<string, Device>();
            var deviceEntities = _localDataAccess.ReadDevices();

            //字典的键是设备编码，值是该设备的预警实体集合
            Dictionary<string, IList<AlarmEntity>> deviceAlarmDict = _localDataAccess.ReadAlarms();
            IEnumerable<ConditionEntity> topConditionEntities = _localDataAccess.GetTopConditions(); //预加载所有顶级条件选项
            IEnumerable<ConditionEntity> conditionEntities = _localDataAccess.GetConditions(); //预加载所有顶级条件选项

            //字典的键是设备编码，值是该设备的手动控制选项实体集合
            Dictionary<string, IList<ControlInfoByManualEntity>> manualEntityDict = _localDataAccess.ReadControlInfosByManual();
            //字典的键是设备编码，值是该设备的联动控制选项实体集合
            Dictionary<string, IList<ControlInfoByTriggerEntity>> linkageEntityDict = _localDataAccess.ReadControlInfosByTrigger();

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
                    IsMonitor = true
                };
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
                    foreach (var variableEntity in deviceEntity.Variables)
                    {
                        device.Variables.Add(new models.Variable()
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

                devices.Add(device);
                if (!deviceNumDict.ContainsKey(device.DeviceNum))
                {
                    deviceNumDict.Add(device.DeviceNum, device);
                }
            }
            DeviceList = new ObservableCollection<Device>(devices);

            foreach (var device in devices)
            {
                // 加载预警信息
                if (deviceAlarmDict.TryGetValue(device.DeviceNum, out IList<AlarmEntity>? alarmEntities))
                {
                    initAlarmOptions(device, alarmEntities, topConditionEntities, conditionEntities);
                }

                // 加载手动控制信息
                if (manualEntityDict.TryGetValue(device.DeviceNum, out IList<ControlInfoByManualEntity>? manualEntities))
                {
                    initControlInfoByManualOptions(device, manualEntities);
                }

                // 加载联动控制信息
                if (linkageEntityDict.TryGetValue(device.DeviceNum, out IList<ControlInfoByTriggerEntity>? linkageEntities))
                {
                    initControlInfoByTriggerOptions(device, linkageEntities, deviceNumDict, topConditionEntities, conditionEntities);
                }
            }
        }

        private CancellationTokenSource cts { get; set; }
        private readonly IList<Task> taskList = new List<Task>();

        //在当前内存中的设备预警状态监控记录，即还未保存到数据库的预警状态监控记录。当设备的预警状态发生变化时，先将变化记录添加到该集合中，再由一个定时器定期将集合中的记录保存到数据库中，并从集合中移除已保存的记录
        private ConcurrentQueue<AlarmHistoryRecord> updateAlarmStateQueue = new ConcurrentQueue<AlarmHistoryRecord>();
        //保存在数据库中的预警状态监控记录。注意：这个队列中只存放从数据库中读取的最新的历史记录，即如果同一个预警信息有多个历史记录，只存放最新的一条记录
        private ConcurrentQueue<AlarmHistoryRecord> alarmHistoryQueue = new ConcurrentQueue<AlarmHistoryRecord>();

        internal async Task BeginMonitor()
        {
            StopMonitor();

            //从数据库中读取最近的预警历史记录到内存中
            alarmHistoryQueue.Clear();
            var recentAlarmHistoryRecords = _localDataAccess.ReadRecentAlarms();
            recentAlarmHistoryRecords.SelectMany(p => p.Value).ToList().ForEach(p => alarmHistoryQueue.Enqueue(p));

            cts = new CancellationTokenSource();
            foreach(var device in DeviceList)
            {
                Task task = Task.Run(async () =>
                {
                    while (!cts.IsCancellationRequested)
                    {
                        var propDict = new Dictionary<string, ProtocolParameter>();
                        foreach (var parameter in device.CommunicationParameters)
                        {
                            propDict.TryAdd(parameter.PropName, new ProtocolParameter
                            {
                                PropName = parameter.PropName,
                                PropValue = parameter.PropValue,
                                PropValueType = parameter.PropValueType
                            });
                        }
                        device.ConnectionState = DeviceConnectionStates.Connecting;
                        if (!await _communicationService.ConnectDeviceAsync(device.DeviceNum, propDict))
                        {
                            device.ConnectionState = DeviceConnectionStates.Disconnected;
                            _logger.LogInformation($"The device {device.Name} is not connected");
                        }
                        else
                        {
                            device.ConnectionState = DeviceConnectionStates.Connected;
                        }

                        if (device.ConnectionState == DeviceConnectionStates.Connected)
                        {
                            foreach (var variable in device.Variables)
                            {
                                var @var = new serv.Variable()
                                {
                                    VarNum = variable.VarNum,
                                    VarAddress = variable.VarAddress,
                                    VarType = variable.VarType
                                };
                                var val = await _communicationService.ReadAsync(device.DeviceNum, @var);
                                variable.Value = val;
                            }

                            //预警条件
                            bool isWarning = false;
                            foreach (var alarm in device.Alarms)
                            {
                                //因为设备中的点位信息是实时变化的，所以在每次计算预警状态时，需要将当前设备中的点位信息更新到预警条件中，从而保证调用IsMatch方法时检测的变量值是最新的，保证预警状态监控的准确性
                                alarm.Condition?.UpdateSourceVariables(device.Variables.ToList());

                                bool isMatching = alarm.Condition.IsMatch();
                                if (isMatching)
                                {
                                    isWarning = true;
                                    alarm.AlarmState = new AlarmState(AlarmStatus.Unsolved);
                                    alarm.AlarmTime = DateTime.Now;
                                    
                                }
                                else
                                {
                                    alarm.AlarmState = new AlarmState(AlarmStatus.SolvedBySystem);
                                    alarm.SolvedTime = DateTime.Now;
                                }
                                IList<models.Variable> alarmVariables = new List<models.Variable>();
                                alarm.Condition.GetSourceVariables().ToList().ForEach(v =>
                                {
                                    var deviceVariable = device.Variables.FirstOrDefault(dv => dv.VarNum == v.VarNum);
                                    if (deviceVariable != null)
                                    {
                                        alarmVariables.Add(new models.Variable()
                                        {
                                            VarNum = v.VarNum,
                                            VarName = v.VarName,
                                            VarType = v.VarType,
                                            DeviceNum = v.DeviceNum,
                                            Value = deviceVariable.Value
                                        });
                                    }
                                });
                                alarm.AlarmValues = alarmVariables;

                                var recordFromMemory = updateAlarmStateQueue.FirstOrDefault(r => string.Equals(r.DeviceNum, device.DeviceNum) && string.Equals(r.AlarmNum, alarm.AlarmNum));
                                if (recordFromMemory == null)
                                {
                                    var recordFromDB = alarmHistoryQueue.FirstOrDefault(r => string.Equals(r.DeviceNum, device.DeviceNum) && string.Equals(r.AlarmNum, alarm.AlarmNum));
                                    if (recordFromDB == null) //如果内存和数据库中都没有该预警信息的监控历史记录，说明这是一条新的预警实时信息，需要添加到数据库
                                    {
                                        if (isMatching)
                                        {
                                            var newRecord = new AlarmHistoryRecord()
                                            {
                                                DeviceNum = device.DeviceNum,
                                                AlarmNum = alarm.AlarmNum,
                                                AlarmState = alarm.AlarmState != null ? Enum.GetName(typeof(AlarmStatus), alarm.AlarmState.Status) : null,
                                                AlarmTime = alarm.AlarmTime
                                            };
                                            updateAlarmStateQueue.Enqueue(newRecord);
                                        }
                                    }
                                    else //如果内存中没有但数据库中有该预警信息的监控历史记录，则需要判断数据库中相关预警最近的历史记录的状态与当前预警信息的状态是否一致，如果不一致，则说明预警状态发生了变化，需要添加一条新的监控历史记录到数据库中
                                    {
                                        if (alarm.AlarmState != null && !string.Equals(recordFromDB.AlarmState, Enum.GetName(typeof(AlarmStatus), alarm.AlarmState.Status)))
                                        {
                                            var newRecord = new AlarmHistoryRecord()
                                            {
                                                DeviceNum = device.DeviceNum,
                                                AlarmNum = alarm.AlarmNum,
                                                AlarmTime = alarm.AlarmTime
                                            };
                                            if (string.Equals(recordFromDB.AlarmState, Enum.GetName(typeof(AlarmStatus), AlarmStatus.SolvedByManual))) //如果最近的预警状态是SolvedByManual，说明该预警信息正在进行人工干预，这时该预警状态将不需要监控。如果要再次监控此预警状态，则需要用户添加新的预警条件信息
                                            {
                                                continue;
                                            }
                                            if (isMatching) //当前是报警状态
                                            {
                                                newRecord.AlarmState = Enum.GetName(typeof(AlarmStatus), alarm.AlarmState.Status);
                                            }
                                            else //当前是非报警状态
                                            {
                                                if (!string.Equals(recordFromDB.AlarmState, Enum.GetName(typeof(AlarmStatus), AlarmStatus.SolvedByManual))) //当前是已处理状态，但数据库中最近的历史记录是已人工处理状态，此时不需要添加新的监控历史记录到数据库中，因为系统已处理和人工已处理都是已处理状态
                                                {
                                                    newRecord.AlarmState = Enum.GetName(typeof(AlarmStatus), alarm.AlarmState.Status);
                                                }
                                            }
                                            if (!string.IsNullOrEmpty(recordFromDB.AlarmState)) //如果当前是报警状态或者是从报警转变为已处理状态，则需要添加一条新的监控历史记录到数据库中
                                            {
                                                updateAlarmStateQueue.Enqueue(newRecord);
                                            }
                                        }
                                    }
                                }
                                else //如果内存中已经有该预警信息的监控历史记录，则需要判断内存中相关预警最近的历史记录的状态与当前预警信息的状态是否一致，如果不一致，则说明预警状态发生了变化，需要添加一条新的监控历史记录到数据库中
                                {
                                    if (alarm.AlarmState != null && !string.Equals(recordFromMemory.AlarmState, Enum.GetName(typeof(AlarmStatus), alarm.AlarmState.Status)))
                                    {
                                        var newRecord = new AlarmHistoryRecord()
                                        {
                                            DeviceNum = device.DeviceNum,
                                            AlarmNum = alarm.AlarmNum,
                                            AlarmTime = alarm.AlarmTime
                                        };
                                        if (string.Equals(recordFromMemory.AlarmState, Enum.GetName(typeof(AlarmStatus), AlarmStatus.SolvedByManual))) //如果现在是人工处理状态，则该预警状态将不需要监控
                                        {
                                            continue;
                                        }
                                        if (isMatching) //当前是报警状态
                                        {
                                            newRecord.AlarmState = Enum.GetName(typeof(AlarmStatus), alarm.AlarmState.Status);
                                        }
                                        else //当前是非报警状态
                                        {
                                            if (!string.Equals(recordFromMemory.AlarmState, Enum.GetName(typeof(AlarmStatus), AlarmStatus.SolvedByManual))) //当前是已处理状态，但数据库中最近的历史记录是已人工处理状态，此时不需要添加新的监控历史记录到数据库中，因为系统已处理和人工已处理都是已处理状态
                                            {
                                                newRecord.AlarmState = Enum.GetName(typeof(AlarmStatus), alarm.AlarmState.Status);
                                            }
                                        }
                                        if (!string.IsNullOrEmpty(recordFromMemory.AlarmState)) //如果当前是报警状态或者是从报警转变为已处理状态，则需要添加一条新的监控历史记录到数据库中
                                        {
                                            updateAlarmStateQueue.Enqueue(newRecord);
                                        }
                                    }
                                }
                            }
                            device.IsWarning = isWarning;
                        }
                        await Task.Delay(5000);
                    }
                });
                task.ContinueWith(t =>
                {
                    if (t.Exception != null)
                    {
                        _logger.LogError($"An error occurred while monitoring device {device.Name}: {t.Exception}");
                    }
                }, TaskContinuationOptions.OnlyOnFaulted);
                taskList.Add(task);
            }
        }

        internal async Task StopMonitor()
        {
            if (cts != null)
            {
                cts.Cancel();
                //等待所有监视任务结束
                await Task.WhenAll(taskList);
                taskList.Clear();
            }
        }

        //开启定时器，每隔一段时间将队列中的所有预警历史记录一次性保存到数据库中
        private CancellationTokenSource cts_alarm_history { get; set; }
        private Task task_alarm_history;
        internal async Task BeginAlarmHistoryPersistence()
        {
            if (!await StopAlarmHistoryPersistence())
            {
                return;
            }

            cts_alarm_history = new CancellationTokenSource();
            task_alarm_history = Task.Run(async () =>
            {
                while (!cts_alarm_history.IsCancellationRequested)
                {
                    if (updateAlarmStateQueue.Any())
                    {
                        IList<AlarmHistoryRecord> alarmHistoryRecords = updateAlarmStateQueue.ToList();
                        try
                        {                            
                            _localDataAccess.BatchUpdateAlarmHistory(alarmHistoryRecords);

                            updateAlarmStateQueue.Clear(); //当队列中的所有预警历史记录都保存到数据库后，需要会清空队列，从而保证预警历史记录在内存或数据库中是同一份数据

                            //当队列中的所有预警历史记录都保存到数据库后，需要从数据库读取相关预警信息的最新的历史记录到内存中，从而确保alarmHistoryQueue保存的是数据库中最后添加的预警历史记录，而updateAlarmStateQueue内存中保存的是写入数据库后新创建的预警历史记录
                            alarmHistoryQueue.Clear();
                            var recentAlarmHistoryRecords = _localDataAccess.ReadRecentAlarms();
                            recentAlarmHistoryRecords.SelectMany(p => p.Value).ToList().ForEach(p => alarmHistoryQueue.Enqueue(p));
                        }
                        catch (Exception ex)
                        {
                        }
                    }
                    else
                    {
                        await Task.Delay(TimeSpan.FromSeconds(30));
                    }
                }
            });
        }

        internal async Task<bool> StopAlarmHistoryPersistence()
        {
            if (cts_alarm_history != null && task_alarm_history != null)
            {
                cts_alarm_history.Cancel();
                await Task.WhenAll(task_alarm_history);
                try
                {
                    task_alarm_history?.Dispose();
                    task_alarm_history = null;
                    cts_alarm_history.TryReset();

                    return true;
                }
                catch(Exception ex)
                {
                    return false;
                }
            }
            else
            {
                return true;
            }
        }

        public void OnNavigateTo(NavigationContext context = null)
        {
            BeginMonitor();
            BeginAlarmHistoryPersistence();
        }

        public void OnNavigateFrom(NavigationContext context = null)
        {
            StopMonitor();
            StopAlarmHistoryPersistence();
        }
        #endregion
    }
}
