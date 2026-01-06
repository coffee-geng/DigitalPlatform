using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public class FilterSchemeEditInfo
    {
        public FilterSchemeEditInfo(FilterScheme filterScheme, IEnumerable rawCollection, bool allowLivePreview, bool enableAutoCompletion)
        {
            ArgumentNullException.ThrowIfNull(filterScheme);
            ArgumentNullException.ThrowIfNull(rawCollection);

            FilterScheme = filterScheme;
            RawCollection = rawCollection;
            AllowLivePreview = allowLivePreview;
            EnableAutoCompletion = enableAutoCompletion;
        }

        public FilterScheme FilterScheme { get; private set; }

        public IEnumerable RawCollection { get; private set; }

        public bool AllowLivePreview { get; private set; }

        public bool EnableAutoCompletion { get; private set; }
    }
}
