using Coffee.DigitalPlatform.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.IDataAccess
{
    public class AlarmHistoryRecord
    {
        [Column(name: "a_num")]
        public string AlarmNum { get; set; }

        [Column(name: "d_num")]
        public string DeviceNum { get; set; }

        [Column(name: "state")]
        public string AlarmState { get; set; }

        [Column(name: "alarm_values")]
        public IList<AlarmVariable> AlarmVariables { get; set; }

        [Column("alarm_time")]
        public DateTime? AlarmTime { get; set; }

        [Column("solve_time")]
        public DateTime? SolvedTime { get; set; }

        [Column(name: "user_id")]
        public string UserId { get; set; }

        [Column(name: "statechange_history")]
        public int StateChangedHistory { get; set; }
    }
}
