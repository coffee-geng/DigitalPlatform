using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Entities
{
    public class DeviceEntity
    {
        public string DeviceNum { get; set; }
        public string X { get; set; }
        public string Y { get; set; }
        public string Z { get; set; }
        public string Width { get; set; }
        public string Height { get; set; }
        public string DeviceTypeName { get; set; }
        public string Label { get; set; }

        public string FlowDirection { get; set; } = "0";
        public string Rotate { get; set; } = "0";

        public List<CommunicationParameterEntity> CommunicationParameters { get; set; }
        public List<VariableEntity> Variables { get; set; }
        public List<ManualControlEntity> ManualControls { get; set; }
    }
}
