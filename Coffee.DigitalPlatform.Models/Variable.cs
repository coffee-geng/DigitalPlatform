using Coffee.DigitalPlatform.Common;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Xml.Linq;

namespace Coffee.DigitalPlatform.Models
{
    public class Variable : ObservableObject, IDataErrorInfo
    {
        // 设备编码
        private string _deviceNum;
        public string DeviceNum
        {
            get { return _deviceNum; }
            set { SetProperty(ref _deviceNum, value); }
        }

        // 变量编码
        private string _varNum;
        public string VarNum
        {
            get { return _varNum; }
            set { SetProperty(ref _varNum, value); }
        }

        // 变量名称
        private string _varName;
        public string VarName
        {
            get { return _varName; }
            set { SetProperty(ref _varName, value); }
        }

        // 变量地址
        private string _varAddress;
        public string VarAddress
        {
            get { return _varAddress; }
            set { SetProperty(ref _varAddress, value); }
        }

        // 变量类型
        private Type _varType;
        public Type VarType
        {
            get { return _varType; }
            set { SetProperty(ref _varType, value); }
        }

        // 变量是否可空类型
        private bool _isNullableVar;
        public bool IsNullableVar
        {
            get { return _isNullableVar; }
            set { SetProperty(ref _isNullableVar, value); }
        }

        // 偏移量
        private double _offset;
        public double Offset
        {
            get { return _offset; }
            set { SetProperty(ref _offset, value); }
        }

        // 系数
        private double _factor = 1;
        public double Factor
        {
            get { return _factor; }
            set { SetProperty(ref _factor, value); }
        }

        // 变量值，即从设备指定点位中读取或写入的值
        public object Value { get; set; }

        private Dictionary<string, string> _errorDict = new Dictionary<string, string>();

        public string Error => _errorDict.Values.FirstOrDefault();

        public string this[string columnName]
        {
            get
            {
                string err = string.Empty;
                switch (columnName)
                {
                    case nameof(VarName):
                        err = verifyVarName();
                        break;
                    case nameof(VarAddress):
                        err = verifyVarAddress();
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
                return err;
            }
        }

        private string verifyVarName()
        {
            if (string.IsNullOrWhiteSpace(VarName))
            {
                return "点位名称不能为空";
            }
            else if (VarName.Length < 3 || VarName.Length > 20)
            {
                return "姓名长度应在3-20个字符之间";
            }
            else if (ValidateDuplication != null && !ValidateDuplication(this, VarName))
            {
                return "点位名称在当前设备中不能重复";
            }
            return string.Empty;
        }

        private string verifyVarAddress()
        {
            if (string.IsNullOrWhiteSpace(VarAddress))
            {
                return "点位地址不能为空";
            }
            return string.Empty;
        }

        //验证属性值是否有重复，返回true表示不重复，false表示重复
        public Func<Variable, string, bool> ValidateDuplication { get; set; }
    }
}
