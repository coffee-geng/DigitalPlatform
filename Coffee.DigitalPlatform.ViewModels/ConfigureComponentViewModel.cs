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

            SaveDeviceConfigurationCommand = new RelayCommand<object>(doSaveDeviceConfigurationCommand);
            CloseCommand = new RelayCommand<object>(doCloseCommand);

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
            set { SetProperty(ref _currentDevice, value); }
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
                DeviceNum = "D" + DateTime.Now.ToString("yyyyMMddHHmmssFFF"),
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
                DeviceList.Add(device);
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
                    deviceEntities.Add(deviceEntity);
                }

                _localDataAccess.SaveDevices(deviceEntities);

                VisualStateManager.GoToElementState(owner as Window, "ShowSuccess", true);
            }
            catch(Exception ex)
            {
                FailureMessageOnSaving = ex.Message;
                VisualStateManager.GoToElementState(owner as Window, "ShowFailure", true);
            }
        }
        #endregion
    }
}
