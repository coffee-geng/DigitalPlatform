using Coffee.DigitalPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.ViewModels
{
    public class TrendHelper
    {
        public static TrendSeriesInfo createTrendSeries(TrendVariableInfo trendVariable)
        {
            TrendSeriesInfo seriesInfo = null;
            if (trendVariable.VariableType == typeof(bool))
                seriesInfo = createTrendStepLineSeries<bool>(trendVariable);
            else if (trendVariable.VariableType == typeof(byte))
                seriesInfo = createTrendLineSeries<byte>(trendVariable);
            else if (trendVariable.VariableType == typeof(short))
                seriesInfo = createTrendLineSeries<short>(trendVariable);
            else if (trendVariable.VariableType == typeof(ushort))
                seriesInfo = createTrendLineSeries<ushort>(trendVariable);
            else if (trendVariable.VariableType == typeof(int))
                seriesInfo = createTrendLineSeries<int>(trendVariable);
            else if (trendVariable.VariableType == typeof(uint))
                seriesInfo = createTrendLineSeries<uint>(trendVariable);
            else if (trendVariable.VariableType == typeof(long))
                seriesInfo = createTrendLineSeries<long>(trendVariable);
            else if (trendVariable.VariableType == typeof(ulong))
                seriesInfo = createTrendLineSeries<ulong>(trendVariable);
            else if (trendVariable.VariableType == typeof(float))
                seriesInfo = createTrendLineSeries<float>(trendVariable);
            else if (trendVariable.VariableType == typeof(double))
                seriesInfo = createTrendLineSeries<double>(trendVariable);
            else if (trendVariable.VariableType == typeof(decimal))
                seriesInfo = createTrendLineSeries<decimal>(trendVariable);

            return seriesInfo;
        }

        public static TrendLineSeriesInfo<T> createTrendLineSeries<T>(TrendVariableInfo trendVariable)
        {
            var seriesInfo = new TrendLineSeriesInfo<T>()
            {
                TrendDeviceNum = trendVariable.DeviceNum,
                TrendVariableNum = trendVariable.VariableNum,
                Title = trendVariable.VariableName,
                AxisXNum = trendVariable.AxisXNum,
                AxisYNum = trendVariable.AxisYNum,
                Color = trendVariable.Color,
            };
            return seriesInfo;
        }

        public static TrendStepLineSeriesInfo<T> createTrendStepLineSeries<T>(TrendVariableInfo trendVariable)
        {
            var seriesInfo = new TrendStepLineSeriesInfo<T>()
            {
                TrendDeviceNum = trendVariable.DeviceNum,
                TrendVariableNum = trendVariable.VariableNum,
                Title = trendVariable.VariableName,
                AxisXNum = trendVariable.AxisXNum,
                AxisYNum = trendVariable.AxisYNum,
                Color = trendVariable.Color,
            };
            return seriesInfo;
        }
    }
}
