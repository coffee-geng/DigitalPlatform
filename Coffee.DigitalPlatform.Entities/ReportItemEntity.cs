using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Entities
{
    //用于统计设备点位信息
    public class ReportItemEntity
    {
        [Column("var_num")]
        public string VariableNum { get; set; }

        [Column("device_num")]
        public string DeviceNum { get; set; }

        [Column("var_name")]
        public string VariableName { get; set; }

        [Column("device_name")]
        public string DeviceName { get; set; }

        [Column("last_value")]
        public string LastValue { get; set; }//最新值

        [Column("avg")]
        public string AvageValue { get; set; }// 平均值

        [Column("max")]
        public string MaxValue { get; set; }// 记录中的最大值

        [Column("min")]
        public string MinValue { get; set; }// 记录中的最小值

        [Column("alarm_count")]
        public string AlarmCount { get; set; }// 报警触发次数

        [Column("linkage_count")]
        public string LinkageCount { get; set; }// 联控触发次数

        [Column("last_time")]
        public string LastTime { get; set; }// 最新记录时间

        [Column("record_count")]
        public string TotalCount { get; set; }// 总记录数
    }

    //用于写入设备点位的即时信息
    public class RecordEntity
    {
        [Column("device_num")]
        public string DeviceNum { get; set; }

        [Column("var_num")]
        public string VariableNum { get; set; }

        [Column("device_name")]
        public string DeviceName { get; set; }

        [Column("var_name")]
        public string VariableName { get; set; }

        [Column("record_value")]
        public string RecordValue { get; set; }

        [Column("alarm_num")]
        public string AlarmNum { get; set; }

        [Column("linkage_num")]
        public string LinkageNum { get; set; }

        [Column("record_time")]
        public DateTime RecordTime { get; set; }

        [Column("user_name")]
        public string UserName { get; set; }

        [Column("state")]
        public string State { get; set; }

        [Column(name: "statechange_history")]
        public int StateChangedHistory { get; set; }
    }
}
