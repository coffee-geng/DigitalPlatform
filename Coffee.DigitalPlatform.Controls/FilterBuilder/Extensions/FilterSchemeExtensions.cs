using System;
using System.Collections;
using System.Linq;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public static class FilterSchemeExtensions
    {
        private const string Separator = "||";

        public static void Apply(this FilterScheme filterScheme, IEnumerable rawCollection, IList filteredCollection)
        {
            ArgumentNullException.ThrowIfNull(filterScheme);
            ArgumentNullException.ThrowIfNull(rawCollection);
            ArgumentNullException.ThrowIfNull(filteredCollection);

            filteredCollection.Clear();

            foreach (var item in rawCollection.Cast<object>().Where(filterScheme.CalculateResult))
            {
                filteredCollection.Add(item);
            }
        }
    }
}
