using Coffee.DigitalPlatform.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Entities
{
    public class DeviceEntity : IEquatable<DeviceEntity>
    {
        [Column(name: "d_num")]
        public string DeviceNum { get; set; }

        [Column(name: "x")]
        public string X { get; set; }

        [Column(name: "y")]
        public string Y { get; set; }

        [Column(name: "z")]
        public string Z { get; set; }

        [Column(name: "w")]
        public string Width { get; set; }

        [Column(name: "h")]
        public string Height { get; set; }

        [Column(name: "d_type_name")]
        public string DeviceTypeName { get; set; }

        [Column(name: "header")]
        public string Label { get; set; }

        [Column(name: "flow_direction")]
        public string FlowDirection { get; set; } = "0";

        [Column(name: "rotate")]
        public string Rotate { get; set; } = "0";

        [NotMapped]
        public List<CommunicationParameterEntity> CommunicationParameters { get; set; }

        [NotMapped]
        public List<VariableEntity> Variables { get; set; }

        public bool Equals(DeviceEntity? other)
        {
            if (other == null)
                return false;
            if (ReferenceEquals(this, other))
                return true;
            bool b1 = string.Equals(this.DeviceNum, other.DeviceNum);
            bool b2 = string.Equals(this.X, other.X);
            bool b3 = string.Equals(this.Y, other.Y);
            bool b4 = string.Equals(this.Z, other.Z);
            bool b5 = string.Equals(this.Width, other.Width);
            bool b6 = string.Equals(this.Height, other.Height);
            bool b7 = string.Equals(this.DeviceTypeName, other.DeviceTypeName);
            bool b8 = string.Equals(this.Label, other.Label);
            bool b9 = string.Equals(this.FlowDirection, other.FlowDirection);
            bool b10 = string.Equals(this.Rotate, other.Rotate);

            bool b11 = TypeUtils.EqualCollection(this.CommunicationParameters, other.CommunicationParameters);
            bool b12 = TypeUtils.EqualCollection(this.Variables, other.Variables);

            bool a = b1 && b2 && b3 && b4 && b5 && b6 && b7 && b8 && b9 && b10;
            bool b = b11 && b12;
            return a && b;
        }
    }
}
