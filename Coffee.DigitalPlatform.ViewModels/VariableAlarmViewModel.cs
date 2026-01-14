using Coffee.DigitalPlatform.Common;
using Coffee.DigitalPlatform.Controls.FilterBuilder;
using Coffee.DigitalPlatform.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Linq;
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

            NewAlarmCommand = new RelayCommand(doNewAlarmCommand);
            AddNewAlarmCommand = new RelayCommand<Alarm>(doAddNewAlarmCommand);
            CancelNewAlarmCommand = new RelayCommand(doCancelNewAlarmCommand);

            ReceiveFilterSchemeCommand = new RelayCommand<ReceiveFilterSchemeArgs>(doReceiveFilterSchemeCommand);
        }

        public Device CurrentDevice { get; set; }

        public ObservableCollection<Alarm> AlarmConditions { get; set; } = new ObservableCollection<Alarm>();

        #region 新建预警信息
        
        public RelayCommand NewAlarmCommand { get; set; }

        public RelayCommand<Alarm> AddNewAlarmCommand {  get; set; }

        public RelayCommand CancelNewAlarmCommand { get; set; }

        public RelayCommand<ReceiveFilterSchemeArgs> ReceiveFilterSchemeCommand { get; set; }

        private void doNewAlarmCommand()
        {
            if (CurrentDevice.Variables != null && CurrentDevice.Variables.Count > 0)
            {
                var variables = CurrentDevice.Variables.Distinct(new VariableByNameComparer());
                var properties = variables.ToDictionary(v1 => v1.VarName, v2 => v2.VarType);
                Type dynamicType = DynamicClassCreator.CreateDynamicType(CurrentDevice.DeviceNum, properties);
                object instance = Activator.CreateInstance(dynamicType);
                foreach (var variable in variables)
                {
                    dynamicType.GetProperty(variable.VarName).SetValue(instance, variable.Value);
                }
                var filterScheme = new FilterScheme(dynamicType)
                {
                    Title = Guid.NewGuid().ToString()
                };
                var schemeInfo = new FilterSchemeEditInfo(filterScheme, new List<dynamic>() { instance }, true, true);

                this.AlarmConditions.Add(new Alarm()
                {
                    ConditionTemplate = schemeInfo,
                    IsFirstEditing = true
                });
            }
        }

        private void doAddNewAlarmCommand(Alarm alarm)
        {
            if (alarm == null || alarm.ConditionTemplate == null)
                return;
            var conditionChain = ConditionFactory.CreateCondition(alarm.ConditionTemplate.FilterScheme);
            alarm.AlarmMessage = alarm.NewAlarmMessage;
            alarm.AlarmTag = alarm.NewAlarmTag;
            alarm.Condition = conditionChain;
            alarm.FormattedCondition = conditionChain.ToString();
            alarm.IsFirstEditing = false;

            // 当开始新建预警时，会先将预警模版添加到列表末尾，以便用户编辑
            // 所以确定添加预警信息前，需将此预警模模版从列表中移除
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
        }

        private void doCancelNewAlarmCommand()
        {

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

    public class TestEntity
    {
        public string FirstName { get; set; }
        public int Age { get; set; }
        public int? Id { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime? DateOfDeath { get; set; }
        public bool IsActive { get; set; }
        public TimeSpan Duration { get; set; }
        public decimal Price { get; set; }
        public decimal? NullablePrice { get; set; }
        public MyEnum EnumValue { get; set; }
        public MyEnum? NullableEnumValue { get; set; }
        public Description Description { get; set; }
    }

    public enum MyEnum
    {
        EnumValue1,

        EnumValue2,

        SpecialValue
    }

    public class Description
    {
        public Description(string value)
        {
            Value = value;
        }

        public string Value { get; }
    }

    public class TestDataService
    {
        private readonly Random _random = new(DateTime.Now.Millisecond);
        private ObservableCollection<TestEntity>? _testItems;

        public ObservableCollection<TestEntity> GetTestItems()
        {
            return _testItems ??= GenerateTestItems();
        }

        public ObservableCollection<TestEntity> GenerateTestItems()
        {
            var items = new ObservableCollection<TestEntity>();
            for (var i = 0; i < 10000; i++)
            {
                items.Add(GenerateRandomEntity());
            }

            return items;
        }

        public TestEntity GenerateRandomEntity()
        {
            var testEntity = new TestEntity
            {
                FirstName = GetRandomString(),
                Age = _random.Next(1, 100),
                Id = _random.Next(10) < 1 ? null : _random.Next(10000),
                DateOfBirth = GetRandomDateTime(),
                DateOfDeath = _random.Next(10) >= 2 ? null : GetRandomDateTime(),
                IsActive = _random.Next(2) == 0,
                Duration = new TimeSpan(_random.Next(3), _random.Next(24), _random.Next(60), _random.Next(60)),
                Price = (decimal)(_random.NextDouble() * 1000 - 500),
                NullablePrice = (_random.Next(10) >= 5 ? (decimal?)(_random.NextDouble() * 1000 - 500) : null),
                EnumValue = (MyEnum)_random.Next(0, 3)
            };

            var next = _random.Next(0, 4);
            testEntity.NullableEnumValue = next == 3 ? null : (MyEnum?)next;

            testEntity.Description = new Description(GetRandomString());
            return testEntity;
        }

        public DateTime GetRandomDateTime()
        {
            return DateTime.Now.AddMinutes((int)(-1 * _random.NextDouble() * 30 * 365 * 24 * 60)); //years*days*hours*minuted
        }

        public string GetRandomString()
        {
            return _random.Next(10) < 1
                ? null
                : System.IO.Path.GetRandomFileName().Replace(".", string.Empty);
        }
    }
}
