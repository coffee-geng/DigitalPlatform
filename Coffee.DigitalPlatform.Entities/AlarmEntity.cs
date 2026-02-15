using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Entities
{
    public class AlarmEntity
    {
        public string id { get; set; }

        #region 报警触发条件
        [Column(name: "c_num")]
        public string ConditionNum { get; set; }// 报警条件编号

        [Column(name: "d_num")]
        public string DeviceNum { get; set; }
        #endregion

        #region 报警状态及数据
        /// <summary>
        /// 记录类型：正常0、报警10、报警已处理1、联控2
        /// </summary>
        [Column(name: "state")]
        public string? State { get; set; }

        [Column(name: "a_num")]
        public string AlarmNum { get; set; }

        [Column(name: "content")]
        public string AlarmMessage { get; set; }

        // 预警触发时的数据值，可能是一个变量的值，也可能是多个变量的值，取决于报警条件中使用了多少个变量
        // 用JSON格式保存，格式为{"varNum": "varNum1", "varValue": "value1", ...}
        [Column(name: "alarm_values")]
        public IList<AlarmVariable> AlarmValues { get; set; }

        [Column(name: "tag")]
        public string? AlarmTag { get; set; }

        [Column(name: "level")]
        public int AlarmLevel { get; set; }

        [Column("alarm_time")]
        public string? AlarmTime { get; set; }

        [Column("solve_time")]
        public string? SolvedTime { get; set; }
        #endregion
        /// <summary>
        /// 操作员
        /// </summary>
        [Column(name: "user_id")]
        public string UserId { get; set; }

        [Column(name: "date_time")]
        public string RecordTime { get; set; }
    }
}
