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

        //触发报警的条件链
        public ConditionChain Condition { get; set; }

        public AlarmState AlarmState { get; set; }
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
