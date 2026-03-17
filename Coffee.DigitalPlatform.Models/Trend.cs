using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LiveCharts;
using LiveCharts.Wpf;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;

namespace Coffee.DigitalPlatform.Models
{
    public interface ITrendRender
    {
        void UpdateUI();
    }

    public class TrendChartInfo : ObservableObject, ITrendRender
    {
        public TrendChartInfo()
        {
            ChartNum = shortid.ShortId.Generate(new shortid.Configuration.GenerationOptions(true, true, 18));

            AxisXCollection.CollectionChanged += AxisCollection_CollectionChanged;
            AxisYCollection.CollectionChanged += AxisCollection_CollectionChanged;
            Series.CollectionChanged += Series_CollectionChanged;

            AddAxisXCommand = new RelayCommand(doAddAxisXCommand);
            AddAxisYCommand = new RelayCommand(doAddAxisYCommand);
            RemoveAxisXCommand = new RelayCommand<TrendAxisInfo> (doRemoveAxisXCommand, canRemoveAxisXCommand);
            RemoveAxisYCommand = new RelayCommand<TrendAxisInfo>(doRemoveAxisYCommand, canRemoveAxisYCommand);

            //添加默认的轴
            TrendAxisInfo axixX = new TrendAxisInfo() { IsShowSeperator = true, LabelRotation = 45 };
            AxisXCollection.Add(axixX);

            TrendAxisInfo axixY = new TrendAxisInfo()
            {
                IsShowSeperator = true
            };
            AxisYCollection.Add(axixY);
        }

        private void AxisCollection_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (sender == null || sender is not ObservableCollection<TrendAxisInfo>)
                return;
            AxesCollection rawAxisCollection = null;
            if (sender == AxisXCollection)
                rawAxisCollection = RawAxisXCollection;
            else if (sender == AxisYCollection)
                rawAxisCollection = RawAxisYCollection;
            if (rawAxisCollection == null)
                return;

            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    var axisInfo = item as TrendAxisInfo;
                    if (axisInfo == null)
                        continue;
                    rawAxisCollection.Add(axisInfo.RawAxis);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    var axisInfo = item as TrendAxisInfo;
                    if (axisInfo == null)
                        continue;
                    rawAxisCollection.Remove(axisInfo.RawAxis);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                rawAxisCollection.Clear();
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                foreach (var oldItem in e.OldItems)
                {
                    var axisInfo = oldItem as TrendAxisInfo;
                    if (axisInfo == null)
                        continue;
                    rawAxisCollection.Remove(axisInfo.RawAxis);
                }
                foreach (var newItem in e.NewItems)
                {
                    var axisInfo = newItem as TrendAxisInfo;
                    if (axisInfo == null)
                        continue;
                    rawAxisCollection.Add(axisInfo.RawAxis);
                }
            }
        }

        private void Series_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var item in e.NewItems)
                {
                    var seriesInfo  = item as TrendSeriesInfo;
                    if (seriesInfo == null)
                        continue;
                    RawSeries.Add(seriesInfo.RawSeries);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in e.OldItems)
                {
                    var seriesInfo = item as TrendSeriesInfo;
                    if (seriesInfo == null)
                        continue;
                    RawSeries.Remove(seriesInfo.RawSeries);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                RawSeries.Clear();
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                foreach (var oldItem in e.OldItems)
                {
                    var seriesInfo = oldItem as TrendSeriesInfo;
                    if (seriesInfo == null)
                        continue;
                    RawSeries.Remove(seriesInfo.RawSeries);
                }
                foreach (var newItem in e.NewItems)
                {
                    var seriesInfo = newItem as TrendSeriesInfo;
                    if (seriesInfo == null)
                        continue;
                    RawSeries.Add(seriesInfo.RawSeries);
                }
            }

            BeginRefreshSeries();
        }

        public string ChartNum { get; }

        private bool _isSelected;
        public bool IsSelected
        {
            get { return _isSelected; }
            set { SetProperty(ref _isSelected, value); }
        }

        private string _header = "新建趋势图";
        public string Header
        {
            get { return _header; }
            set { SetProperty(ref _header, value); }
        }

        private bool _isShowLegend;
        public bool IsShowLegend
        {
            get { return _isShowLegend; }
            set
            {
                if (SetProperty(ref _isShowLegend, value))
                {
                    if (value)
                    {
                        _legendLocation = LegendLocation != LegendLocation.None ? LegendLocation : LegendLocation.Top;
                    }
                    else
                    {
                        _legendLocation = LegendLocation.None;
                    }
                }
            }
        }

        private LegendLocation _legendLocation = LegendLocation.None;
        public LegendLocation LegendLocation { get; set; } = LegendLocation.None;

        private ObservableCollection<TrendAxisInfo> _axisXCollection = new ObservableCollection<TrendAxisInfo>();
        public ObservableCollection<TrendAxisInfo> AxisXCollection
        {
            get { return _axisXCollection; }
        }

        private ObservableCollection<TrendAxisInfo> _axisYCollection = new ObservableCollection<TrendAxisInfo>();
        public ObservableCollection<TrendAxisInfo> AxisYCollection
        {
            get { return _axisYCollection; }
        }

        public RelayCommand AddAxisXCommand { get; set; }

        public RelayCommand AddAxisYCommand { get; set; }

        public RelayCommand<TrendAxisInfo> RemoveAxisXCommand { get; set; }

        public RelayCommand<TrendAxisInfo> RemoveAxisYCommand { get; set; }

        private void doAddAxisXCommand()
        {
            var axisX = new TrendAxisInfo();
            AxisXCollection.Add(axisX);
        }

        private void doAddAxisYCommand()
        {
            var axisY = new TrendAxisInfo();
            AxisYCollection.Add(axisY);
        }

        private void doRemoveAxisXCommand(TrendAxisInfo axisInfo)
        {
            if (canRemoveAxisXCommand(axisInfo))
            {
                AxisXCollection.Remove(axisInfo);
            }
        }

        private void doRemoveAxisYCommand(TrendAxisInfo axisInfo)
        {
            if (canRemoveAxisYCommand(axisInfo))
            {
                AxisYCollection.Remove(axisInfo);
            }
        }

        private bool canRemoveAxisXCommand(TrendAxisInfo axisInfo)
        {
            if  (axisInfo != null)
            {
                int idx = AxisXCollection.IndexOf(axisInfo);
                if (idx < 0)
                    return false;
                else return AxisXCollection.Count > 1 ? true : false; //最少保留一个横轴
            }
            else
            {
                return false;
            }
        }

        private bool canRemoveAxisYCommand(TrendAxisInfo axisInfo)
        {
            if (axisInfo != null)
            {
                int idx = AxisYCollection.IndexOf(axisInfo);
                if (idx < 0)
                    return false;
                else return AxisYCollection.Count > 1 ? true : false; //最少保留一个横轴
            }
            else
            {
                return false;
            }
        }

        private ObservableCollection<TrendSeriesInfo> _series = new ObservableCollection<TrendSeriesInfo>();
        public ObservableCollection<TrendSeriesInfo> Series
        {
            get { return _series; }
        }

        public AxesCollection RawAxisYCollection { get; private set; } = new AxesCollection();
        public AxesCollection RawAxisXCollection { get; private set; } = new AxesCollection();
        public SeriesCollection RawSeries { get; private set; } = new SeriesCollection();

        #region Refresh
        public TimeSpan RefreshSpan { get; set; } = TimeSpan.FromSeconds(1);

        CancellationTokenSource cts = new CancellationTokenSource();

        IList<Task> seriesTasks = new List<Task>(); //每个序列一个刷新任务

        public void BeginRefreshSeries()
        {
            if (!cts.TryReset())
            {
                StopRefreshSeries();
                cts = new CancellationTokenSource();
            }

            //根据当前趋势图使用的设备编号，从设备监控模块中获取设备及其点位变量的状态信息
            var deviceNumList = Series.Select(s => s.TrendDeviceNum).Distinct();
            IList<Device> deviceList = new List<Device>();
            WeakReferenceMessenger.Default.Send<Action<Func<IEnumerable<string>, IEnumerable<Device>>>>(func =>
            {
                if (func == null) 
                    return;
                var list = func.Invoke(deviceNumList);
                if (list != null)
                {
                    deviceList = new List<Device>(list);
                }
            });
            Dictionary<TrendSeriesInfo, Variable> seriesVariableDict = new Dictionary<TrendSeriesInfo, Variable>();
            foreach(var series in Series)
            {
                var device = deviceList.FirstOrDefault(d => d.DeviceNum == series.TrendDeviceNum);
                if (device != null)
                {
                    var variable = device.Variables.FirstOrDefault(@var => @var.VarNum == series.TrendVariableNum);
                    if (variable != null)
                    {
                        if (!seriesVariableDict.ContainsKey(series))
                        {
                            seriesVariableDict.Add(series, variable);
                        }
                        else
                        {
                            seriesVariableDict[series] = variable;
                        }
                    }
                }
            }

            //每个设备监听开启一个线程
            foreach (var device in deviceList)
            {
                var task = Task.Run(async () =>
                {
                    while (!cts.IsCancellationRequested)
                    {
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            var axix1 = AxisXCollection.FirstOrDefault();
                            if (axix1 != null)
                            {
                                axix1.AddLabel(DateTime.Now.ToString("HH:mm:ss"));
                                if (axix1.Labels.Count > 30)
                                {
                                    axix1.RemoveLabelAt(0);
                                }
                            }
                        });

                        var seriesList = Series.Where(s => s.TrendDeviceNum == device.DeviceNum).ToList();
                        Dictionary<TrendSeriesInfo, object> seriesValueDict = new Dictionary<TrendSeriesInfo, object>();
                        foreach (var series in seriesList)
                        {
                            if (seriesVariableDict.TryGetValue(series, out var variable))
                            {
                                seriesValueDict.Add(series, variable.FinalValue);
                            }
                        }

                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            foreach(var pair in seriesValueDict)
                            {
                                var series = pair.Key;
                                series.AddValue(pair.Value);
                                if (series.GetValueCount() > 30)
                                {
                                    series.RemoveFirstValue();
                                }
                            }
                        });
                        await Task.Delay(RefreshSpan);
                    }
                });
                seriesTasks.Add(task);
            }
        }

        public void StopRefreshSeries()
        {
            cts.Cancel();
            if (seriesTasks != null)
            {
                Task.WaitAll(seriesTasks.ToArray());
            }
        }

        public void UpdateUI()
        {
            var series = this.RawSeries.FirstOrDefault();
            if (series != null)
            {
                series.Model.Chart.Updater.Run(false, true);
            }
        }
        #endregion
    }

    public class TrendAxisInfo : ObservableObject, ITrendRender
    {
        public TrendAxisInfo(bool hasDefaultMinOrMax = true)
        {
            AddSectionCommand = new RelayCommand(doAddSectionCommand);
            RemoveSectionCommand = new RelayCommand<TrendSectionInfo>(doRemoveSectionCommand, canRemoveSectionCommand);

            _sections.CollectionChanged += _sections_CollectionChanged;
            
            AxisNum = shortid.ShortId.Generate(new shortid.Configuration.GenerationOptions(true, true, 18));
            RawAxis = new Axis()
            {
                LabelFormatter = new Func<double, string>(d => d.ToString("00")),
                Separator = new Separator()
                {
                    Stroke = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245))
                },
                Sections = new SectionsCollection()
            };
            if (hasDefaultMinOrMax)
            {
                RawAxis.MinValue = 0;
                RawAxis.MaxValue = 100;
                RawAxis.Separator = new Separator()
                {
                    Step = 2,
                    Stroke = new SolidColorBrush(Color.FromArgb(255, 245, 245, 245))
                };
            }

            Labels = new List<string>();
            RawAxis.Labels = new List<string>();
        }

        private void _sections_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (RawAxis != null)
            {
                if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    if (RawAxis.Sections == null)
                    {
                        RawAxis.Sections = new SectionsCollection();
                    }
                    foreach(var item in e.NewItems)
                    {
                        var section = item as TrendSectionInfo;
                        if (section == null)
                            continue;
                        section.ValueChanged = () =>
                        {
                            OnPropertyChanged(nameof(SectionValues));
                        };
                        RawAxis.Sections.Add(section.RawSection);
                    }
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    foreach(var item in e.OldItems)
                    {
                        var section = item as TrendSectionInfo;
                        if (section == null)
                            continue;
                        section.ValueChanged = null;
                        if (RawAxis.Sections != null)
                        {
                            RawAxis.Sections.Remove(section.RawSection);
                        }
                    }
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                {
                    if (Sections != null)
                    {
                        foreach (var section in Sections)
                        {
                            section.ValueChanged = null;
                        }
                    }
                    if (RawAxis.Sections != null)
                    {
                        RawAxis.Sections.Clear();
                    }
                }
                else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
                {
                    if (RawAxis.Sections == null)
                    {
                        RawAxis.Sections = new SectionsCollection();
                    }
                    foreach (var oldItem in e.OldItems)
                    {
                        var section = oldItem as TrendSectionInfo;
                        if (section == null)
                            continue;
                        section.ValueChanged = null;
                        RawAxis.Sections.Remove(section.RawSection);
                    }
                    foreach (var newItem in e.NewItems)
                    {
                        var section = newItem as TrendSectionInfo;
                        if (section == null)
                            continue;
                        section.ValueChanged = () =>
                        {
                            OnPropertyChanged(nameof(SectionValues));
                        };
                        RawAxis.Sections.Add(section.RawSection);
                    }
                }
            }

            OnPropertyChanged(nameof(SectionValues));
        }

        public string AxisNum { get; }

        private string _title;
        public string Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private bool _isShowTitle;
        public bool IsShowTitle
        {
            get { return _isShowTitle; }
            set
            {
                if (SetProperty(ref _isShowTitle, value))
                {
                    RawAxis.Title = IsShowTitle ? Title : null;
                }
            }
        }

        private bool _isShowSeperator;
        public bool IsShowSeperator
        {
            get { return _isShowSeperator; }
            set
            {
                if (SetProperty(ref _isShowSeperator, value))
                {
                    if (value)
                    {
                        if (RawAxis.Separator != null)
                            RawAxis.Separator.StrokeThickness = SeperatorThickness;
                        else
                            RawAxis.Separator = new Separator() { StrokeThickness = SeperatorThickness };
                    }
                    else
                    {
                        RawAxis.Separator = null;
                    }
                }
            }
        }

        private double _seperatorThickness = 1;
        public double SeperatorThickness
        {
            get { return _seperatorThickness; }
            set { _seperatorThickness = value; }
        }

        private string _labelFormatter = "00";
        public string LabelFormatter
        {
            get { return _labelFormatter; }
            set
            {
                if (SetProperty(ref _labelFormatter, value))
                {
                    RawAxis.LabelFormatter = new Func<double, string>(d => d.ToString(value));
                }
            }
        }

        private double _labelRotation = 0;
        public double LabelRotation
        {
            get { return _labelRotation; }
            set
            {
                if (SetProperty(ref _labelRotation, value))
                {
                    RawAxis.LabelsRotation = value;
                }
            }
        }

        private AxisPosition _axisPosition = AxisPosition.LeftBottom;
        public AxisPosition AxisPosition
        {
            get { return _axisPosition; }
            set
            {
                if (SetProperty(ref _axisPosition, value))
                {
                    RawAxis.Position = value;
                }
            }
        }

        private double _minimum = 0;
        public double Minimum
        {
            get { return _minimum; }
            set
            {
                if (SetProperty(ref _minimum, value))
                {
                    RawAxis.MinValue = value;
                }
            }
        }

        private double _maximum = 100;
        public double Maximum
        {
            get { return _maximum; }
            set
            {
                if (SetProperty(ref _maximum, value))
                {
                    RawAxis.MaxValue = value;
                }
            }
        }

        public string SectionValues
        {
            get
            {
                if (Sections == null || !Sections.Any())
                    return "<未配置>";
                return string.Join(",", Sections.Select(s => s.Value));
            }
        }

        public IList<string> Labels { get; }

        public void AddLabel(string label)
        {
            RawAxis.Labels.Add(label);
        }

        public void InsertLabel(int index, string label)
        {
            RawAxis.Labels.Insert(index, label);
        }

        public void RemoveLabel(string label)
        {
            RawAxis.Labels.Remove(label);
        }

        public void RemoveLabelAt(int index)
        {
            RawAxis.Labels.RemoveAt(index);
        }

        private ObservableCollection<TrendSectionInfo> _sections = new ObservableCollection<TrendSectionInfo>();
        public ObservableCollection<TrendSectionInfo> Sections
        {
            get { return _sections; }
        }

        public RelayCommand AddSectionCommand { get; set; }

        public RelayCommand<TrendSectionInfo> RemoveSectionCommand { get; set; }

        private void doAddSectionCommand()
        {
            var sectionInfo = new TrendSectionInfo();
            Sections.Add(sectionInfo);
        }

        private void doRemoveSectionCommand(TrendSectionInfo sectionInfo)
        {
            if (sectionInfo != null)
            {
                Sections.Remove(sectionInfo);
            }
        }

        private bool canRemoveSectionCommand(TrendSectionInfo sectionInfo)
        {
            return sectionInfo != null;
        }

        internal Axis RawAxis { get; }

        public void UpdateUI()
        {
            RawAxis.Model.Chart.Updater.Run(false, true);
        }
    }

    public class TrendSectionInfo : ObservableObject, ITrendRender
    {
        public TrendSectionInfo()
        {
            BrushNames = typeof(Brushes).GetProperties().Select(p => p.Name).ToList();

            RawSection = new AxisSection()
            {
                Value = 0,
                Stroke = Brushes.Red,
                StrokeThickness = 1,
                StrokeDashArray = new DoubleCollection { 5, 5 }
            };
        }

        private double _value;
        public double Value
        {
            get { return _value; }
            set
            {
                if (SetProperty(ref _value, value))
                {
                    if (RawSection != null)
                    {
                        RawSection.Value = value;
                    }

                    ValueChanged?.Invoke();
                }
            }
        }

        private string _color;
        public string Color
        {
            get { return _color; }
            set
            {
                if (SetProperty(ref _color, value))
                {
                    try
                    {
                        Color c = (Color)ColorConverter.ConvertFromString(value);
                        RawSection.Stroke = new SolidColorBrush(c);

                        ValueChanged?.Invoke();
                    }
                    catch
                    {
                        RawSection.Stroke = null;
                    }
                }
            }
        }

        public IList<string> BrushNames { get; }

        public Action ValueChanged { get; set; }

        internal AxisSection RawSection { get; }

        public void UpdateUI()
        {
            RawSection.Model.Chart.Updater.Run(false, true);
        }
    }

    public abstract class TrendSeriesInfo : ObservableObject, ITrendRender
    {
        private object lockObj = new object();

        public string TrendDeviceNum { get; set; }

        public string TrendVariableNum { get; set; }

        private string _title;
        public string Title
        {
            get { return _title; }
            set 
            { 
                if (SetProperty(ref _title, value))
                {
                    if (RawSeries != null)
                        RawSeries.Title = value;
                }
            }
        }

        private string _color;
        public string Color
        {
            get { return _color; }
            set
            {
                if (SetProperty(ref _color, value))
                {
                    if (RawSeries != null)
                    {
                        try
                        {
                            Color c = (Color)ColorConverter.ConvertFromString(value);
                            RawSeries.Stroke = new SolidColorBrush(c);
                        }
                        catch
                        {
                            RawSeries.Stroke = null;
                        }
                    }
                }
            }
        }

        private string _axisXNum;
        public string AxisXNum
        {
            get { return _axisXNum; }
            set
            {
                if (SetProperty(ref _axisXNum, value))
                {
                    if (FindAxisXIndex != null)
                    {
                        int index_axisX = FindAxisXIndex.Invoke(value);
                        if (index_axisX >= 0)
                        {
                            RawSeries.ScalesXAt = index_axisX;
                        }
                    }
                }
            }
        }

        private string _axisYNum;
        public string AxisYNum
        {
            get { return _axisYNum; }
            set
            {
                if (SetProperty(ref _axisYNum, value))
                {
                    if (FindAxisYIndex != null)
                    {
                        int index_axisY = FindAxisYIndex.Invoke(value);
                        if (index_axisY >= 0)
                        {
                            RawSeries.ScalesYAt = index_axisY;
                        }
                    }
                }
            }
        }

        public void AddValue(object value)
        {
            RawSeries.Values.Add(value);
        }

        public bool RemoveValueAt(int index)
        {
            lock (lockObj)
            {
                if (index < 0 || index >= RawSeries.Values.Count)
                    return false;
                RawSeries.Values.RemoveAt(index);
                return true;
            }
        }

        public bool RemoveFirstValue()
        {
            return RemoveValueAt(0);
        }

        public bool RemoveLastValue()
        {
            int lastIndex = RawSeries.Values.Count - 1;
            return RemoveValueAt(lastIndex);
        }

        public int GetValueCount()
        {
            return RawSeries.Values.Count;
        }

        public Func<string, int> FindAxisXIndex { get; set; }

        public Func<string, int> FindAxisYIndex { get; set; }

        public Action ValueChanged { get; set; }

        internal abstract Series RawSeries { get; }

        public void UpdateUI()
        {
            RawSeries.Model.Chart.Updater.Run(false, false);
        }
    }

    public class TrendLineSeriesInfo<T> : TrendSeriesInfo
    {
        public TrendLineSeriesInfo()
        {
            _rawSeries = new LineSeries()
            {
                Values = new ChartValues<T>(),
                Fill = Brushes.Transparent,
                Stroke = Brushes.Black,
                StrokeThickness = 2,
            };
        }

        private readonly LineSeries _rawSeries;

        internal override Series RawSeries
        {
            get { return _rawSeries; }
        }

        public Type ValueType
        {
            get
            {
                return typeof(T);
            }
        }
    }
}
