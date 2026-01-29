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

namespace Coffee.DigitalPlatform.ViewModels
{
    public class MonitorViewModel : ObservableObject
    {
        MainViewModel _mainViewModel;
        ILocalDataAccess _localDataAccess;

        public MonitorViewModel(MainViewModel mainViewModel, ILocalDataAccess localDataAccess)
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

        #region 点位信息或手动控制选项
        public RelayCommand ResetPopupWithVariableListCommand { get; private set; }

        public RelayCommand ResetPopupWithManualListCommand { get; private set; }
        private void initControlInfoByManualOptions(Device device, Dictionary<string, IList<ControlInfoByManualEntity>> manualEntityDict)
        {
            if (device == null || string.IsNullOrWhiteSpace(device.DeviceNum))
                throw new ArgumentNullException(nameof(device));
            if (manualEntityDict == null || !manualEntityDict.ContainsKey(device.DeviceNum))
                return;
            var manualEntities = manualEntityDict[device.DeviceNum];
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
                if (ActionManager.ExecuteAndResult<object>("ShowConfigureComponentDialog", ViewModelLocator.Instance.ConfigureComponentViewModel))
                {
                    // 添加一个等待页面（预留）

                    // 可能会有耗时控件
                    //cts.Cancel();
                    //Task.WaitAll(tasks.ToArray());

                    //cts = new CancellationTokenSource();
                    //tasks.Clear();

                    //// 刷新   配置文件/数据库
                    //ComponentsInit();
                    //// 启动监听
                    //this.Monitor();
                }
            }
        }

        #region 设备监控
        public ObservableCollection<Device> DeviceList { get; private set; }

        private void loadComponentsFromDatabase()
        {
            var devices = new List<Device>();
            var deviceEntities = _localDataAccess.ReadDevices();
            Dictionary<string, IList<ControlInfoByManualEntity>> manualEntityDict = _localDataAccess.ReadControlInfosByManual();
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

                // 加载手动控制信息
                initControlInfoByManualOptions(device, manualEntityDict);
                devices.Add(device);
            }
            DeviceList = new ObservableCollection<Device>(devices);
        }
        #endregion
    }
}
