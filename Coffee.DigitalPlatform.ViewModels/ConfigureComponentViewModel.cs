using Coffee.DigitalPlatform.CommWPF;
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

            TestData.Add("aaa");
            TestData.Add("bbb");
        }

        public ObservableCollection<string> TestData { get; set; } = new ObservableCollection<string>();

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

        public RelayCommand<DragEventArgs> CreateComponentByDragCommand { get; set; }

        public RelayCommand<Device> SelectDeviceCommand { get; set; }

        private void initComponentGroups()
        {

        }

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
                GetDevices = () => DeviceList.ToList()
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
        }
        #endregion

        #region 通信参数
        public List<CommunicationParameter> CommunicationParameters { get; set; }

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
    }
}
