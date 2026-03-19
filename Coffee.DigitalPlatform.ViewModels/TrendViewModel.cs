using Coffee.DigitalPlatform.CommWPF;
using Coffee.DigitalPlatform.Entities;
using Coffee.DigitalPlatform.IDataAccess;
using Coffee.DigitalPlatform.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.ViewModels
{
    public class TrendViewModel : ObservableObject, INavigationService, IDisposable
    {
        public TrendViewModel(MonitorComponentViewModel monitorViewModel, ILocalDataAccess localDataAccess)
        {
            if (monitorViewModel == null)
                throw new ArgumentNullException(nameof(monitorViewModel));
            if (localDataAccess == null)
                throw new ArgumentNullException(nameof(localDataAccess));
            _monitorViewModel = monitorViewModel;
            _localDataAccess = localDataAccess;

            AddTrendCommand = new RelayCommand(doAddTrendCommand);
            RemoveTrendCommand = new RelayCommand<TrendChartInfo>(doRemoveTrendCommand, canRemoveTrendCommand);
            ShowConfigTrendDialogCommand = new RelayCommand(doShowConfigTrendDialogCommand, canShowConfigTrendDialogCommand);
            ShowConfigAxisDialogCommand = new RelayCommand(doShowConfigAxisDialogCommand, canShowConfigAxisDialogCommand);

            //返回当前趋势图要求使用的设备信息
            WeakReferenceMessenger.Default.Register<Action<Func<IEnumerable<string>, IEnumerable<Device>>>>(this, new MessageHandler<object, Action<Func<IEnumerable<string>, IEnumerable<Device>>>>((obj, action) =>
            {
                if (action != null)
                {
                    action.Invoke(deviceNumList =>
                    {
                        if (deviceNumList == null)
                            return Enumerable.Empty<Device>();
                        return _monitorViewModel.DeviceList.Where(device => deviceNumList.Contains(device.DeviceNum));
                    });
                }
            }));
        }

        private MonitorComponentViewModel _monitorViewModel;

        private ILocalDataAccess _localDataAccess;

        public ObservableCollection<TrendChartInfo> Trends { get; } = new ObservableCollection<TrendChartInfo>();

        private TrendChartInfo _currentTrend;
        public TrendChartInfo CurrentTrend
        {
            get { return _currentTrend; }
            set { SetProperty(ref _currentTrend, value); }
        }

        public RelayCommand AddTrendCommand {  get; set; }

        public RelayCommand<TrendChartInfo> RemoveTrendCommand { get; set; }

        private void doAddTrendCommand()
        {
            foreach (var trend in Trends)
            {
                trend.IsSelected = false;
            }
            var trendInfo = new TrendChartInfo()
            {
                IsSelected = true
            };
            CurrentTrend = trendInfo;
        }

        private void doRemoveTrendCommand(TrendChartInfo trendInfo)
        {
            if (canRemoveTrendCommand(trendInfo))
            {
                if (trendInfo.IsSelected) //如果删除的是当前选中的图表，则在删除后，需选择上一个图表，如果删除的是第一个图表，则选择下一个
                {
                    int idx = Trends.IndexOf(trendInfo);
                    Trends.RemoveAt(idx);
                    if (idx == 0)
                        Trends.First().IsSelected = true;
                    else
                        Trends[idx-1].IsSelected = true;
                }
                else
                {
                    Trends.Remove(trendInfo);
                }
            }
        }

        private bool canRemoveTrendCommand(TrendChartInfo trendInfo)
        {
            if (trendInfo != null)
            {
                int idx = Trends.IndexOf(trendInfo);
                if (idx < 0)
                    return false;
                else return Trends.Count > 1 ? true : false; //最少保留一个趋势图
            }
            else
            {
                return false;
            }
        }

        public RelayCommand ShowConfigTrendDialogCommand { get; set; }

        public RelayCommand ShowConfigAxisDialogCommand {  get; set; }

        private void doShowConfigTrendDialogCommand()
        {
            if (canShowConfigTrendDialogCommand())
            {
                var deviceList = _monitorViewModel.DeviceList;
                ActionManager.Execute("ShowConfigTrendDialog", new ConfigTrendDialogViewModel(CurrentTrend, deviceList));
            }
        }

        private bool canShowConfigTrendDialogCommand()
        {
            return CurrentTrend != null;
        }

        private void doShowConfigAxisDialogCommand()
        {
            if (canShowConfigAxisDialogCommand())
            {
                ActionManager.Execute("ShowConfigAxisDialog", new ConfigAxisDialogViewModel(CurrentTrend));
            }
        }

        private bool canShowConfigAxisDialogCommand()
        {
            return CurrentTrend != null;
        }

        IEnumerable<TrendChartInfo> entityToModel(IEnumerable<TrendEntity> trendEntities)
        {
            if (trendEntities == null || !trendEntities.Any())
                return Enumerable.Empty<TrendChartInfo>();
            IList<TrendChartInfo> trends = new List<TrendChartInfo>();
            foreach (var trendEntity in trendEntities)
            {
                var trend = new TrendChartInfo(trendEntity.TrendNum)
                {
                    Header = trendEntity.Header,
                    IsShowLegend = trendEntity.IsShowLegend
                };

                var funcAxis = new Func<AxisEntity, TrendAxisInfo>(axisEntity =>
                {
                    var axis = new TrendAxisInfo(axisEntity.AxisNum)
                    {
                        Title = axisEntity.Title,
                        IsShowTitle = axisEntity.IsShowTitle,
                        IsShowSeperator = axisEntity.IsShowSeperator,
                        LabelFormatter = axisEntity.LabelFormatter,
                    };
                    if (Enum.TryParse(typeof(LiveCharts.AxisPosition), axisEntity.Position, out object? position))
                    {
                        axis.AxisPosition = (LiveCharts.AxisPosition)position;
                    }
                    if (double.TryParse(axisEntity.Minimum, out var minimum))
                    {
                        axis.Minimum = minimum;
                    }
                    if (double.TryParse(axisEntity.Maximum, out var maximum))
                    {
                        axis.Maximum = maximum;
                    }

                    if (axisEntity.Sections != null && axisEntity.Sections.Count() > 0)
                    {
                        foreach (var sectionEntity in axisEntity.Sections)
                        {
                            var section = new TrendSectionInfo()
                            {
                                Value = double.Parse(sectionEntity.Value.ToString()),
                                Color = sectionEntity.Color
                            };
                            axis.Sections.Add(section);
                        }
                    }
                    return axis;
                });
                if (trendEntity.AxisX != null)
                {
                    trend.AxisX = funcAxis.Invoke(trendEntity.AxisX);
                }
                if (trendEntity.AxisYList != null && trendEntity.AxisYList.Any())
                {
                    foreach (var axisYEntity in trendEntity.AxisYList)
                    {
                        var axisY = funcAxis.Invoke(axisYEntity);
                        trend.AxisYCollection.Add(axisY);
                    }
                }
                trend.EnsureAxis();

                foreach(var seriesEntity in trendEntity.Series)
                {
                    var series = createTrendSeries(seriesEntity, seriesEntity.AxisNum);
                    trend.Series.Add(series);
                }

                trends.Add(trend);
            }
            return trends;
        }

        private TrendSeriesInfo createTrendSeries(SeriesEntity seriesEntity, string axisYNum)
        {
            if (seriesEntity == null || string.IsNullOrWhiteSpace(seriesEntity.DeviceNum) || string.IsNullOrWhiteSpace(seriesEntity.VarNum))
                return null;
            if (_monitorViewModel.DeviceList == null)
                return null;
            var device = _monitorViewModel.DeviceList.Where(d => d.DeviceNum == seriesEntity.DeviceNum).FirstOrDefault();
            if (device == null)
                return null;
            var variable = device.Variables.Where(@var => var.VarNum == seriesEntity.VarNum).FirstOrDefault();
            if (variable == null)
                return null;
            var trendVariable = new TrendVariableInfo()
            {
                AxisYNum = axisYNum,
                DeviceNum = device.DeviceNum,
                VariableNum = variable.VarNum,
                VariableName = variable.VarName,
                VariableType = variable.VarType,
                Color = seriesEntity.Color
            };
            return TrendHelper.createTrendSeries(trendVariable);
        }

        public void OnNavigateTo(NavigationContext context = null)
        {
            var trendEntities = _localDataAccess.ReadTrends();
            if (trendEntities.Any())
            {
                Trends.Clear();
                var trendInfos = entityToModel(trendEntities);
                var firstItem = trendInfos.First();
                foreach(var trendInfo in trendInfos)
                {
                    trendInfo.IsSelected = trendInfo == firstItem;
                    Trends.Add(trendInfo);
                }
            }
            else if (!Trends.Any()) //保证最少有一个趋势图
            {
                Trends.Add(new TrendChartInfo()
                {
                    IsSelected = true,
                });
            }
            CurrentTrend = Trends.FirstOrDefault();

            foreach (var trendInfo in Trends)
            {
                trendInfo.BeginRefreshSeries();
            }
            _monitorViewModel.BeginMonitor();
        }

        public void OnNavigateFrom(NavigationContext context = null)
        {
            if (Trends == null || !Trends.Any())
                return;
            foreach (var trendInfo in Trends)
            {
                trendInfo.StopRefreshSeries();
            }
        }

        public void Dispose()
        {
            //页面切换或者关闭窗口都需要释放Task
            if (Trends == null || !Trends.Any())
                return;
            foreach (var trendInfo in Trends)
            {
                trendInfo.StopRefreshSeries();
            }
        }
    }
}
