using Coffee.DigitalPlatform.Controls.FilterBuilder;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows;
using System.Reflection;
using Coffee.DigitalPlatform.Controls;
using System.Text.RegularExpressions;
using Coffee.DigitalPlatform.Models;

namespace Coffee.DigitalPlatform.Views
{
    [ValueConversion(typeof(Type), typeof(Visibility))]
    public class ValueTypeVisibilityConverter : IValueConverter
    {
        public static ValueTypeVisibilityConverter Instance { get; } = new ValueTypeVisibilityConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue || value.GetType() is not Type)
            {
                return Visibility.Collapsed;
            }
            var valueType = value as Type;
            bool isMatch = false;

            if (parameter is not null && parameter.GetType().IsArray)
            {
                var parameterType = (ValueControlType[])parameter;
                foreach (var p in parameterType)
                {
                    var pType = p.GetType();
                    if (pType.IsEnum)
                    {
                        var memberInfo = pType.GetMember(p.ToString())?.FirstOrDefault();
                        if (memberInfo != null)
                        {
                            var typeAttrs = memberInfo.GetCustomAttributes<ValueTypeAttribute>().DistinctBy(p => p.VarType);
                            if (typeAttrs != null && typeAttrs.Any())
                            {
                                if (typeAttrs.Any(attr => attr.VarType == valueType))
                                {
                                    isMatch = true;
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            else if (parameter != null && parameter is ValueControlType valueControlType)
            {
                var memberInfo = parameter.GetType().GetMember(valueControlType.ToString())?.FirstOrDefault();
                var typeAttrs = memberInfo.GetCustomAttributes<ValueTypeAttribute>().DistinctBy(p => p.VarType);
                if (typeAttrs != null && typeAttrs.Any())
                {
                    if (typeAttrs.Any(attr => attr.VarType == valueType))
                    {
                        isMatch = true;
                    }
                }
            }

            return isMatch ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class EnumValuesConverter : IValueConverter
    {
        public static EnumValuesConverter Instance { get; } = new EnumValuesConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue)
                return null;
            if (!value.GetType().IsEnum)
            {
                return null;
            }
            return Enum.GetValues(value.GetType());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    [ValueConversion(typeof(Type), typeof(ValueControlType))]
    public class ValueTypeToValueControlTypeConverter : IValueConverter
    {
        public static ValueTypeToValueControlTypeConverter Instance { get; } = new ValueTypeToValueControlTypeConverter();

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue || value.GetType() is not Type)
            {
                return ValueControlType.Text;
            }
            
            Type valueType = (Type)value;
            if (valueType == typeof(string))
            {
                return ValueControlType.Text;
            }
            else if (valueType == typeof(bool))
            {
                return ValueControlType.Boolean;
            }
            else if (valueType == typeof(byte[]))
            {
                return ValueControlType.SByte;
            }
            else if ((valueType == typeof(byte)) ||
                (valueType == typeof(short)) ||
                (valueType == typeof(ushort)) ||
                (valueType == typeof(int)) ||
                (valueType == typeof(uint)) ||
                (valueType == typeof(long)) ||
                (valueType == typeof(ulong)) ||
                (valueType == typeof(double)) ||
                (valueType == typeof(float)) ||
                (valueType == typeof(decimal)))
            {
                if (parameter is string && parameter.ToString() == "Generic")
                {
                    return ValueControlType.Numeric;
                }
                else
                {
                    if (valueType == typeof(byte))
                    {
                        return ValueControlType.Byte;
                    }
                    else if (valueType == typeof(byte[]))
                    {
                        return ValueControlType.SByte;
                    }
                    else if (valueType == typeof(short))
                    {
                        return ValueControlType.Short;
                    }
                    else if (valueType == typeof(ushort))
                    {
                        return ValueControlType.UnsignedShort;
                    }
                    else if (valueType == typeof(int))
                    {
                        return ValueControlType.Integer;
                    }
                    else if (valueType == typeof(uint))
                    {
                        return ValueControlType.UnsignedInteger;
                    }
                    else if (valueType == typeof(long))
                    {
                        return ValueControlType.Long;
                    }
                    else if (valueType == typeof(ulong))
                    {
                        return ValueControlType.UnsignedLong;
                    }
                    else if (valueType == typeof(double))
                    {
                        return ValueControlType.Double;
                    }
                    else if (valueType == typeof(float))
                    {
                        return ValueControlType.Float;
                    }
                    else if (valueType == typeof(decimal))
                    {
                        return ValueControlType.Decimal;
                    }
                    else
                    {
                        return ValueControlType.Numeric;
                    }
                }
            }
            else if (valueType == typeof(DateTime))
            {
                return ValueControlType.DateTime;
            }
            else if (valueType == typeof(TimeSpan))
            {
                return ValueControlType.TimeSpan;
            }
            else if (valueType == typeof(Enum))
            {
                return ValueControlType.Enum;
            }
            else
            {
                return ValueControlType.Text;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || value == DependencyProperty.UnsetValue || !value.GetType().IsEnum || value is ValueControlType)
            {
                return Visibility.Collapsed;
            }
            ValueControlType controlType = (ValueControlType)value;
            if (controlType == ValueControlType.Numeric)
            {
                var memberInfo = controlType.GetType().GetMember(controlType.ToString())?.FirstOrDefault();
                var typeAttrs = memberInfo.GetCustomAttributes<ValueTypeAttribute>().DistinctBy(p => p.VarType);
                
                var doubleType = typeAttrs.Where(attr => attr.VarType == typeof(double)).Select(attr => attr.VarType).FirstOrDefault();
                if (doubleType != null)
                    return doubleType;
                var intType = typeAttrs.Where(attr => attr.VarType == typeof(int)).Select(attr => attr.VarType).FirstOrDefault();
                if (intType != null) 
                    return intType;
                return typeAttrs.Select(attr => attr.VarType).FirstOrDefault();
            }
            else
            {
                var memberInfo = controlType.GetType().GetMember(controlType.ToString())?.FirstOrDefault();
                var type = memberInfo.GetCustomAttributes<ValueTypeAttribute>().Select(p => p.VarType).FirstOrDefault();
                return type != null ? type : typeof(string);
            }
        }
    }

    public class LinkageActionVariableValueConverter : IMultiValueConverter
    {
        public static LinkageActionVariableValueConverter Instance {  get; } = new LinkageActionVariableValueConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length == 0)
                return null;
            if (values[0] == null || values[0] == DependencyProperty.UnsetValue)
                return null;
            if (values[0] is Variable @var)
            {
                //根据变量从联控信息中找到相关变量对应的设置值
                if (values.Length > 1)
                {
                    if (values[1] != null && values[1] != DependencyProperty.UnsetValue && values[1] is ControlInfoByTrigger controlInfo)
                    {
                        var v1 = controlInfo.TempVariableValueDict.Where(a => a.Key.VarNum == @var.VarNum && a.Key.DeviceNum == @var.DeviceNum).Select(p => p.Value).FirstOrDefault();
                        if (v1 != null)
                        {
                            return v1;
                        }
                        var v2 = controlInfo.NewLinkageActions.Where(a => a.Variable.VarNum == var.VarNum && a.Variable.DeviceNum == var.DeviceNum).Select(a => a.Value).FirstOrDefault();
                        return v2;
                    }
                }
                return @var.FinalValue;
            }
            else
            {
                return values[0];
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ReplaceLinkageActionCommandParameterConverter : IMultiValueConverter
    {
        public static ReplaceLinkageActionCommandParameterConverter Instance { get; } = new ReplaceLinkageActionCommandParameterConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length == 0)
                return null;
            if (values[0] == null || values[0] == DependencyProperty.UnsetValue)
                return null;
            if (!(values[0] is Variable @var))
                return null;
            var newAction = new LinkageAction() { Variable = @var };
            if (values.Length > 1 && values[1] != null && values[1] != DependencyProperty.UnsetValue && values[1] is ControlInfoByTrigger controlInfo)
            {
                var v1 = controlInfo.TempVariableValueDict.Where(a => a.Key.VarNum == @var.VarNum && a.Key.DeviceNum == @var.DeviceNum).Select(p => p.Value).FirstOrDefault();
                if (v1 != null)
                {
                    newAction.Value = v1;
                }
                else
                {
                    var v2 = controlInfo.NewLinkageActions.Where(a => a.Variable.VarNum == var.VarNum && a.Variable.DeviceNum == var.DeviceNum).Select(a => a.Value).FirstOrDefault();
                    newAction.Value = v2;
                }
                if (values.Length >2 && values[2] != null && values[2] != DependencyProperty.UnsetValue && values[2] is LinkageAction oldAction)
                {
                    if (oldAction.Variable != null && (oldAction.Variable.VarNum != @var.VarNum || oldAction.Variable.DeviceNum != var.DeviceNum))
                    {
                        return new ReplaceLinkageActionCommandParameter() { NewAction = newAction, OldAction = oldAction };
                    }
                    else
                    {
                        return null;
                    }
                }
                else
                {
                    return new ReplaceLinkageActionCommandParameter() { NewAction = newAction };
                }
            }
            else
            {
                newAction.Value = @var.FinalValue;
                return new ReplaceLinkageActionCommandParameter() { NewAction = newAction };
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
