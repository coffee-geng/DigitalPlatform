using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Models
{
    public class TrendDeviceInfo : ObservableObject
    {
        private string _header;
        public string Header
        {
            get { return _header; }
            set { SetProperty(ref _header, value); }
        }

        public ObservableCollection<TrendVariableInfo> Variables { get; set; }
    }

    public class TrendVariableInfo : ObservableObject
    {
        public string DeviceNum { get; set; }
        public string VariableNum { get; set; }

        public string AxisXNum { get; set; }
        public string AxisYNum { get; set; }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        private string _variableName;
        public string VariableName
        {
            get { return _variableName; }
            set { SetProperty(ref _variableName, value); }
        }

        private Type _variableType;
        public Type VariableType
        {
            get { return _variableType; }
            set { SetProperty(ref _variableType, value); }
        }

        private string _color;
        public string Color
        {
            get { return _color; }
            set { SetProperty(ref _color, value); }
        }
    }
}
