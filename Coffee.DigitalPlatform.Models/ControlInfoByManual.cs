using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Models
{
    public class ControlInfoByManual : ObservableObject, IDataErrorInfo
    {
        //手动控制信息的编码
        private string _cNum;
        public string CNum
        {
            get { return _cNum; }
            set { SetProperty(ref _cNum, value); }
        }

        // 设备编码
        private string _deviceNum;
        public string DeviceNum
        {
            get { return _deviceNum; }
            set { SetProperty(ref _deviceNum, value); }
        }

        private string _header;
        public string Header
        {
            get => _header;
            set => SetProperty(ref _header, value);
        }

        private Variable _variable;
        public Variable Variable
        {
            get => _variable;
            set => SetProperty(ref _variable, value);
        }

        private object _value;
        public object Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        private Type _valueType = typeof(string);
        public Type ValueType
        {
            get => _valueType;
            set => SetProperty(ref _valueType, value);
        }

        private Dictionary<string, string> _errorDict = new Dictionary<string, string>();

        public string Error => _errorDict.Values.FirstOrDefault();

        public string this[string columnName]
        {
            get
            {
                string err = string.Empty;
                switch (columnName)
                {
                    case nameof(Header):
                        err = verifyHeader();
                        break;
                    case nameof(Variable):
                        err = verifyVariable();
                        break;
                    case nameof(Value):
                        err = verifyValue();
                        break;
                }
                if (!string.IsNullOrWhiteSpace(err))
                {
                    if (!_errorDict.ContainsKey(columnName))
                    {
                        _errorDict.Add(columnName, err);
                    }
                    else
                    {
                        _errorDict[columnName] = err;
                    }
                }
                else
                {
                    _errorDict.Remove(columnName);
                }
                return err;
            }
        }

        private string verifyHeader()
        {
            if (string.IsNullOrWhiteSpace(Header))
            {
                return "手动控制选项名字不能为空";
            }
            return string.Empty;
        }

        private string verifyVariable()
        {
            if (Variable == null || string.IsNullOrWhiteSpace(Variable.VarName))
            {
                return "点位地址不能为空";
            }
            return string.Empty;
        }

        private string verifyValue()
        {
            if (Value == null)
            {
                return "点位值不能为空";
            }
            return string.Empty;
        }
    }
}
