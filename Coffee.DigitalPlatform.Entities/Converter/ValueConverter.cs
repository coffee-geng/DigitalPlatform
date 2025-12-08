using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Entities.Converter
{
    public abstract class ValueConverter<TProperty, TDatabase>
    {
        public abstract TProperty ConvertFromDatabase(TDatabase value);
        public abstract TDatabase ConvertToDatabase(TProperty value);
    }

    public class IntToStringConverter : ValueConverter<int, string>
    {
        public override int ConvertFromDatabase(string value) => int.TryParse(value, out int i) ? i : 0;
        public override string ConvertToDatabase(int value) => value.ToString();
    }
}
