using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Entities
{
    public class SettingInfoEntity
    {
        [Column("info_num")]
        public string InfoNum { get; set; }

        [Column("header")]
        public string Title { get; set; }

        [Column("content")]
        public string Description { get; set; }

        [Column("value")]
        public string Value { get; set; }

        [Column(name: "value_type")]
        public string ValueType { get; set; }

        [Column("device_num")]
        public string DeviceNum { get; set; }

        [Column("var_num")]
        public string VariableNum { get; set; }

        [Column("type")]
        public string Type { get; set; }
    }
}
