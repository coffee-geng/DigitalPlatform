using Coffee.DigitalPlatform.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Entities
{
    public class TrendEntity : IEquatable<TrendEntity>
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

        public bool Equals(TrendEntity? other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            bool b1 = string.Equals(this.TrendNum, other.TrendNum);
            bool b2 = string.Equals(this.Header, other.Header);
            bool b3 = string.Equals(this.IsShowLegend, other.IsShowLegend);
            
            bool b4 = string.Equals(this.AxisX, other.AxisX);
            bool b5 = TypeUtils.EqualCollection(this.AxisYList, other.AxisYList);
            bool b6 = TypeUtils.EqualCollection(this.Series, other.Series);

            bool a = b1 && b2 && b3;
            bool b = b4 && b5 && b6;
            return a && b;
        }
    }

    public class AxisEntity : IEquatable<AxisEntity>
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

        public bool Equals(AxisEntity? other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            bool b1 = string.Equals(this.TrendNum, other.TrendNum);
            bool b2 = string.Equals(this.AxisNum, other.AxisNum);
            bool b3 = string.Equals(this.AxisType, other.AxisType);
            bool b4 = string.Equals(this.Title, other.Title);
            bool b5 = string.Equals(this.IsShowTitle, other.IsShowTitle);
            bool b6 = string.Equals(this.Minimum, other.Minimum);
            bool b7 = string.Equals(this.Maximum, other.Maximum);
            bool b8 = string.Equals(this.Position, other.Position);
            bool b9 = string.Equals(this.LabelFormatter, other.LabelFormatter);
            bool b10 = string.Equals(this.IsShowSeperator, other.IsShowSeperator);

            bool b11 = TypeUtils.EqualCollection(this.Sections, other.Sections);

            bool a = b1 && b2 && b3 && b4 && b5 && b6 && b7 && b8 && b9 && b10;
            bool b = b11;
            return a && b;
        }
    }

    public class SectionEntity : IEquatable<SectionEntity>
    {
        [Column("section_num")]
        public string SectionNum { get; set; }

        [Column("axis_num")]
        public string AxisNum { get; set; }

        [Column("value")]
        public string Value { get; set; }

        [Column("color")]
        public string Color { get; set; }

        public bool Equals(SectionEntity? other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            bool b1 = string.Equals(this.SectionNum, other.SectionNum);
            bool b2 = string.Equals(this.AxisNum, other.AxisNum);
            bool b3 = string.Equals(this.Value, other.Value);
            bool b4 = string.Equals(this.Color, other.Color);
            return b1 && b2 && b3 && b4;
        }
    }

    public class SeriesEntity : IEquatable<SeriesEntity>
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

        public bool Equals(SeriesEntity? other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            bool b1 = string.Equals(this.TrendNum, other.TrendNum);
            bool b2 = string.Equals(this.AxisNum, other.AxisNum);
            bool b3 = string.Equals(this.DeviceNum, other.DeviceNum);
            bool b4 = string.Equals(this.VarNum, other.VarNum);
            bool b5 = string.Equals(this.Title, other.Title);
            bool b6 = string.Equals(this.Color, other.Color);
            return b1 && b2 && b3 && b4 && b5 && b6;
        }
    }
}
