using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public interface IPropertyCollection
    {
        List<IPropertyMetadata> Properties { get; }

        IPropertyMetadata? GetProperty(string propertyName);
    }
}
