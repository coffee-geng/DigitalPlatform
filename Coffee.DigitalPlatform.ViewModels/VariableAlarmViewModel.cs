using Coffee.DigitalPlatform.Common;
using Coffee.DigitalPlatform.Controls.FilterBuilder;
using Coffee.DigitalPlatform.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Coffee.DigitalPlatform.ViewModels
{
    public class VariableAlarmViewModel : ObservableObject
    {
        public VariableAlarmViewModel(Device device)
        {
            CurrentDevice = device;

            AddAlarmCommand = new RelayCommand(doAddAlarmCommand, canAddAlarmCommand);
            EditAlarmCommand = new RelayCommand<Alarm>(doEditAlarmCommand, canEditAlarmCommand);
            RemoveAlarmCommand = new RelayCommand<Alarm>(doRemoveAlarmCommand, canRemoveAlarmCommand);
            ConfirmAddAlarmCommand = new RelayCommand<Alarm>(doConfirmAddAlarmCommand);
            ConfirmEditAlarmCommand = new RelayCommand<Alarm> (doConfirmEditAlarmCommand);
            CancelAddAlarmCommand = new RelayCommand(doCancelAddAlarmCommand);
            CancelEditAlarmCommand = new RelayCommand<Alarm>(doCancelEditAlarmCommand);

            ReceiveFilterSchemeCommand = new RelayCommand<ReceiveFilterSchemeArgs>(doReceiveFilterSchemeCommand);
            ResetCommand = new RelayCommand(doResetCommand);

            if (CurrentDevice.Variables != null && CurrentDevice.Variables.Count > 0)
            {
                var instance = createAlarmConditionFilter();
                var dynamicType = instance.GetType();

                //根据设备点位信息，定义了当前设备预警条件的默认筛选器上下文
                var filterScheme = new FilterScheme(dynamicType)
                {
                    Title = Guid.NewGuid().ToString()
                };
                var schemeInfo = new FilterSchemeEditInfo(filterScheme, new List<dynamic>() { instance }, true, true);

                if (device.Alarms != null && device.Alarms.Count > 0)
                {
                    foreach (var alarm in device.Alarms)
                    {
                        ICondition topCondition = alarm.Condition;
                        if (topCondition != null)
                        {
                            topCondition.SyncDeviceNum(device.DeviceNum);
                        }
                        initVariableInCondition(topCondition, device, dynamicType);
                        if (topCondition != null)
                        {
                            //根据当前预警条件，生成对应的条件筛选器上下文
                            var filterSchemeByAlarm = new FilterScheme(dynamicType, Guid.NewGuid().ToString(), alarm.Condition.Raw);
                            var schemeInfoByAlarm = new FilterSchemeEditInfo(filterScheme, new List<dynamic>() { instance }, true, true);
                            alarm.ConditionTemplate = schemeInfoByAlarm;
                            alarm.FormattedCondition = topCondition.ToString();
                        }
                        else
                        {
                            alarm.ConditionTemplate = schemeInfo;
                        }
                        alarm.IsFirstEditing = false;
                        this.Alarms.Add(alarm);
                    }
                }
            }
        }

        public Device CurrentDevice { get; private set; }

        public ObservableCollection<Alarm> Alarms { get; set; } = new ObservableCollection<Alarm>();

        private bool _isEditingAlarm;
        public bool IsEditingAlarm
        {
            get { return _isEditingAlarm; }
            set
            {
                if (SetProperty(ref _isEditingAlarm, value))
                {
                    if (EditAlarmCommand != null)
                    {
                        EditAlarmCommand.NotifyCanExecuteChanged();
                    }
                    if (RemoveAlarmCommand != null)
                    {
                        RemoveAlarmCommand.NotifyCanExecuteChanged();
                    }
                    if (AddAlarmCommand != null)
                    {
                        AddAlarmCommand.NotifyCanExecuteChanged();
                    }
                }
            }
        }

        #region 新建预警信息

        public RelayCommand AddAlarmCommand { get; set; }

        public RelayCommand<Alarm> EditAlarmCommand { get; set; }

        public RelayCommand<Alarm> RemoveAlarmCommand { get; set; }

        public RelayCommand<Alarm> ConfirmAddAlarmCommand {  get; set; }

        public RelayCommand<Alarm> ConfirmEditAlarmCommand { get; set; }

        public RelayCommand CancelAddAlarmCommand { get; set; }

        public RelayCommand<Alarm> CancelEditAlarmCommand { get; set; }

        public RelayCommand<ReceiveFilterSchemeArgs> ReceiveFilterSchemeCommand { get; set; }

        public RelayCommand ResetCommand { get; set; }

        private void doAddAlarmCommand()
        {
            if (CurrentDevice.Variables != null && CurrentDevice.Variables.Count > 0)
            {
                var instance = createAlarmConditionFilter();
                var dynamicType = instance.GetType();

                var filterScheme = new FilterScheme(dynamicType)
                {
                    Title = Guid.NewGuid().ToString()
                };
                var schemeInfo = new FilterSchemeEditInfo(filterScheme, new List<dynamic>() { instance }, true, true);

                this.Alarms.Add(new Alarm()
                {
                    ConditionTemplate = schemeInfo,
                    IsFirstEditing = true,
                });

                this.IsEditingAlarm = true;
            }
        }

        private bool canAddAlarmCommand()
        {
            return !this.IsEditingAlarm;
        }

        private void doEditAlarmCommand(Alarm alarm)
        {
            if (alarm == null)
                return;
            alarm.NewAlarmMessage = alarm.AlarmMessage;
            alarm.NewAlarmTag = alarm.AlarmTag;
            alarm.IsFirstEditing = false;

            var instance = createAlarmConditionFilter();
            var dynamicType = instance.GetType();

            var filterScheme = new FilterScheme(dynamicType, Guid.NewGuid().ToString(), alarm.Condition.Raw);
            var schemeInfo = new FilterSchemeEditInfo(filterScheme, new List<dynamic>() { instance }, true, true);
            alarm.ConditionTemplate = schemeInfo;

            Task.Delay(TimeSpan.FromMilliseconds(500)).ContinueWith(t =>
            {
                alarm.IsEditing = true;
                if (this.Alarms != null && Alarms.Any())
                {
                    var list = Alarms.Where(c => c != alarm);
                    foreach (var item in list)
                    {
                        item.IsEditing = false;
                    }
                }
                this.IsEditingAlarm = true;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private bool canEditAlarmCommand(Alarm alarm)
        {
            return !this.IsEditingAlarm;
        }

        private void doRemoveAlarmCommand(Alarm alarm)
        {
            if (alarm == null || !Alarms.Any(a => a == alarm))
                return;
            Alarms.Remove(alarm);
            if (CurrentDevice != null)
            {
                CurrentDevice.Alarms.Remove(alarm);
            }
        }

        private bool canRemoveAlarmCommand(Alarm alarm)
        {
            return !this.IsEditingAlarm;
        }

        private void doConfirmAddAlarmCommand(Alarm alarm)
        {
            if (alarm == null || alarm.ConditionTemplate == null)
                return;
            var conditionChain = ConditionFactory.CreateCondition(alarm.ConditionTemplate.FilterScheme);
            
            alarm.AlarmNum = "a_" + DateTime.Now.ToString("yyyyMMddHHmmssFFF");
            alarm.AlarmMessage = alarm.NewAlarmMessage;
            alarm.AlarmTag = alarm.NewAlarmTag;
            alarm.Condition = conditionChain;
            alarm.FormattedCondition = conditionChain.ToString();
            alarm.IsFirstEditing = false;

            // 当开始新建预警时，会先将预警模版添加到列表末尾，以便用户进行编辑
            // 这一项是临时的，当确定编辑时，在添加编辑后的预警前，需将这临时项从列表中移除
            if (Alarms.Count > 0)
            {
                this.Alarms.RemoveAt(Alarms.Count - 1);
            }
            // 确定添加预警信息
            Alarms.Add(alarm);

            if (CurrentDevice != null)
            {
                CurrentDevice.Alarms.Add(alarm);
            }

            this.IsEditingAlarm = false;
        }

        private void doConfirmEditAlarmCommand(Alarm alarm)
        {
            if (alarm == null || alarm.ConditionTemplate == null)
                return;
            var conditionChain = ConditionFactory.CreateCondition(alarm.ConditionTemplate.FilterScheme);
            alarm.AlarmMessage = alarm.NewAlarmMessage;
            alarm.AlarmTag = alarm.NewAlarmTag;
            alarm.Condition = conditionChain;
            alarm.FormattedCondition = conditionChain.ToString();
            alarm.IsFirstEditing = false;
            alarm.IsEditing = false;

            this.IsEditingAlarm = false;
        }

        private void doCancelAddAlarmCommand()
        {
            // 当开始新建预警时，会先将预警模版添加到列表末尾，以便用户编辑
            // 这一项是临时的，当取消编辑时，需将这项从列表中移除
            if (Alarms.Count > 0)
            {
                this.Alarms.RemoveAt(Alarms.Count - 1);
            }

            this.IsEditingAlarm = false;
        }

        private void doCancelEditAlarmCommand(Alarm alarm)
        {
            if (alarm == null)
                return;
            alarm.NewAlarmMessage = null;
            alarm.NewAlarmTag = null;
            alarm.IsEditing = false;

            this.IsEditingAlarm= false;
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
            //当退出窗口时，需要重置所有预警条件的编辑状态，以便下次打开窗口时没有预警条件处于编辑状态
            if (this.Alarms != null)
            {
                foreach(var alarm in this.Alarms)
                {
                    alarm.IsEditing = false;
                }
            }
        }

        //根据当前设备的点位变量信息生成用于过滤预警条件的类
        private object createAlarmConditionFilter()
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
            catch(Exception ex)
            {
                throw new Exception($"不能根据当前设备的点位变量信息生成用于过滤预警条件的类！", ex);
            }
        }

        // 递归初始化预警条件中的点位变量信息
        private void initVariableInCondition(ICondition condition, Device device, Type dynamicType)
        {
            if (condition == null)
                return;
            if (condition is Coffee.DigitalPlatform.Models.Condition conditionExp)
            {
                if (!string.Equals(conditionExp.Source.DeviceNum, device.DeviceNum))
                {
                    throw new Exception($"预警条件中的点位变量所属设备编号{conditionExp.Source.DeviceNum}，与当前设备编号{device.DeviceNum}不匹配，无法加载预警条件！");
                }
                var variable = device.Variables.FirstOrDefault(v => v.VarNum == conditionExp.Source.VarNum);
                if (variable == null)
                {
                    throw new Exception($"预警条件中的点位变量编号{conditionExp.Source.VarNum}，在当前设备的点位变量列表中不存在，无法加载预警条件！");
                }
                PropertyInfo property = null;
                try
                {
                    property = dynamicType.GetProperty(variable.VarNum);
                }
                catch (Exception ex)
                {
                    throw new Exception($"预警条件中的点位变量编号{conditionExp.Source.VarNum}，在当前设备的点位变量列表中不存在，无法加载预警条件！");
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

    public class VariableByNameComparer : IEqualityComparer<Variable>
    {
        public bool Equals(Variable? x, Variable? y)
        {
            if (ReferenceEquals(x, y)) return true;
            if (x == null || y == null) return false;
            return x.VarName == y.VarNum;
        }

        public int GetHashCode([DisallowNull] Variable obj)
        {
            unchecked
            {
                var hashCode = obj.VarName.GetHashCode();
                return hashCode;
            }
        }
    }

    public class ReceiveFilterSchemeArgs
    {
        public FilterScheme FilterScheme { get; set; }

        //是哪一项接收FilterScheme的更改
        //即正在编辑的是哪一个预警信息或联控选项信息
        public IReceiveFilterScheme Receiver { get; set; }
    }
}
