using Coffee.DigitalPlatform.CommWPF;
using CommunityToolkit.Mvvm.Messaging.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Models
{
    public class ReportItem
    {
        public string VariableNum { get; set; }

        public string DeviceNum { get; set; }

        public string VariableName { get; set; }

        public string DeviceName { get; set; }

        public string LastValue { get; set; }//最新值

        public string AvageValue { get; set; }// 平均值

        public string MaxValue { get; set; }// 记录中的最大值

        public string MinValue { get; set; }// 记录中的最小值

        public string AlarmCount { get; set; }// 报警触发次数

        public string LinkageCount { get; set; }// 联控触发次数

        public string LastTime { get; set; }// 最新记录时间

        public string TotalCount { get; set; }// 总记录数
    }

    public class RecordItem
    {
        public string DeviceNum { get; set; }

        public string VariableNum { get; set; }

        public string DeviceName { get; set; }

        public string VariableName { get; set; }

        public string RecordValue { get; set; }

        public RecordStatus State {  get; set; }

        public string AlarmNum { get; set; }

        public string LinkageNum { get; set; }

        public string RecordTime { get; set; }

        public string UserName { get; set; }
    }

    public enum RecordStatus
    {
        Normal,
        Alarm,
        Linkage,
        AlamLinkage
    }

    public class ReportRecordsMessage : ValueChangedMessage<IEnumerable<RecordItem>>
    {
        public ReportRecordsMessage(IEnumerable<RecordItem> value) : base(value)
        {
        }
    }
}
