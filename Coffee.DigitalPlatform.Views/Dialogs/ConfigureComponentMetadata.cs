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

    public class ActiveCommunicationParameterIndexConverter : IMultiValueConverter
    {
        public static ActiveCommunicationParameterIndexConverter Instance { get; } = new ActiveCommunicationParameterIndexConverter();

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 2)
                return null;
            if (values[0] == null || values[0] == DependencyProperty.UnsetValue || !(values[0] is CommunicationParameter param))
                return null;
            if (values[1] == null || values[1] == DependencyProperty.UnsetValue || !(values[1] is ObservableCollection<CommunicationParameterDefinition> paramDefinitions))
                return null;
            var activeParam = paramDefinitions.Where(paramDef => paramDef.ParameterName == param.PropName).FirstOrDefault();
            return activeParam != null ? paramDefinitions.IndexOf(activeParam) : -1;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
