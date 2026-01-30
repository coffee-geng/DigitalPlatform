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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Coffee.DigitalPlatform.ViewModels
{
    public class MonitorComponentViewModel : AbstractComponentViewModel
    {
        MainViewModel _mainViewModel;
        ILocalDataAccess _localDataAccess;

        public MonitorComponentViewModel(MainViewModel mainViewModel, ILocalDataAccess localDataAccess) : base()
        {
            _mainViewModel = mainViewModel;
            _localDataAccess = localDataAccess;

            initDataForMonitor();

            ConfigureComponentCommand = new RelayCommand(showConfigureComponentDialog);
            ResetPopupWithVariableListCommand = new RelayCommand(doResetPopupWithVariableListCommand);
            ResetPopupWithManualListCommand = new RelayCommand(doResetPopupWithManualListCommand);
        }

        #region 设备状态统计
        public Variable Temperature { get; set; }
        public Variable Humidity { get; set; }
        public Variable PM { get; set; }
        public Variable Pressure { get; set; }
        public Variable FlowRate { get; set; }

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
                Variable variable = device.Variables.FirstOrDefault(v => !string.IsNullOrWhiteSpace(v.VarNum) && v.VarNum == manualEntity.VarNum);
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
        #endregion

        void initDataForMonitor()
        {
            initGasRankList();

            initAlarmList();

            loadComponentsFromDatabase();
        }

        public RelayCommand ConfigureComponentCommand {  get; set; }

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
                // 可以打开编辑   启动窗口   主动
                var configViewModel = ViewModelLocator.Instance.ConfigureComponentViewModel;
                configViewModel.StartSaveTrackTimer();
                if (ActionManager.ExecuteAndResult<object>("ShowConfigureComponentDialog", configViewModel))
                {
                    // 添加一个等待页面（预留）

                    // 可能会有耗时控件
                    //cts.Cancel();
                    //Task.WaitAll(tasks.ToArray());

                    //cts = new CancellationTokenSource();
                    //tasks.Clear();

                    // 刷新   配置文件/数据库
                    initDataForMonitor();
                    // 启动监听
                    //this.Monitor();
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
                    IsMonitor = true,
                    IsWarning = true
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
        #endregion
    }
}
