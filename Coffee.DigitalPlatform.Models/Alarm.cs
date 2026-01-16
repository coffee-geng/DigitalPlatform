using Coffee.DigitalPlatform.Controls.FilterBuilder;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Models
{
    public class Alarm : ObservableObject
    {
        public int Index { get; set; }
        public string id { get; set; }

        //触发报警的条件，可以是单个表达式条件，也可以是多个表达式组合成的条件链
        private ICondition _condition;
        public ICondition Condition 
        {
            get {  return _condition; }
            set { SetProperty(ref _condition, value); }
        }

        //是否处理过这个预警信息
        private AlarmState _alarmState;
        public AlarmState AlarmState 
        {
            get { return _alarmState; }
            set { SetProperty(ref _alarmState, value); }
        }

        //预警信息
        private string _alarmMessage;
        public string AlarmMessage
        {
            get { return _alarmMessage; }
            set { SetProperty(ref _alarmMessage, value); }
        }

        private string _alarmTag;
        public string AlarmTag
        {
            get { return _alarmTag; }
            set { SetProperty(ref _alarmTag, value); }
        }

        private string _formattedCondition;
        public string FormattedCondition
        {
            get { return _formattedCondition; }
            set { SetProperty(ref _formattedCondition, value); }
        }

        #region Alarm 编辑状态的属性
        
        //是否是新建预警信息
        public bool IsFirstEditing { get; set; } = true;

        //如果正在编辑，则展开Expander以便显示编辑区域
        //否则，收缩Expander以便隐藏编辑区域
        private bool _isEditing = false;
        public bool IsEditing
        {
            get { return _isEditing; }
            set { SetProperty(ref _isEditing, value); }
        }

        private FilterSchemeEditInfo _conditionTemplate;
        public FilterSchemeEditInfo ConditionTemplate
        {
            get { return _conditionTemplate; }
            set { SetProperty(ref _conditionTemplate, value); }
        }

        private string _newAlarmMessage;
        public string NewAlarmMessage
        {
            get { return _newAlarmMessage; }
            set { SetProperty(ref _newAlarmMessage, value); }
        }

        private string _newAlarmTag;
        public string NewAlarmTag
        {
            get { return _newAlarmTag; }
            set { SetProperty(ref _newAlarmTag, value); }
        }
        #endregion
    }

    public class AlarmState
    {
        public AlarmState(AlarmStatus status)
        {
            Status = status;
        }

        public AlarmStatus Status { get; private set; }

        public DateTime? SolvedTime {  get; set; }
    }

    public enum AlarmStatus
    {
        Unsolved,
        Solved
    }
}
