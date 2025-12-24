using Coffee.DigitalPlatform.Common;
using Coffee.DigitalPlatform.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Coffee.DigitalPlatform.Views
{
    public class SelectCommunicationParameterValueCommandParameterConverter : IMultiValueConverter
    {
        public static SelectCommunicationParameterValueCommandParameterConverter Instance { get; } = new SelectCommunicationParameterValueCommandParameterConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 3)
                return null;
            if (values[0] == null || values[0] == DependencyProperty.UnsetValue || !(values[0] is CommunicationParameterOption paramOption))
                return null;
            if (values[1] == null || values[1] == DependencyProperty.UnsetValue || !(values[1] is CommunicationParameterDefinition paramDef))
                return null;
            if (values[2] == null || values[2] == DependencyProperty.UnsetValue || !(values[2] is int indexOfCommParams))
                return null;
            return new SelectCommunicationParameterValueCommandParameter()
            {
                ParameterDef = paramDef,
                ParameterValue = paramOption,
                IndexOfCommunicationParameters = indexOfCommParams
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class CommunicationParameterSelectorSourceConverter : IMultiValueConverter
    {
        public static CommunicationParameterSelectorSourceConverter Instance { get; } = new CommunicationParameterSelectorSourceConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 3)
                return null;
            if (values[0] == null || values[0] == DependencyProperty.UnsetValue || !(values[0] is ObservableCollection<CommunicationParameterDefinition> paramDefinitions))
                return null;
            if (values[1] == null || values[1] == DependencyProperty.UnsetValue || !(values[1] is ObservableCollection<CommunicationParameter> parameters))
                return null;
            if (values[2] == null || values[2] == DependencyProperty.UnsetValue || !(values[2] is CommunicationParameter curParam))
                return null;

            List<CommunicationParameterDefinition> tempParamDefs = new List<CommunicationParameterDefinition>();
            if (string.Equals(curParam.PropName, "Protocol"))
            {
                var protocolParam = paramDefinitions.FirstOrDefault(paramDef => string.Equals(paramDef.ParameterName, "Protocol"));
                if (protocolParam != null)
                {
                    tempParamDefs.Add(protocolParam);
                }
            }
            else
            {
                foreach (var paramDef in paramDefinitions)
                {
                    if (parameters.Any(para => string.Equals(para.PropName, paramDef.ParameterName)))
                    {
                        if (!string.Equals(curParam.PropName, paramDef.ParameterName)) //在某一行的通信参数项中，当前添加项肯定是在当前下拉框的选项列表中的
                        {
                            continue;
                        }
                    }
                    tempParamDefs.Add(paramDef);
                }
            }
            return tempParamDefs;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ActiveCommunicationParameterIndexConverter : IMultiValueConverter
    {
        public static ActiveCommunicationParameterIndexConverter Instance { get; } = new ActiveCommunicationParameterIndexConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return null;
            if (values[0] == null || values[0] == DependencyProperty.UnsetValue || !(values[0] is CommunicationParameter param))
                return null;
            if (values[1] == null || values[1] == DependencyProperty.UnsetValue || !(values[1] is IList<CommunicationParameterDefinition> paramDefinitions))
                return null;
            var activeParam = paramDefinitions.Where(paramDef => paramDef.ParameterName == param.PropName).FirstOrDefault();
            return activeParam != null ? paramDefinitions.IndexOf(activeParam) : -1;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ActiveCommunicationParameterValueIndexConverter : IMultiValueConverter
    {
        public static ActiveCommunicationParameterValueIndexConverter Instance { get; } = new ActiveCommunicationParameterValueIndexConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return null;
            if (values[0] == null || values[0] == DependencyProperty.UnsetValue || !(values[0] is CommunicationParameter param))
                return null;
            if (values[1] == null || values[1] == DependencyProperty.UnsetValue || !(values[1] is CommunicationParameterDefinition paramDef))
                return null;
            var valOptions = paramDef.ValueOptions = paramDef.ValueOptions ?? new List<CommunicationParameterOption>();
            if (!string.IsNullOrWhiteSpace(param.PropValue))
            {
                //如果通信参数有初始值，并且在选项列表中能找到，则返回对应的索引
                var activeOption = valOptions.Where(option => string.Equals(option.PropOptionValue, param.PropValue)).FirstOrDefault();
                if (activeOption != null)
                {
                    return valOptions.IndexOf(activeOption);
                }
                else
                {
                    return paramDef.DefaultOptionIndex;
                }
            }
            else
            {
                return paramDef.DefaultOptionIndex;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    // 点位的变量类型转换器。如果没有指定点位的变量类型，则默认为int类型。
    public class ActiveVariableTypeConverter : IMultiValueConverter
    {
        public static ActiveVariableTypeConverter Instance { get; } = new ActiveVariableTypeConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return null;
            if (values[0] == null || values[0] == DependencyProperty.UnsetValue || !(values[0] is Variable variable))
                return null;
            if (values[1] == null || values[1] == DependencyProperty.UnsetValue || !(values[1] is IList<VariableType> primitiveTypeList))
                return null;

            Type primitiveType = variable.VarType != null ? variable.VarType : typeof(int); //如果没有指定点位的变量类型，则默认为int类型
            var activePrimitive = primitiveTypeList.Where(t => t.TypeClass == primitiveType).FirstOrDefault();
            //return activePrimitive != null ? primitiveTypeList.IndexOf(activePrimitive) : -1;
            return activePrimitive;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
