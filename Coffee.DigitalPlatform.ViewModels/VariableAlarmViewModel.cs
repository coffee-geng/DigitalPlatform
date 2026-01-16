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

            AddAlarmCommand = new RelayCommand(doAddAlarmCommand);
            EditAlarmCommand = new RelayCommand<Alarm>(doEditAlarmCommand, canDoEditAlarmCommand);
            RemoveAlarmCommand = new RelayCommand<Alarm>(doRemoveAlarmCommand, canDoRemoveAlarmCommand);
            ConfirmAddAlarmCommand = new RelayCommand<Alarm>(doConfirmAddAlarmCommand);
            ConfirmEditAlarmCommand = new RelayCommand<Alarm> (doConfirmEditAlarmCommand);
            CancelAddAlarmCommand = new RelayCommand(doCancelAddAlarmCommand);
            CancelEditAlarmCommand = new RelayCommand<Alarm>(doCancelEditAlarmCommand);

            ReceiveFilterSchemeCommand = new RelayCommand<ReceiveFilterSchemeArgs>(doReceiveFilterSchemeCommand);
        }

        public Device CurrentDevice { get; set; }

        public ObservableCollection<Alarm> AlarmConditions { get; set; } = new ObservableCollection<Alarm>();

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

        private void doAddAlarmCommand()
        {
            if (CurrentDevice.Variables != null && CurrentDevice.Variables.Count > 0)
            {
                var variables = CurrentDevice.Variables.Distinct(new VariableByNameComparer());
                var varGenerator = VariableNameGenerator.GetInstance(CurrentDevice.DeviceNum);
                Dictionary<string, Type> properties = new Dictionary<string, Type>();
                Dictionary<Variable, string> variableNameDict = new Dictionary<Variable, string>(); //保存的是点位信息对象和其传入FilterBuilder中的变量名
                foreach(var @var in variables)
                {
                    string varNameInFilterScheme = varGenerator.GenerateValidVariableName(@var.VarName);
                    properties.Add(varNameInFilterScheme, var.VarType);
                    variableNameDict.Add(var, varNameInFilterScheme);
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
                var filterScheme = new FilterScheme(dynamicType)
                {
                    Title = Guid.NewGuid().ToString()
                };
                var schemeInfo = new FilterSchemeEditInfo(filterScheme, new List<dynamic>() { instance }, true, true);

                this.AlarmConditions.Add(new Alarm()
                {
                    ConditionTemplate = schemeInfo,
                    IsFirstEditing = true,
                });

                this.IsEditingAlarm = true;
            }
        }

        private void doEditAlarmCommand(Alarm alarm)
        {
            if (alarm == null)
                return;
            alarm.NewAlarmMessage = alarm.AlarmMessage;
            alarm.NewAlarmTag = alarm.AlarmTag;
            alarm.IsFirstEditing = false;
            
            var variables = CurrentDevice.Variables.Distinct(new VariableByNameComparer());
            var varGenerator = VariableNameGenerator.GetInstance(CurrentDevice.DeviceNum);
            Dictionary<string, Type> properties = new Dictionary<string, Type>();
            Dictionary<Variable, string> variableNameDict = new Dictionary<Variable, string>(); //保存的是点位信息对象和其传入FilterBuilder中的变量名
            foreach (var @var in variables)
            {
                string varNameInFilterScheme = varGenerator.GenerateValidVariableName(@var.VarName);
                properties.Add(varNameInFilterScheme, var.VarType);
                variableNameDict.Add(var, varNameInFilterScheme);
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

            var filterScheme = new FilterScheme(dynamicType, Guid.NewGuid().ToString(), alarm.Condition.Raw);
            var schemeInfo = new FilterSchemeEditInfo(filterScheme, new List<dynamic>() { instance }, true, true);
            alarm.ConditionTemplate = schemeInfo;

            Task.Delay(TimeSpan.FromMilliseconds(500)).ContinueWith(t =>
            {
                alarm.IsEditing = true;
                if (this.AlarmConditions != null && AlarmConditions.Any())
                {
                    var list = AlarmConditions.Where(c => c != alarm);
                    foreach (var item in list)
                    {
                        item.IsEditing = false;
                    }
                }
                this.IsEditingAlarm = true;
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private bool canDoEditAlarmCommand(Alarm alarm)
        {
            return !this.IsEditingAlarm;
        }

        private void doRemoveAlarmCommand(Alarm alarm)
        {
            if (alarm == null || !AlarmConditions.Any(a => a == alarm))
                return;
            AlarmConditions.Remove(alarm);
            if (CurrentDevice != null)
            {
                CurrentDevice.Alarms.Remove(alarm);
            }
        }

        private bool canDoRemoveAlarmCommand(Alarm alarm)
        {
            return !this.IsEditingAlarm;
        }

        private void doConfirmAddAlarmCommand(Alarm alarm)
        {
            if (alarm == null || alarm.ConditionTemplate == null)
                return;
            var conditionChain = ConditionFactory.CreateCondition(alarm.ConditionTemplate.FilterScheme);
            alarm.AlarmMessage = alarm.NewAlarmMessage;
            alarm.AlarmTag = alarm.NewAlarmTag;
            alarm.Condition = conditionChain;
            alarm.FormattedCondition = conditionChain.ToString();
            alarm.IsFirstEditing = false;

            // 当开始新建预警时，会先将预警模版添加到列表末尾，以便用户进行编辑
            // 这一项是临时的，当确定编辑时，在添加编辑后的预警前，需将这临时项从列表中移除
            if (AlarmConditions.Count > 0)
            {
                this.AlarmConditions.RemoveAt(AlarmConditions.Count - 1);
            }
            // 确定添加预警信息
            AlarmConditions.Add(alarm);

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
            if (AlarmConditions.Count > 0)
            {
                this.AlarmConditions.RemoveAt(AlarmConditions.Count - 1);
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
            if (args.Alarm == null) return;
            object instance = Activator.CreateInstance(args.FilterScheme.TargetType);
            args.Alarm.ConditionTemplate = new FilterSchemeEditInfo(args.FilterScheme, new List<dynamic>() { instance }, true, true);
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

        //是哪一个预警列表中哪一项接收FilterScheme的更改
        //即正在编辑的是哪一个预警信息
        public Alarm Alarm { get; set; }
    }
}
