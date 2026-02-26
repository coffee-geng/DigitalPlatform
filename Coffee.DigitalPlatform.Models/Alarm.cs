using Coffee.DigitalPlatform.CommWPF;
using Coffee.DigitalPlatform.Controls.FilterBuilder;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Models
{
    public class Alarm : ObservableObject, IReceiveFilterScheme, ISaveState
    {
        // 预警编号
        public string AlarmNum { get; set; }

        //预警信息
        private string _alarmMessage;
        public string AlarmMessage
        {
            get { return _alarmMessage; }
            set
            {
                if (SetProperty(ref _alarmMessage, value))
                {
                    _isDirty = true;
                }
            }
        }

        //预警等级
        private int _alarmLevel;
        public int AlarmLevel
        {
            get { return _alarmLevel; }
            set
            {
                if (SetProperty(ref _alarmLevel, value))
                {
                    _isDirty = true;
                }
            }
        }

        private string _alarmTag;
        public string AlarmTag
        {
            get { return _alarmTag; }
            set
            {
                if (SetProperty(ref _alarmTag, value))
                {
                    _isDirty = true;
                }
            }
        }

        #region 报警来源
        private Device _alarmDevice;
        public Device AlarmDevice
        {
            get { return _alarmDevice; }
            set
            {
                if (SetProperty(ref _alarmDevice, value))
                {
                    _isDirty = true;
                }
            }
        }

        //预警触发时的数据值，可能是一个变量的值，也可能是多个变量的值，取决于报警条件中使用了多少个变量
        private IList<Variable> _alarmValues;
        public IList<Variable> AlarmValues
        {
            get { return _alarmValues; }
            set { SetProperty(ref _alarmValues, value); }
        }

        //预警触发时间
        private DateTime? _alarmTime;
        public DateTime? AlarmTime
        {
            get { return _alarmTime; }
            set { SetProperty(ref _alarmTime, value); }
        }
        #endregion

        #region 报警条件
        //触发报警的条件，可以是单个表达式条件，也可以是多个表达式组合成的条件链
        private ICondition _condition;
        public ICondition Condition
        {
            get { return _condition; }
            set
            {
                if (SetProperty(ref _condition, value))
                {
                    _isDirty = true;
                }
            }
        }

        private string _formattedCondition;
        public string FormattedCondition
        {
            get { return _formattedCondition; }
            set
            {
                if (SetProperty(ref _formattedCondition, value))
                {
                    _isDirty = true; //不需要跟踪Condition对象的变化，只要FormattedCondition变化，就认为是脏数据
                }
            }
        }
        #endregion

        #region 报警处理
        //是否处理过这个预警信息
        private AlarmState _alarmState;
        public AlarmState AlarmState
        {
            get { return _alarmState; }
            set { SetProperty(ref _alarmState, value); }
        }

        //预警处理完成的时间，如果未处理完成，则为null
        private DateTime? _solvedTime;
        public DateTime? SolvedTime
        {
            get { return _solvedTime; }
            set { SetProperty(ref _solvedTime, value); }
        }

        private string _userId;
        public string UserId
        {
            get { return _userId; }
            set { SetProperty(ref _userId, value); }
        }
        #endregion

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

        #region ISaveState 接口实现
        private bool _isDirty = false;
        public bool IsDirty 
        {
            get { return _isDirty; }
        }

        public void Save()
        {
            _isDirty = false;
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

        //当预警处理完成，则记录处理时间
        //否则，处理时间为null
        public DateTime? SolvedTime { get; set; }
    }

    public enum AlarmStatus
    {
        [Display(Name = "未知")]
        Unknown,
        [Display(Name = "未处理")]
        Unsolved,
        [Display(Name = "系统自动排错")]
        SolvedBySystem,
        [Display(Name = "已人工处理")]
        SolvedByManual
    }

    public interface IReceiveFilterScheme
    {
        FilterSchemeEditInfo ConditionTemplate { get; set; }
    }
}
