using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.CommWPF
{
    public interface IAuxiliaryLineContext : IUIElementContext
    {
        AuxiliaryLineTypes AuxiliaryType { get; set; }

        bool IsVisible {  get; set; }
    }
}
