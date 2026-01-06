using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public class DataTypeExpressionToConditionsConverter : IValueConverter
    {
        public static DataTypeExpressionToConditionsConverter Instance { get; } = new DataTypeExpressionToConditionsConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is not DataTypeExpression dataTypeExpression)
            {
                return null;
            }

            var isNullable = false;

            if (PropertyHelper.IsPropertyAvailable(dataTypeExpression, "IsNullable"))
            {
                isNullable = PropertyHelper.GetPropertyValue<bool>(dataTypeExpression, "IsNullable");
            }

            object conditions = isNullable ? ConditionHelper.GetNullableValueConditions() : ConditionHelper.GetValueConditions();

            switch (dataTypeExpression.ValueControlType)
            {
                case ValueControlType.Boolean:
                    conditions = ConditionHelper.GetBooleanConditions();
                    break;

                case ValueControlType.DateTime:
                    // No custom conditions
                    break;

                case ValueControlType.Enum:
                case ValueControlType.Byte:
                case ValueControlType.SByte:
                case ValueControlType.Short:
                case ValueControlType.UnsignedShort:
                case ValueControlType.Integer:
                case ValueControlType.UnsignedInteger:
                case ValueControlType.Long:
                case ValueControlType.UnsignedLong:
                case ValueControlType.Decimal:
                case ValueControlType.Float:
                case ValueControlType.Double:
                case ValueControlType.Numeric:
                    // No custom conditions
                    break;

                case ValueControlType.TimeSpan:
                    // No custom conditions
                    break;

                case ValueControlType.Text:
                    conditions = ConditionHelper.GetStringConditions();
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(value));
            }

            return conditions;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(Condition), typeof(string))]
    public class ConditionNameConverter : IValueConverter
    {
        public static ConditionNameConverter Instance { get; } = new ConditionNameConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
                return null;
            if (value is Condition condition)
            {
                var nameAttr = typeof(Condition).GetField(Enum.GetName(typeof(Condition), condition)).GetCustomAttribute<DisplayAttribute>();
                if (nameAttr != null && !string.IsNullOrWhiteSpace(nameAttr.Name))
                {
                    return LanguageHelper.GetString(nameAttr.Name);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(ConditionGroupType), typeof(string))]
    public class ConditionGroupTypeNameConverter : IValueConverter
    {
        public static ConditionGroupTypeNameConverter Instance { get; } = new ConditionGroupTypeNameConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
                return null;
            if (value is ConditionGroupType conditionGroupType)
            {
                var nameAttr = typeof(ConditionGroupType).GetField(Enum.GetName(typeof(ConditionGroupType), conditionGroupType)).GetCustomAttribute<DisplayAttribute>();
                if (nameAttr != null && !string.IsNullOrWhiteSpace(nameAttr.Name))
                {
                    return LanguageHelper.GetString(nameAttr.Name);
                }
                else
                {
                    return null;
                }
            }
            else
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
