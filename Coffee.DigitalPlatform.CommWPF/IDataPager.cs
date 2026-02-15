using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Coffee.DigitalPlatform.CommWPF
{
    public interface IDataPager
    {
        int CurrentPage { get; set; }
        int PageSize { get; set; }
    }
}
