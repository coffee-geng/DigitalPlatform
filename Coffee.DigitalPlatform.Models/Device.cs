using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Models
{
    public class Device : CheckableTreeItem
    {
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
    }

    public enum FlowDirections
    {
        Clockwise,
        Anticlockwise
    }
}
