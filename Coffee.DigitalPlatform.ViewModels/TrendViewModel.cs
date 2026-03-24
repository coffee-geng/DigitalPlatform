using Coffee.DigitalPlatform.CommWPF;
using Coffee.DigitalPlatform.Entities;
using Coffee.DigitalPlatform.IDataAccess;
using Coffee.DigitalPlatform.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

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

            SaveCommand = new RelayCommand<FrameworkElement>(doSaveCommand);
            SaveToImageCommand = new RelayCommand<object>(doSaveToImageCommand);

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
            set 
            { 
                if (SetProperty(ref _currentTrend, value))
                {
                    var otherTrends = Trends.Where(t => t != value);
                    foreach(var otherTrend in otherTrends)
                    {
                        //otherTrend.StopRefreshSeries();
                    }
                }
            }
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
            Trends.Add(trendInfo);
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

        #region 提示信息
        private string _failureMessageOnSaving;

        public string FailureMessageOnSaving
        {
            get { return _failureMessageOnSaving; }
            set { SetProperty(ref _failureMessageOnSaving, value); }
        }

        public RelayCommand<object> CloseErrorMessageBoxCommand { get; set; }

        private void doCloseErrorMessageBox(object owner)
        {
            VisualStateManager.GoToElementState(owner as FrameworkElement, "HideFailure", true);
        }
        #endregion

        #region Save
        public RelayCommand<FrameworkElement> SaveCommand { get; set; }

        public RelayCommand<object> SaveToImageCommand { get; set; }

        private void doSaveCommand(FrameworkElement owner)
        {
            VisualStateManager.GoToElementState(owner, "NormalToSuccess", false);
            VisualStateManager.GoToElementState(owner, "NormalToFailure", false);

            IList<TrendEntity> trendEntities = new List<TrendEntity>();
            if (Trends != null)
            {
                foreach (var trend in Trends)
                {
                    var trendEntity = new TrendEntity()
                    {
                        TrendNum = trend.ChartNum,
                        Header = trend.Header,
                        IsShowLegend = trend.IsShowLegend
                    };

                    if (trend.AxisX != null)
                    {
                        var axisX = trend.AxisX;
                        var axisXEntity = new AxisEntity()
                        {
                            TrendNum = trendEntity.TrendNum,
                            AxisNum = axisX.AxisNum,
                            AxisType = Enum.GetName(typeof(AxisTypes), AxisTypes.AxisX),
                            Title = axisX.Title,
                            IsShowTitle = axisX.IsShowTitle,
                            IsShowSeperator = axisX.IsShowSeperator,
                            LabelFormatter = axisX.LabelFormatter,
                            Minimum = axisX.Minimum.ToString(),
                            Maximum = axisX.Maximum.ToString(),
                            Position = Enum.GetName(typeof(AxisPosition), axisX.AxisPosition)
                        };
                        if (axisX.Sections != null)
                        {
                            axisXEntity.Sections = axisX.Sections.Select(s => new SectionEntity
                            {
                                SectionNum = s.SectionNum,
                                AxisNum = axisX.AxisNum,
                                Value = s.Value.ToString(),
                                Color = s.Color,
                            });
                        }
                        trendEntity.AxisX = axisXEntity;
                    }
                    if (trend.AxisYCollection != null && trend.AxisYCollection.Count > 0)
                    {
                        IList<AxisEntity> axisYEntities = new List<AxisEntity>();
                        foreach (var axisY in trend.AxisYCollection)
                        {
                            var axisYEntity = new AxisEntity()
                            {
                                TrendNum = trendEntity.TrendNum,
                                AxisNum = axisY.AxisNum,
                                AxisType = Enum.GetName(typeof(AxisTypes), AxisTypes.AxisY),
                                Title = axisY.Title,
                                IsShowTitle = axisY.IsShowTitle,
                                IsShowSeperator = axisY.IsShowSeperator,
                                LabelFormatter = axisY.LabelFormatter,
                                Minimum = axisY.Minimum.ToString(),
                                Maximum = axisY.Maximum.ToString(),
                                Position = Enum.GetName(typeof(AxisPosition), axisY.AxisPosition)
                            };
                            if (axisY.Sections != null)
                            {
                                axisYEntity.Sections = axisY.Sections.Select(s => new SectionEntity
                                {
                                    SectionNum = s.SectionNum,
                                    AxisNum = axisY.AxisNum,
                                    Value = s.Value.ToString(),
                                    Color = s.Color,
                                });
                            }
                            axisYEntities.Add(axisYEntity);
                        }
                        trendEntity.AxisYList = axisYEntities;
                    }

                    if (trend.Series != null)
                    {
                        IList<SeriesEntity> seriesEntities = new List<SeriesEntity>();
                        foreach (var series in trend.Series)
                        {
                            var seriesEntity = new SeriesEntity
                            {
                                TrendNum = trendEntity.TrendNum,
                                AxisNum = series.AxisYNum,
                                DeviceNum = series.TrendDeviceNum,
                                VarNum = series.TrendVariableNum,
                                Title = series.Title,
                                Color = series.Color
                            };
                            seriesEntities.Add(seriesEntity);
                        }
                        trendEntity.Series = seriesEntities;
                    }

                    trendEntities.Add(trendEntity);
                }
            }

            try
            {
                _localDataAccess.SaveTrends(trendEntities);

                VisualStateManager.GoToElementState(owner, "ShowSuccess", true);
            }
            catch (Exception ex)
            {
                FailureMessageOnSaving = ex.Message;
                VisualStateManager.GoToElementState(owner, "ShowFailure", true);
            }
        }

        private void doSaveToImageCommand(object obj)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "Chart" + DateTime.Now.ToString("yyyyMMddHHmmssFFF") + ".png";
            saveFileDialog.CheckPathExists = true;
            if (saveFileDialog.ShowDialog() == true)
            {
                CreateBitmapFromVisual((obj as Visual), saveFileDialog.FileName);
            }
        }

        private void CreateBitmapFromVisual(Visual target, string fileName)
        {
            if (target == null || string.IsNullOrEmpty(fileName)) return;

            Rect bounds = VisualTreeHelper.GetDescendantBounds(target);

            RenderTargetBitmap renderTarget = new RenderTargetBitmap((Int32)bounds.Width, (Int32)bounds.Height, 96, 96, PixelFormats.Pbgra32);

            DrawingVisual visual = new DrawingVisual();

            using (DrawingContext context = visual.RenderOpen())
            {
                VisualBrush visualBrush = new VisualBrush(target);
                context.DrawRectangle(visualBrush, null, new Rect(new Point(), bounds.Size));
            }

            renderTarget.Render(visual);
            PngBitmapEncoder bitmapEncoder = new PngBitmapEncoder();
            bitmapEncoder.Frames.Add(BitmapFrame.Create(renderTarget));
            using (Stream stm = File.Create(fileName))
            {
                bitmapEncoder.Save(stm);
            }
        }
        #endregion

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
                            var section = new TrendSectionInfo(sectionEntity.SectionNum)
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
