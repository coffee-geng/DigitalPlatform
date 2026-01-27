using Coffee.DigitalPlatform.Common;
using Coffee.DigitalPlatform.Controls.FilterBuilder;
using Coffee.DigitalPlatform.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace Coffee.DigitalPlatform.ViewModels
{
    public class ControlInfoByTriggerViewModel : ObservableObject
    {
        public ControlInfoByTriggerViewModel(Device device, ObservableCollection<Device> deviceList)
        {
            CurrentDevice = device;

            AddControlInfoByTriggerCommand = new RelayCommand(doAddControlInfoByTriggerCommand, canAddControlInfoByTriggerCommand);
            EditControlInfoByTriggerCommand = new RelayCommand<ControlInfoByTrigger>(doEditControlInfoByTriggerCommand, canEditControlInfoByTriggerCommand);
            RemoveControlInfoByTriggerCommand = new RelayCommand<ControlInfoByTrigger>(doRemoveControlInfoByTriggerCommand, canRemoveControlInfoByTriggerCommand);
            ConfirmAddControlInfoByTriggerCommand = new RelayCommand<ControlInfoByTrigger>(doConfirmAddControlInfoByTriggerCommand);
            ConfirmEditControlInfoByTriggerCommand = new RelayCommand<ControlInfoByTrigger>(doConfirmEditControlInfoByTriggerCommand);
            CancelAddControlInfoByTriggerCommand = new RelayCommand(doCancelAddControlInfoByTriggerCommand);
            CancelEditControlInfoByTriggerCommand = new RelayCommand<ControlInfoByTrigger>(doCancelEditControlInfoByTriggerCommand);

            ReceiveFilterSchemeCommand = new RelayCommand<ReceiveFilterSchemeArgs>(doReceiveFilterSchemeCommand);
            ResetCommand = new RelayCommand(doResetCommand);

            if (CurrentDevice.Variables != null && CurrentDevice.Variables.Count > 0)
            {
                var instance = createControlInfoByTriggerConditionFilter();
                var dynamicType = instance.GetType();

                //根据设备点位信息，定义了当前设备联动控制条件的默认筛选器上下文
                var filterScheme = new FilterScheme(dynamicType)
                {
                    Title = Guid.NewGuid().ToString()
                };
                var schemeInfo = new FilterSchemeEditInfo(filterScheme, new List<dynamic>() { instance }, true, true);

                if (device.ControlInfosByTrigger != null && device.ControlInfosByTrigger.Count > 0)
                {
                    foreach (var controlInfo in device.ControlInfosByTrigger)
                    {
                        ICondition topCondition = controlInfo.Condition;
                        if (topCondition != null)
                        {
                            topCondition.SyncDeviceNum(device.DeviceNum);
                        }
                        initVariableInCondition(topCondition, device, dynamicType);
                        if (topCondition != null)
                        {
                            //根据当前联动控制条件，生成对应的条件筛选器上下文
                            var filterSchemeByControlInfo = new FilterScheme(dynamicType, Guid.NewGuid().ToString(), controlInfo.Condition.Raw);
                            var schemeInfoByControlInfo = new FilterSchemeEditInfo(filterScheme, new List<dynamic>() { instance }, true, true);
                            controlInfo.ConditionTemplate = schemeInfoByControlInfo;
                            controlInfo.FormattedCondition = topCondition.ToString();
                        }
                        else
                        {
                            controlInfo.ConditionTemplate = schemeInfo;
                        }
                        controlInfo.IsFirstEditing = false;
                        this.ControlInfosByTrigger.Add(controlInfo);
                    }
                }
            }

            DeviceCollection = deviceList;
        }

        public Device CurrentDevice { get; private set; }

        public ObservableCollection<ControlInfoByTrigger> ControlInfosByTrigger { get; set; } = new ObservableCollection<ControlInfoByTrigger>();

        private bool _isEditingControlInfo;
        public bool IsEditingControlInfo
        {
            get { return _isEditingControlInfo; }
            set
            {
                if (SetProperty(ref _isEditingControlInfo, value))
                {
                    if (EditControlInfoByTriggerCommand != null)
                    {
                        EditControlInfoByTriggerCommand.NotifyCanExecuteChanged();
                    }
                    if (RemoveControlInfoByTriggerCommand != null)
                    {
                        RemoveControlInfoByTriggerCommand.NotifyCanExecuteChanged();
                    }
                    if (AddControlInfoByTriggerCommand != null)
                    {
                        AddControlInfoByTriggerCommand.NotifyCanExecuteChanged();
                    }
                }
            }
        }

        public ObservableCollection<Device> DeviceCollection { get; private set; } = new ObservableCollection<Device>();

        #region 新建联动控制信息

        public RelayCommand AddControlInfoByTriggerCommand { get; set; }

        public RelayCommand<ControlInfoByTrigger> EditControlInfoByTriggerCommand { get; set; }

        public RelayCommand<ControlInfoByTrigger> RemoveControlInfoByTriggerCommand { get; set; }

        public RelayCommand<ControlInfoByTrigger> ConfirmAddControlInfoByTriggerCommand { get; set; }

        public RelayCommand<ControlInfoByTrigger> ConfirmEditControlInfoByTriggerCommand { get; set; }

        public RelayCommand CancelAddControlInfoByTriggerCommand { get; set; }

        public RelayCommand<ControlInfoByTrigger> CancelEditControlInfoByTriggerCommand { get; set; }

        public RelayCommand<ReceiveFilterSchemeArgs> ReceiveFilterSchemeCommand { get; set; }

        public RelayCommand ResetCommand { get; set; }

        private void doAddControlInfoByTriggerCommand()
        {
            if (CurrentDevice.Variables != null && CurrentDevice.Variables.Count > 0)
            {
                var instance = createControlInfoByTriggerConditionFilter();
                var dynamicType = instance.GetType();

                var filterScheme = new FilterScheme(dynamicType)
                {
                    Title = Guid.NewGuid().ToString()
                };
                var schemeInfo = new FilterSchemeEditInfo(filterScheme, new List<dynamic>() { instance }, true, true);

                this.ControlInfosByTrigger.Add(new ControlInfoByTrigger()
                {
                    ConditionTemplate = schemeInfo,
                    IsFirstEditing = true,
                });

                this.IsEditingControlInfo = true;
            }
        }

        private bool canAddControlInfoByTriggerCommand()
        {
            return !this.IsEditingControlInfo;
        }

        private void doEditControlInfoByTriggerCommand(ControlInfoByTrigger controlInfo)
        {
            if (controlInfo == null)
                return;
            controlInfo.NewHeader = controlInfo.Header;
            controlInfo.NewDevice = controlInfo.Device;
            controlInfo.NewVariable = controlInfo.Variable;
            controlInfo.NewValue = controlInfo.Value;
            controlInfo.IsFirstEditing = false;

            var instance = createControlInfoByTriggerConditionFilter();
            var dynamicType = instance.GetType();

            var filterScheme = new FilterScheme(dynamicType, Guid.NewGuid().ToString(), controlInfo.Condition.Raw);
            var schemeInfo = new FilterSchemeEditInfo(filterScheme, new List<dynamic>() { instance }, true, true);
            controlInfo.ConditionTemplate = schemeInfo;

            Task.Delay(TimeSpan.FromMilliseconds(500)).ContinueWith(t =>
            {
                controlInfo.IsEditing = true;
                if (this.ControlInfosByTrigger != null && ControlInfosByTrigger.Any())
                {
                    var list = ControlInfosByTrigger.Where(c => c != controlInfo);
                    foreach (var item in list)
                    {
                        item.IsEditing = false;
                    }
                }
                this.IsEditingControlInfo = true;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private bool canEditControlInfoByTriggerCommand(ControlInfoByTrigger controlInfo)
        {
            return !this.IsEditingControlInfo;
        }

        private void doRemoveControlInfoByTriggerCommand(ControlInfoByTrigger controlInfo)
        {
            if (controlInfo == null || !ControlInfosByTrigger.Any(a => a == controlInfo))
                return;
            ControlInfosByTrigger.Remove(controlInfo);
            if (CurrentDevice != null)
            {
                CurrentDevice.ControlInfosByTrigger.Remove(controlInfo);
            }
        }

        private bool canRemoveControlInfoByTriggerCommand(ControlInfoByTrigger controlInfo)
        {
            return !this.IsEditingControlInfo;
        }

        private void doConfirmAddControlInfoByTriggerCommand(ControlInfoByTrigger controlInfo)
        {
            if (controlInfo == null || controlInfo.ConditionTemplate == null)
                return;
            var conditionChain = ConditionFactory.CreateCondition(controlInfo.ConditionTemplate.FilterScheme);
            var device = DeviceCollection?.FirstOrDefault(d => controlInfo.NewDevice != null && !string.IsNullOrWhiteSpace(controlInfo.NewDevice.DeviceNum) && d.DeviceNum == controlInfo.NewDevice.DeviceNum);

            controlInfo.LinkageNum = "lk_" + DateTime.Now.ToString("yyyyMMddHHmmssFFF");
            controlInfo.Header = controlInfo.NewHeader;
            controlInfo.Variable = controlInfo.NewVariable;
            controlInfo.Value = controlInfo.NewValue;
            controlInfo.Device = device;
            controlInfo.Condition = conditionChain;
            controlInfo.FormattedCondition = conditionChain.ToString();
            controlInfo.IsFirstEditing = false;

            // 当开始新建联控选项时，会先将联控选项模版添加到列表末尾，以便用户进行编辑
            // 这一项是临时的，当确定编辑时，在添加编辑后的联控选项前，需将这临时项从列表中移除
            if (ControlInfosByTrigger.Count > 0)
            {
                this.ControlInfosByTrigger.RemoveAt(ControlInfosByTrigger.Count - 1);
            }
            // 确定添加联控选项信息
            ControlInfosByTrigger.Add(controlInfo);

            if (CurrentDevice != null)
            {
                CurrentDevice.ControlInfosByTrigger.Add(controlInfo);
            }

            this.IsEditingControlInfo = false;
        }

        private void doConfirmEditControlInfoByTriggerCommand(ControlInfoByTrigger controlInfo)
        {
            if (controlInfo == null || controlInfo.ConditionTemplate == null)
                return;
            var conditionChain = ConditionFactory.CreateCondition(controlInfo.ConditionTemplate.FilterScheme);
            var newDevice = DeviceCollection?.FirstOrDefault(d => controlInfo.NewDevice != null && !string.IsNullOrWhiteSpace(controlInfo.NewDevice.DeviceNum) && d.DeviceNum == controlInfo.NewDevice.DeviceNum);
            controlInfo.Header = controlInfo.NewHeader;
            controlInfo.Device = newDevice;
            controlInfo.Variable = controlInfo.NewVariable;
            controlInfo.Value = controlInfo.NewValue;
            controlInfo.Condition = conditionChain;
            controlInfo.FormattedCondition = conditionChain.ToString();
            controlInfo.IsFirstEditing = false;
            controlInfo.IsEditing = false;

            this.IsEditingControlInfo = false;
        }

        private void doCancelAddControlInfoByTriggerCommand()
        {
            // 当开始新建联控选项时，会先将联控选项模版添加到列表末尾，以便用户编辑
            // 这一项是临时的，当取消编辑时，需将这项从列表中移除
            if (ControlInfosByTrigger.Count > 0)
            {
                this.ControlInfosByTrigger.RemoveAt(ControlInfosByTrigger.Count - 1);
            }

            this.IsEditingControlInfo = false;
        }

        private void doCancelEditControlInfoByTriggerCommand(ControlInfoByTrigger controlInfo)
        {
            if (controlInfo == null)
                return;
            controlInfo.NewHeader = null;
            controlInfo.NewVariable = null;
            controlInfo.NewValue = null;
            controlInfo.NewDevice = null;
            controlInfo.IsEditing = false;

            this.IsEditingControlInfo = false;
        }

        private void doReceiveFilterSchemeCommand(ReceiveFilterSchemeArgs args)
        {
            if (args == null) return;
            if (args.Receiver == null) return;
            object instance = Activator.CreateInstance(args.FilterScheme.TargetType);
            args.Receiver.ConditionTemplate = new FilterSchemeEditInfo(args.FilterScheme, new List<dynamic>() { instance }, true, true);
        }

        private void doResetCommand()
        {
            //当退出窗口时，需要重置所有联控选项条件的编辑状态，以便下次打开窗口时没有联控条件处于编辑状态
            if (this.ControlInfosByTrigger != null)
            {
                foreach (var controlInfo in this.ControlInfosByTrigger)
                {
                    controlInfo.IsEditing = false;
                }
            }
        }

        //根据当前设备的点位变量信息生成用于过滤联动控制条件的类
        private object createControlInfoByTriggerConditionFilter()
        {
            try
            {
                var variables = CurrentDevice.Variables.Distinct(new VariableByNameComparer());
                var varGenerator = VariableNameGenerator.GetInstance(CurrentDevice.DeviceNum);
                Dictionary<string, Type> properties = new Dictionary<string, Type>();
                Dictionary<Variable, string> variableNameDict = new Dictionary<Variable, string>(); //保存的是点位信息对象和其传入FilterBuilder中的变量名
                foreach (var @var in variables)
                {
                    string varNameInFilterScheme = @var.VarNum;
                    properties.Add(varNameInFilterScheme, @var.VarType);
                    variableNameDict.Add(@var, varNameInFilterScheme);
                }
                Type dynamicType = DynamicClassCreator.CreateDynamicType(CurrentDevice.DeviceNum, properties, (TypeBuilder typeBuilder, Dictionary<string, PropertyBuilder> propertyBuildDict) =>
                {
                    foreach (var variable in variables)
                    {
                        // 获取自定义属性的构造函数
                        Type attributeType = typeof(DisplayNameAttribute);
                        ConstructorInfo constructor = attributeType.GetConstructor(new Type[] { typeof(string) });
                        // 准备构造函数的参数
                        object[] constructorArgs = new object[] { variable.VarName };
                        CustomAttributeBuilder attributeBuilder = new CustomAttributeBuilder(constructor, constructorArgs, new PropertyInfo[] { }, new object[] { });
                        var varNameInFilterScheme = variableNameDict[variable];
                        if (propertyBuildDict.TryGetValue(varNameInFilterScheme, out PropertyBuilder propertyBuild))
                        {
                            propertyBuild.SetCustomAttribute(attributeBuilder);
                        }
                    }
                });
                object instance = Activator.CreateInstance(dynamicType);
                foreach (var variable in variables)
                {
                    var propInfo = dynamicType.GetProperty(variable.VarName);
                    if (propInfo != null)
                    {
                        propInfo.SetValue(instance, variable.Value);
                    }
                }
                return instance;
            }
            catch (Exception ex)
            {
                throw new Exception($"不能根据当前设备的点位变量信息生成用于过滤联动控制条件的类！", ex);
            }
        }

        // 递归初始化联动控制选项条件中的点位变量信息
        private void initVariableInCondition(ICondition condition, Device device, Type dynamicType)
        {
            if (condition == null)
                return;
            if (condition is Coffee.DigitalPlatform.Models.Condition conditionExp)
            {
                if (!string.Equals(conditionExp.Source.DeviceNum, device.DeviceNum))
                {
                    throw new Exception($"联动控制选项条件中的点位变量所属设备编号{conditionExp.Source.DeviceNum}，与当前设备编号{device.DeviceNum}不匹配，无法加载联控条件！");
                }
                var variable = device.Variables.FirstOrDefault(v => v.VarNum == conditionExp.Source.VarNum);
                if (variable == null)
                {
                    throw new Exception($"联动控制选项条件中的点位变量编号{conditionExp.Source.VarNum}，在当前设备的点位变量列表中不存在，无法加载联控条件！");
                }
                PropertyInfo property = null;
                try
                {
                    property = dynamicType.GetProperty(variable.VarNum);
                }
                catch (Exception ex)
                {
                    throw new Exception($"联动控制选项条件中的点位变量编号{conditionExp.Source.VarNum}，在当前设备的点位变量列表中不存在，无法加载联控条件！");
                }
                conditionExp.Source.OwnerTypeInFilterScheme = dynamicType;
                conditionExp.Source.PropertyInFilterScheme = property;
            }
            else if (condition is Coffee.DigitalPlatform.Models.ConditionChain conditionGroup)
            {
                if (conditionGroup.ConditionItems.Any())
                {
                    foreach (var conditionItem in conditionGroup.ConditionItems)
                    {
                        initVariableInCondition(conditionItem, device, dynamicType);
                    }
                }
            }
        }
        #endregion
    }
}
