using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.CommWPF
{
    public class AuxiliaryInfo : ObservableObject
    {
        public AuxiliaryInfo(AuxiliaryLineTypes auxiliaryType)
        {
            AuxiliaryType = auxiliaryType;
            switch (auxiliaryType)
            {
                case AuxiliaryLineTypes.VerticalLine:
                    Width = 1;
                    Height = 2000;
                    break;
                case AuxiliaryLineTypes.HorizontalLine:
                    Width = 2000;
                    Height = 1;
                    break;
            }
        }

        public AuxiliaryLineTypes AuxiliaryType {  get; private set; }

        private bool _isVisible = false;
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetProperty(ref _isVisible, value); }
        }

        private double _width = 0;
        public double Width
        {
            get { return _width; }
            set { SetProperty(ref _width, value); }
        }

        private double _height = 0;
        public double Height
        {
            get { return _height; }
            set { SetProperty(ref _height, value); }
        }

        private double _x = 0;
        public double X
        {
            get { return _x; }
            set { SetProperty(ref _x, value); }
        }

        private double _y = 0;
        public double Y
        {
            get { return _y; }
            set { SetProperty(ref _y, value); }
        }

        private int _z = 0;
        public int Z
        {
            get { return _z; }
            set { SetProperty(ref _z, value); }
        }
    }
}
