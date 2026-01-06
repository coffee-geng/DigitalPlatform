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

            AddConditionCommand = new RelayCommand(doAddConditionCommand);
        }

        public Device CurrentDevice { get; set; }

        public ObservableCollection<Alarm?> AlarmConditions { get; set; } = new ObservableCollection<Alarm?>();

        public RelayCommand AddConditionCommand { get; set; }

        private void doAddConditionCommand()
        {
            if (!AlarmConditions.Any(c => c == null))
            {
                AlarmConditions.Add(null);

                if (CurrentDevice.Variables != null && CurrentDevice.Variables.Count > 0)
                {
                    //var variables = CurrentDevice.Variables.Distinct(new VariableByNameComparer());
                    //var properties = variables.ToDictionary(v1 => v1.VarName, v2 => v2.VarType);
                    //Type dynamicType = DynamicClassCreator.CreateDynamicType(CurrentDevice.DeviceNum, properties);
                    //object instance = Activator.CreateInstance(dynamicType);
                    //foreach (var variable in variables)
                    //{
                    //    dynamicType.GetProperty(variable.VarName).SetValue(instance, variable.Value);
                    //}
                    //var filterScheme = new FilterScheme(dynamicType);
                    //var schemeInfo = new FilterSchemeEditInfo(filterScheme, new List<dynamic>() { instance }, false, false);
                    //this.DefaultFilterScheme = schemeInfo;

                    var filterScheme = new FilterScheme(typeof(TestEntity));
                    var rawItems = new TestDataService().GetTestItems();
                    var filterSchemeEditInfo = new FilterSchemeEditInfo(filterScheme, rawItems, true, true);
                    this.DefaultFilterScheme = filterSchemeEditInfo;
                }
            }
        }

        public Dictionary<Alarm, FilterSchemeEditInfo> AlarmConditionFilterSchemeDict { get; set; }

        private FilterSchemeEditInfo _defaultFilterScheme = null;
        public FilterSchemeEditInfo DefaultFilterScheme
        {
            get { return _defaultFilterScheme; }
            set { SetProperty(ref _defaultFilterScheme, value); }
        }
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
