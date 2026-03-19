using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Entities
{
    public class TrendEntity
    {
        [Column("trend_num")]
        public string TrendNum { get; set; }

        [Column("trend_header")]
        public string Header { get; set; }

        [Column("show_legend")]
        public bool IsShowLegend { get; set; }

        [NotMapped]
        public AxisEntity AxisX { get; set; }

        [NotMapped]
        public IEnumerable<AxisEntity> AxisYList { get; set; }

        [NotMapped]
        public IEnumerable<SeriesEntity> Series { get; set; }
    }

    public class AxisEntity
    {
        [Column("axis_type")]
        public string AxisType { get; set; }

        [Column("axis_num")]
        public string AxisNum { get; set; }

        [Column("trend_num")]
        public string TrendNum { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("show_title")]
        public bool IsShowTitle { get; set; }

        [Column("min")]
        public string Minimum { get; set; }

        [Column("max")]
        public string Maximum { get; set; }

        [Column("show_seperator")]
        public bool IsShowSeperator { get; set; }

        [Column("label_formatter")]
        public string LabelFormatter { get; set; }

        [Column("position")]
        public string Position { get; set; }

        [NotMapped]
        public IEnumerable<SectionEntity> Sections { get; set; }
    }

    public class SectionEntity
    {
        [Column("axis_num")]
        public string AxisNum { get; set; }

        [Column("value")]
        public string Value { get; set; }

        [Column("color")]
        public string Color { get; set; }
    }

    public class SeriesEntity
    {
        [Column("device_num")]
        public string DeviceNum { get; set; }

        [Column("var_num")]
        public string VarNum { get; set; }

        [Column("trend_num")]
        public string TrendNum { get; set; }

        [Column("axis_index")]
        public string AxisNum { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("color")]
        public string Color { get; set; }
    }
}
