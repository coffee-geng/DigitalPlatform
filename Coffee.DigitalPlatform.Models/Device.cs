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
using Coffee.DigitalPlatform.Common;

namespace Coffee.DigitalPlatform.Models
{
    public class Device : ObservableObject, IComponentContext
    {
        ILocalDataAccess _localDataAccess;

        public Device(ILocalDataAccess localDataAccess)
        {
            _localDataAccess = localDataAccess;


            CommunicationParameters.CollectionChanged += CommunicationParameters_CollectionChanged;

            AddCommunicationParameter = new RelayCommand<CommunicationParameter>(doAddCommunicationParameter);
            RemoveCommunicationParameter = new RelayCommand<CommunicationParameter>(doRemoveCommunicationParameter, canRemoveCommunicationParameter);
            RecommandCommunicationParameter = new RelayCommand(doRecommandCommunicationParameter);
            SelectCommunicationParameterValueCommand = new RelayCommand<SelectCommunicationParameterValueCommandParameter>(doSelectCommunicationParameterValue);

            AddVariableCommand = new RelayCommand<Variable>(doAddVariable);
            RemoveVariableCommand = new RelayCommand<Variable>(doRemoveVariable, canRemoveVariable);
        }

        // 设备编号
        public string DeviceNum { get; set; }

        // 设备名称
        public string Name { get; set; }

        private bool? _isVisible = true;
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

        public RelayCommand<Device> DeleteCommand { get; set; }

        #region IComponentContext实现
        public Func<List<IComponentContext>> GetDevices { get; set; }

        public Func<List<IAuxiliaryLineContext>> GetAuxiliaryLines { get; set; }

        public IEnumerable<IComponentContext> GetComponentsToCheckAlign()
        {
            if (GetDevices == null)
                return null;
            var devices = GetDevices();
            return devices.Where(d => d is Device && d != this).Cast<IComponentContext>().ToList();
        }

        public IEnumerable<IAuxiliaryLineContext> GetRulers()
        {
            if (GetAuxiliaryLines == null)
                return null;
            var auxiliaryLines = GetAuxiliaryLines();
            return auxiliaryLines.Where(d => d.AuxiliaryType == AuxiliaryLineTypes.HorizontalRuler || d.AuxiliaryType == AuxiliaryLineTypes.VerticalRuler);
        }

        public IEnumerable<IAuxiliaryLineContext> GetRulers(AuxiliaryLineTypes auxiliaryType)
        {
            var rulers = GetRulers();
            if (rulers != null)
            {
                if (auxiliaryType == AuxiliaryLineTypes.HorizontalRuler)
                {
                    return rulers.Where(d => d.AuxiliaryType == AuxiliaryLineTypes.HorizontalRuler);
                }
                else if (auxiliaryType == AuxiliaryLineTypes.VerticalRuler)
                {
                    return rulers.Where(d => d.AuxiliaryType == AuxiliaryLineTypes.VerticalRuler);
                }
            }
            return rulers;
        }

        public IEnumerable<IAuxiliaryLineContext> GetLinesToAlign()
        {
            if (GetAuxiliaryLines == null)
                return null;
            var auxiliaryLines = GetAuxiliaryLines();
            return auxiliaryLines.Where(d => d.AuxiliaryType == AuxiliaryLineTypes.HorizontalLine || d.AuxiliaryType == AuxiliaryLineTypes.VerticalLine);
        }

        public IEnumerable<IAuxiliaryLineContext> GetLinesToAlign(AuxiliaryLineTypes auxiliaryType)
        {
            var lines = GetLinesToAlign();
            if (lines != null)
            {
                if (auxiliaryType == AuxiliaryLineTypes.HorizontalLine)
                {
                    return lines.Where(d => d.AuxiliaryType == AuxiliaryLineTypes.HorizontalLine);
                }
                else if (auxiliaryType == AuxiliaryLineTypes.VerticalLine)
                {
                    return lines.Where(d => d.AuxiliaryType == AuxiliaryLineTypes.VerticalLine);
                }
            }
            return lines;
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
                Command = new RelayCommand(() =>
                {
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

        #region 通信参数操作
        public ObservableCollection<CommunicationParameterDefinition> CommunicationParameterDefinitions { get; private set; } = new ObservableCollection<CommunicationParameterDefinition>();

        public ObservableCollection<CommunicationParameter> CommunicationParameters { get; private set; } = new ObservableCollection<CommunicationParameter>();

        private bool _communicationParameterCollectionRefreshing;
        // 每当CommunicationParameters集合发生变化时，该属性取反一次
        // 该属性作为多路绑定的一个输入源，用于在CommunicationParameters集合发生变化时，通知界面刷新通信参数下拉框中的选项集合
        // 这样就弥补了CommunicationParameters集合变化后，不能及时通知绑定的下拉框数据源刷新选项的问题
        public bool CommunicationParameterCollectionRefreshing
        {
            get { return _communicationParameterCollectionRefreshing; }
            set { SetProperty(ref _communicationParameterCollectionRefreshing, value); }
        }

        public RelayCommand<CommunicationParameter> AddCommunicationParameter { get; set; }

        public RelayCommand<CommunicationParameter> RemoveCommunicationParameter { get; set; }

        //根据当前设备的类型及状态，推荐一个通信参数给用户
        //如果当前设备没有任何通信参数，则首先推荐通信协议参数
        //仅推荐符号当前通信协议的参数，用户已经添加的将不再重复推荐
        public RelayCommand RecommandCommunicationParameter { get; set; }

        public RelayCommand<SelectCommunicationParameterValueCommandParameter> SelectCommunicationParameterValueCommand { get; set; }

        private void CommunicationParameters_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            CommunicationParameterCollectionRefreshing = !CommunicationParameterCollectionRefreshing;
        }

        private void doAddCommunicationParameter(CommunicationParameter commParam)
        {
            if (commParam == null)
                throw new Exception("通信参数不能为空！");
            CommunicationParameters.Add(commParam);
        }

        private bool canRemoveCommunicationParameter(CommunicationParameter commParam)
        {
            if (commParam == null)
                return false;
            if (!CommunicationParameters.Contains(commParam))
                return false;
            if (commParam.PropName == "Protocol")
                return false; //通信协议参数不允许删除
            return true;
        }

        private void doRemoveCommunicationParameter(CommunicationParameter commParam)
        {
            if (commParam == null)
                throw new Exception("通信参数不能为空！");
            CommunicationParameters.Remove(commParam);
        }

        private void doRecommandCommunicationParameter()
        {
            //当设备没有添加任何通信参数时，必须先添加通信协议参数
            //之后才可添加其他符合当前设备的其他通信参数
            if (!CommunicationParameterDefinitions.Any())
            {
                //添加通信协议参数到下拉框
                var protocolEntity = _localDataAccess.GetProtocolParamDefinition();
                if (protocolEntity != null)
                {
                    var optionEntities = _localDataAccess.GetCommunicationParameterOptions(protocolEntity);
                    var protocolParamDef = new CommunicationParameterDefinition()
                    {
                        ParameterName = protocolEntity.ParameterName,
                        Label = protocolEntity.Label,
                        ValueInputType = (ValueInputTypes)protocolEntity.ValueInputType,
                        ValueDataType = protocolEntity.ValueDataType,
                        DefaultOptionIndex = protocolEntity.DefaultOptionIndex,
                        DefaultValueOption = protocolEntity.DefaultValueOption,
                        ValueOptions = optionEntities != null && optionEntities.Any() ?
                                       optionEntities.Select(o => new CommunicationParameterOption
                                       {
                                           PropName = o.PropName,
                                           PropOptionValue = o.PropOptionValue,
                                           PropOptionLabel = o.PropOptionLabel
                                       }).ToList() : null
                    };
                    CommunicationParameterDefinitions.Add(protocolParamDef);
                    //添加通信协议参数到设备的通信参数列表
                    var protocolParam = new CommunicationParameter
                    {
                        PropName = protocolEntity.ParameterName,
                        PropValue = optionEntities != null && optionEntities.Any() ?
                                     optionEntities[protocolEntity.DefaultOptionIndex].PropOptionValue : string.Empty,
                        PropValueType = protocolEntity.ValueDataType
                    };
                    CommunicationParameters.Add(protocolParam);
                }
            }
            else
            {
                var protocolName = CommunicationParameters.Where(param => param.PropName == "Protocol").Select(param => param.PropValue).FirstOrDefault();
                if (protocolName == null)
                    return;
                var commParamDefs = _localDataAccess.GetCommunicationParamDefinitions(protocolName);
                if (commParamDefs == null || !commParamDefs.Any())
                    return;
                foreach (var paramDefEntity in commParamDefs)
                {
                    //不重复添加用户已经添加的通信参数到下拉框
                    if (CommunicationParameterDefinitions.Any(paramDef => paramDef.ParameterName == paramDefEntity.ParameterName))
                    {
                        continue;
                    }
                    CommunicationParameterDefinitions.Add(new CommunicationParameterDefinition
                    {
                        ParameterName = paramDefEntity.ParameterName,
                        Label = paramDefEntity.Label,
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

                //从通信参数下拉框中找第一个未添加到设备的参数选项，并将其添加到设备的通信参数列表中
                var firstMatch = CommunicationParameterDefinitions.Where(paramDef => !CommunicationParameters.Any(para => string.Equals(para.PropName, paramDef.ParameterName))).FirstOrDefault();
                if (firstMatch != null)
                {
                    var commParam = new CommunicationParameter
                    {
                        PropName = firstMatch.ParameterName,
                        PropValue = firstMatch.DefaultValueOption,
                        PropValueType = firstMatch.ValueDataType
                    };
                    CommunicationParameters.Add(commParam);
                }
            }
        }

        private void doSelectCommunicationParameterValue(SelectCommunicationParameterValueCommandParameter cmdParam)
        {
            if (cmdParam == null || cmdParam.ParameterDef == null)
                return;
            if (CommunicationParameters == null || !CommunicationParameters.Any() || cmdParam.IndexOfCommunicationParameters < 0 || cmdParam.IndexOfCommunicationParameters >= CommunicationParameters.Count)
                return;
            //从CommunicationParameters中找到与通信参数定义下拉框对应的通信参数名
            if (cmdParam.ParameterDef.ParameterName != CommunicationParameters[cmdParam.IndexOfCommunicationParameters].PropName)
                return;
            CommunicationParameters[cmdParam.IndexOfCommunicationParameters].PropValue = cmdParam.ParameterValue.PropOptionValue;

            //如果切换成另一个通信协议，则需要清除当前设备已经添加的其他通信参数并重新推荐符合当前通信协议的通信参数给用户选择
            if (string.Equals(cmdParam.ParameterDef.ParameterName, "Protocol"))
            {
                var protocolParam = CommunicationParameters.FirstOrDefault(param => param.PropName == "Protocol");
                CommunicationParameters.Clear();
                if (protocolParam != null)
                {
                    CommunicationParameters.Add(protocolParam);
                }

                //切换通信协议后，重新设置通信参数下拉框中的选项，只保留通信协议参数
                var protocolParamDef = CommunicationParameterDefinitions.FirstOrDefault(paramDef => paramDef.ParameterName == "Protocol");
                CommunicationParameterDefinitions.Clear();
                if (protocolParamDef != null)
                {
                    CommunicationParameterDefinitions.Add(protocolParamDef);
                }
            }
        }
        #endregion

        #region 变量点位配置
        public ObservableCollection<Variable> Variables { get; private set; } = new ObservableCollection<Variable>();

        public RelayCommand<Variable> AddVariableCommand { get; set; }

        public RelayCommand<Variable> RemoveVariableCommand { get; set; }

        private void doAddVariable(Variable variable)
        {
            if (variable != null)
            {
                Variables.Add(variable);
            }
            else
            {
                Variables.Add(new Variable()
                {
                    VarNum = $"V{DateTime.Now.ToString("yyyyMMddHHmmssfff")}",
                    VarType = typeof(int), //默认为整型
                    ValidateDuplication = (variable, propName) =>
                    {
                        if (propName == nameof(Variable.VarName))
                        {
                            //验证点位名称在当前设备中不能重复
                            if (Variables.Any(v => v != variable && string.Equals(v.VarName, variable.VarName, StringComparison.OrdinalIgnoreCase)))
                            {
                                return false;
                            }
                        }
                        return true;
                    }
                });
            }
        }

        private bool canRemoveVariable(Variable variable)
        {
            if (variable == null)
                return false;
            if (!Variables.Contains(variable))
                return false;
            return true;
        }

        private void doRemoveVariable(Variable variable)
        {
            if (variable == null)
                throw new Exception("变量点位信息不能为空！");
            Variables.Remove(variable);
        }
        #endregion

        #region 预警信息
        public ObservableCollection<Alarm> Alarms { get; private set; } = new ObservableCollection<Alarm>();


        #endregion
    }

    public class SelectCommunicationParameterValueCommandParameter
    {
        public CommunicationParameterOption ParameterValue { get; set; }

        public CommunicationParameterDefinition ParameterDef { get; set; }

        //当前参数定义对象与设备通信参数列表CommunicationParameters中第几个参数关联
        public int IndexOfCommunicationParameters { get; set; }
    }

    public enum FlowDirections
    {
        Clockwise,
        Anticlockwise
    }
}
