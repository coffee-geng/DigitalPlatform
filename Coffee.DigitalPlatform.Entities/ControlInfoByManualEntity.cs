using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Entities
{
    public class ControlInfoByManualEntity
    {
        [Column(name: "id")]
        public string Id { get; set; }

        [Column(name: "c_num")]
        public string CNum { get; set; }

        [Column(name: "c_header")]
        public string Header { get; set; }

        [Column(name: "v_num")]
        public string VarNum { get; set; }

        [Column(name: "c_value")]
        public string Value { get; set; }

        [Column(name: "d_num")]
        public string DeviceNum { get; set; }
    }
}
