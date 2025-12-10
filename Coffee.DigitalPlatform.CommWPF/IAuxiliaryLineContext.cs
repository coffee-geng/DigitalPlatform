using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.CommWPF
{
    public interface IAuxiliaryLineContext
    {
        AuxiliaryLineTypes AuxiliaryType { get; set; }

        bool IsVisible {  get; set; }

        double X { get; set; }

        double Y { get; set; }

        int Z { get; set; }

        double Width { get; set; }

        double Height { get; set; }
    }
}
