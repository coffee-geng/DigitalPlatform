using Coffee.DigitalPlatform.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Coffee.DigitalPlatform.ViewModels
{
    public class ConfigTrendDialogViewModel : ObservableObject
    {
        public ConfigTrendDialogViewModel(TrendChartInfo currentTrend, IList<Device> deviceList)
        {
            BrushNames = typeof(Brushes).GetProperties().Select(p => p.Name).ToList();
            CurrentTrend = currentTrend;

            TrendDeviceList = deviceList.Where(d => d.Variables.Count > 0).Select(d => new TrendDeviceInfo()
            {
                Header = d.Name,
                Variables = new System.Collections.ObjectModel.ObservableCollection<TrendVariableInfo>(d.Variables.Select(@var => initTrendVariable(d, @var)))
            }).ToList();
        }

        private TrendChartInfo _currentTrend;
        public TrendChartInfo CurrentTrend
        {
            get { return _currentTrend; }
            set { SetProperty(ref _currentTrend, value); }
        }

        private IList<TrendDeviceInfo> _trendDeviceList;
        //编辑趋势图时，待选的设备信息列表
        public IList<TrendDeviceInfo> TrendDeviceList
        {
            get { return _trendDeviceList; }
            set { SetProperty(ref _trendDeviceList, value); }
        }

        public IList<string> BrushNames { get; }

        //初始化待选设备和点位信息选项
        private TrendVariableInfo initTrendVariable(Device device, Variable variable)
        {
            if (device == null)
                throw new ArgumentNullException(nameof(device));
            if (variable == null)
                throw new ArgumentNullException(nameof(variable));

            var trendVariable = new TrendVariableInfo()
            {
                DeviceNum = device.DeviceNum,
                VariableNum = variable.VarNum,
                VariableName = variable.VarName,
                VariableType = variable.VarType
            };

            //判断当前编辑的编辑的趋势图中是否已经关联了参数指定的设备和变量信息
            //如果已经关联，则需设置成已选状态
            TrendSeriesInfo activeTrendSeries = null;
            if (CurrentTrend != null)
            {
                activeTrendSeries = CurrentTrend.Series.FirstOrDefault(s => s.TrendDeviceNum == device.DeviceNum && s.TrendVariableNum == variable.VarNum);
            }
            trendVariable.IsSelected = activeTrendSeries != null;
            //如果待选的设备和点位信息已经应用于当前编辑的趋势图中,则使用当前应用的数据
            if (trendVariable.IsSelected)
            {
                trendVariable.AxisXNum = activeTrendSeries.AxisXNum;
                trendVariable.AxisYNum = activeTrendSeries.AxisYNum;
                trendVariable.Color = activeTrendSeries.Color;
            }
            else if (CurrentTrend != null)
            {
                trendVariable.AxisXNum = CurrentTrend.AxisXCollection.FirstOrDefault()?.AxisNum;
                trendVariable.AxisYNum = CurrentTrend.AxisYCollection.FirstOrDefault()?.AxisNum;
                trendVariable.Color = BrushNames[new Random().Next(0, BrushNames.Count - 1)];
            }

            //监听TrendVariableInfo的属性
            trendVariable.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(TrendVariableInfo.IsSelected)) //选中状态，则添加序列；切换到未选中，则移除这个序列
                {
                    onTrendVariableSelectStateChanged(s as TrendVariableInfo);
                }
                else if (e.PropertyName == nameof(TrendSeriesInfo.Color))
                {
                    onTrendVariablePropertyChanged(s as TrendVariableInfo, e.PropertyName);
                }
                else if (e.PropertyName == nameof(TrendSeriesInfo.AxisXNum))
                {
                    onTrendVariablePropertyChanged(s as TrendVariableInfo, e.PropertyName);
                }
                else if (e.PropertyName == nameof(TrendSeriesInfo.AxisYNum))
                {
                    onTrendVariablePropertyChanged(s as TrendVariableInfo, e.PropertyName);
                }
            };

            return trendVariable;
        }

        private void onTrendVariableSelectStateChanged(TrendVariableInfo trendVariable)
        {
            if (CurrentTrend == null || trendVariable == null)
                return;
            var series = CurrentTrend.Series.FirstOrDefault(s => s.TrendDeviceNum == trendVariable.DeviceNum && s.TrendVariableNum == trendVariable.VariableNum);
            if (trendVariable.IsSelected)
            {
                if (series == null) //如果当前趋势图中还有没参数（设备和变量）指定相关序列，则添加序列
                {
                    TrendSeriesInfo seriesInfo = null;
                    if (trendVariable.VariableType == typeof(bool))
                        seriesInfo = createTrendLineSeries<bool>(trendVariable);
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

                    if (seriesInfo != null)
                    {
                        CurrentTrend.Series.Add(seriesInfo);
                    }
                }
            }
            else
            {
                if (series != null)
                {
                    CurrentTrend.Series.Remove(series);
                }
            }
        }

        private TrendLineSeriesInfo<T> createTrendLineSeries<T>(TrendVariableInfo trendVariable)
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

        private void onTrendVariablePropertyChanged(TrendVariableInfo trendVariable, string propertyName)
        {
            if (CurrentTrend == null || trendVariable == null)
                return;
            var series = CurrentTrend.Series.FirstOrDefault(s => s.TrendDeviceNum == trendVariable.DeviceNum && s.TrendVariableNum == trendVariable.VariableNum);
            if (series == null)
                return;
            if (propertyName == nameof(TrendSeriesInfo.Color))
                series.Color = trendVariable.Color;
            else if (propertyName == nameof(TrendSeriesInfo.AxisXNum))
                series.AxisXNum = trendVariable.AxisXNum;
            else if (propertyName == nameof(TrendSeriesInfo.AxisYNum))
                series.AxisYNum = trendVariable.AxisYNum;
        }
    }
}
