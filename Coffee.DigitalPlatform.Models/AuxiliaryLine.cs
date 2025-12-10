using Coffee.DigitalPlatform.CommWPF;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Models
{
    public class AuxiliaryLine : ObservableObject, IAuxiliaryLineContext
    {
        public AuxiliaryLineTypes AuxiliaryType { get; set; }

        private bool _isVisible = true;
        public bool IsVisible
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
        #endregion
    }
}
