using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Models
{
    public class SettingInfo : ObservableObject
    {
        private string _infoNum = string.Empty;
        public string InfoNum
        {
            get { return _infoNum; }
            set { SetProperty(ref _infoNum, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return _description; }
            set { SetProperty(ref _description, value); }
        }

        private string _deviceNum;
        public string DeviceNum
        {
            get { return _deviceNum; }
            set
            {
                if (SetProperty(ref _deviceNum, value))
                {
                    if (string.IsNullOrEmpty(value)) return;

                    //根据选中的设备，更新设备相关的点位信息
                    var device = DeviceList.FirstOrDefault(d => d.DeviceNum == value);
                    if (device == null) return;

                    VariableNum = "";
                    VariableList.Clear();
                    device.Variables.ToList().ForEach(v => VariableList.Add(v));

                    VariableNum = VariableList.FirstOrDefault()?.VarNum;
                }
            }
        }

        private IList<Device> _deviceList;
        public IList<Device> DeviceList
        {
            get { return _deviceList; }
            set { SetProperty(ref _deviceList, value); }
        }

        private string _variableNum;
        public string VariableNum
        {
            get { return _variableNum; }
            set { SetProperty(ref _variableNum, value); }
        }
        
        public ObservableCollection<Variable> VariableList { get; private set; } = new ObservableCollection<Variable>();
    }

    public enum SettingInfoTypes
    {
        SystemInfo = 1,
        MonitorInfo = 2,
    }
}
