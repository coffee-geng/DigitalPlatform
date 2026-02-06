using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.Mitsubishi.Base
{
    public class DataParameter
    {
        public Areas Area { get; set; }
        public string Address { get; set; }

        public int Count { get; set; }

        public List<byte> Datas { get; set; }
    }
}
