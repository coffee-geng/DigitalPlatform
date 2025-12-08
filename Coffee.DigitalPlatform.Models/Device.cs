using Coffee.DigitalPlatform.CommWPF;
using Coffee.DigitalPlatform.IDataAccess;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Coffee.DigitalPlatform.Models
{
    public class Device : ObservableObject, IComponentContext
    {
        ILocalDataAccess _localDataAccess;

        public Device(ILocalDataAccess localDataAccess)
        {
            _localDataAccess = localDataAccess;
        }

        // 设备编号
        public string DeviceNum { get; set; }

        // 设备名称
        public string Name { get; set; }

        private bool? _isVisible = false;
        public bool? IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (SetProperty(ref _isSelected, value))
                {
                    if (value)
                    {
                        z_temp = this.Z;
                        this.Z = 999;
                    }
                    else
                    {
                        this.Z = z_temp;
                    }
                }
            }
        }

        private bool _isMonitor = false;
        public bool IsMonitor
        {
            get { return _isMonitor; }
            set { SetProperty(ref _isMonitor, value); }
        }

        #region 设备在视图上的位置信息
        private double _x;
        public double X
        {
            get { return _x; }
            set { SetProperty(ref _x, value); }
        }

        private double _y;
        public double Y
        {
            get { return _y; }
            set { SetProperty(ref _y, value); }
        }

        private int z = 0;
        private int z_temp = 0;

        public int Z
        {
            get { return z; }
            set { SetProperty(ref z, value); }
        }

        private double _width;
        public double Width
        {
            get { return _width; }
            set { SetProperty(ref _width, value); }
        }

        private double _height;
        public double Height
        {
            get { return _height; }
            set { SetProperty(ref _height, value); }
        }

        private double _rotate;
        public double Rotate
        {
            get { return _rotate; }
            set { SetProperty(ref _rotate, value); }
        }
        #endregion

        // 流体的流向
        private FlowDirections _flowDirection;
        public FlowDirections FlowDirection
        {
            get { return _flowDirection; }
            set { SetProperty(ref _flowDirection, value); }
        }

        // 根据这个名称动态创建一个组件实例
        public string DeviceType { get; set; }

        public ObservableCollection<CommunicationParameter> CommunicationParameters { get; private set; } = new ObservableCollection<CommunicationParameter>();

        public ObservableCollection<Variable> Variables { get; private set; } = new ObservableCollection<Variable>();

        public RelayCommand<Device> DeleteCommand { get; set; }

        #region IComponentContext实现
        public Func<List<Device>> GetDevices { get; set; }

        public IEnumerable<IComponentContext> GetComponentsToCheckAlign()
        {
            if (GetDevices == null)
                return null;
            var devices = GetDevices();
            return devices.Where(d => !new string[] { "HorizontalLine", "VerticalLine", "WidthRuler", "HeightRuler" }.Contains(d.DeviceType) && d != this).ToList();
        }

        public IEnumerable<IComponentContext> GetRulers()
        {
            if (GetDevices == null)
                return null;
            var devices = GetDevices();
            return devices.Where(d => new string[] { "HorizontalLine", "VerticalLine" }.Contains(d.DeviceType));
        }
        #endregion

        #region 初始化右键菜单 
        public List<Control> ContextMenus { get; set; }

        public void InitContextMenu()
        {
            ContextMenus = new List<Control>();
            ContextMenus.Add(new MenuItem
            {
                Header = "顺时针旋转",
                Command = new RelayCommand(() => this.Rotate += 90),
                Visibility = new string[] {
                    "RAJoints", "TeeJoints","Temperature","Humidity","Pressure","Flow","Speed"
                }.Contains(this.DeviceType) ? Visibility.Visible : Visibility.Collapsed
            });
            ContextMenus.Add(new MenuItem
            {
                Header = "逆时针旋转",
                Command = new RelayCommand(() => this.Rotate -= 90),
                Visibility = new string[] {
                    "RAJoints", "TeeJoints","Temperature","Humidity","Pressure","Flow","Speed"
                }.Contains(this.DeviceType) ? Visibility.Visible : Visibility.Collapsed
            });
            ContextMenus.Add(new MenuItem
            {
                Header = "改变流向",
                Command = new RelayCommand(() => {
                if (FlowDirection == FlowDirections.Clockwise)
                    this.FlowDirection = FlowDirections.Anticlockwise;
                else
                    this.FlowDirection = FlowDirections.Clockwise;
                }),
                Visibility = new string[] { "HorizontalPipeline", "VerticalPipeline" }.Contains(this.DeviceType) ? Visibility.Visible : Visibility.Collapsed
            });

            ContextMenus.Add(new Separator());

            ContextMenus.Add(new MenuItem
            {
                Header = "向上一层",
                Command = new RelayCommand(() => this.Z++)
            });
            ContextMenus.Add(new MenuItem
            {
                Header = "向下一层",
                Command = new RelayCommand(() => this.Z--)
            });
            ContextMenus.Add(new Separator { });

            ContextMenus.Add(new MenuItem
            {
                Header = "删除",
                Command = this.DeleteCommand,
                CommandParameter = this
            });

            var cms = ContextMenus.Where(cm => cm.Visibility == Visibility.Visible).ToList();
            foreach (var item in cms)
            {
                if (item is Separator)
                    item.Visibility = Visibility.Collapsed;
                else
                    break;
            }

        }
        #endregion
    }

    public enum FlowDirections
    {
        Clockwise,
        Anticlockwise
    }
}
