using Coffee.DigitalPlatform.Entities;
using Coffee.DigitalPlatform.IDataAccess;
using Coffee.DigitalPlatform.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using Coffee.DigitalPlatform.Common;
using System.Windows.Data;
using Coffee.DigitalPlatform.CommWPF;

namespace Coffee.DigitalPlatform.ViewModels
{
    public class AlarmViewModel : AbstractComponentViewModel, IDataPager
    {
        private ILocalDataAccess _localDataAccess;

        public AlarmViewModel(ILocalDataAccess localDataAccess)
        {
            _localDataAccess = localDataAccess;

            loadAlarms();
        }

        private ObservableCollection<Alarm> _alarms;
        public ObservableCollection<Alarm> Alarms
        {
            get { return _alarms; }
            set { SetProperty(ref _alarms, value); }
        }

        private int _pageSize = 1;
        public int PageSize
        {
            get { return _pageSize; }
            set
            {
                if (SetProperty(ref _pageSize, value))
                {
                    if (AlarmCollectionView != null)
                    {
                        AlarmCollectionView.Refresh();
                    }
                }
            }
        }

        private int _currentPage = 1;
        public int CurrentPage
        {
            get { return _currentPage; }
            set
            {
                if (SetProperty(ref _currentPage, value))
                {
                    if (AlarmCollectionView != null)
                    {
                        AlarmCollectionView.Refresh();
                    }
                }
            }
        }

        private ListCollectionView _alarmCollectionView;
        public ListCollectionView AlarmCollectionView
        {
            get { return _alarmCollectionView; }
            set
            {
                if (SetProperty(ref _alarmCollectionView, value))
                {
                    if (value != null)
                    {
                        value.Filter = item =>
                        {
                            var index = _alarmCollectionView.SourceCollection.Cast<Alarm>().ToList().IndexOf(item as Alarm);
                            var page = index / PageSize + 1; //页码从1开始
                            return page == CurrentPage;
                        };
                    }
                }
            }
        }

        private void loadAlarms()
        {
            Dictionary<string, IList<AlarmEntity>> deviceAlarmDict = _localDataAccess.ReadAlarms();
            List<AlarmEntity> alarmList = new List<AlarmEntity>();
            foreach (var pair in deviceAlarmDict)
            {
                if (pair.Value == null || !pair.Value.Any())
                    continue;
                var list = pair.Value.OrderByDescending(p => p.AlarmLevel);
                alarmList.AddRange(list);
            }

            Dictionary<string, Device> deviceDict = new Dictionary<string, Device>();
            Dictionary<string, DeviceEntity> deviceEntityDict = _localDataAccess.ReadDevices().ToDictionary(d => d.DeviceNum, d => d);
            foreach (var pair in deviceEntityDict)
            {
                var deviceEntity = pair.Value;
                var device = new Device(_localDataAccess)
                {
                    DeviceNum = deviceEntity.DeviceNum,
                    Name = deviceEntity.Label
                };
                if (deviceEntity.Variables != null)
                {
                    foreach (var variableEntity in deviceEntity.Variables)
                    {
                        var variable = new Variable()
                        {
                            VarNum = variableEntity.VarNum,
                            VarName = variableEntity.Label,
                            VarType = TypeUtils.GetTypeFromAssemblyQualifiedName(variableEntity.VarType),
                            DeviceNum = deviceEntity.DeviceNum,
                        };
                        device.Variables.Add(variable);
                    }
                }
                deviceDict.Add(pair.Key, device);
            }

            IEnumerable<ConditionEntity> topConditionEntities = _localDataAccess.GetTopConditions(); //预加载所有顶级条件选项
            IEnumerable<ConditionEntity> conditionEntities = _localDataAccess.GetConditions(); //预加载所有顶级条件选项

            var alarms = new List<Alarm>();
            alarmList.ForEach(alarmEntity =>
            {
                var alarm = new Alarm
                {
                    AlarmNum = alarmEntity.AlarmNum,
                    AlarmLevel = alarmEntity.AlarmLevel,
                    AlarmMessage = alarmEntity.AlarmMessage,
                    AlarmTag = alarmEntity.AlarmTag,
                    AlarmDevice = deviceDict.ContainsKey(alarmEntity.DeviceNum) ? deviceDict[alarmEntity.DeviceNum] : null,
                    AlarmState = new AlarmState(Enum.TryParse(typeof(AlarmStatus), alarmEntity.State, out object? state) ? (AlarmStatus)state : AlarmStatus.Unknown),
                    AlarmTime = DateTime.TryParse(alarmEntity.AlarmTime, out DateTime alarmTime) ? alarmTime : (DateTime?)null,
                };
                if (alarmEntity.AlarmValues != null && alarmEntity.AlarmValues.Count > 0)
                {
                    var alarmValues = new List<Variable>();
                    foreach (var alarmValue in alarmEntity.AlarmValues)
                    {
                        if (deviceDict.TryGetValue(alarmEntity.DeviceNum, out Device device))
                        {
                            var variable = device.Variables.FirstOrDefault(v => string.Equals(v.VarNum, alarmValue.VarNum));
                            if (variable != null)
                            {
                                variable.FinalValue = alarmValue.Value; //数据表alarms保存的是经过系数和偏移量转换后的值
                                alarmValues.Add(variable);
                            }
                        }
                    }
                    alarm.AlarmValues = alarmValues;
                }

                var topConditionEntity = topConditionEntities?.FirstOrDefault(c => string.Equals(c.CNum, alarmEntity.ConditionNum));
                if (topConditionEntity != null)
                {
                    if (deviceDict.TryGetValue(alarmEntity.DeviceNum, out Device device))
                    {
                        //预警信息仅使用顶级条件项作为触发预警的条件
                        ICondition topCondition = createConditionByEntity(topConditionEntity, conditionEntities, device.Variables.ToDictionary(v => v.VarNum, v => v));
                        alarm.Condition = topCondition;
                        if (alarm.Condition != null && alarm.AlarmValues != null)
                        {
                            var formater = new ExpressionFormatSetting();
                            string key = Enum.GetName(typeof(ExpressionFormatBlocks), ExpressionFormatBlocks.Expression);
                            formater.AddFontForeground(key, Colors.Red);
                            //formater.AddFontWeight(key, FontWeights.Bold);
                            formater.AddFontSize(key, 10);
                            var htmlNode = alarm.Condition.GetExpressionResult(alarm.AlarmValues.ToDictionary(p => p.VarNum, q => q.FinalValue), formater);
                            alarm.FormattedCondition = htmlNode.OuterHtml;
                        }
                    }
                    alarms.Add(alarm);
                }
            });

            PageSize = 2;
            Alarms = new ObservableCollection<Alarm>(alarms);
            AlarmCollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(Alarms);
        }
    }
}
