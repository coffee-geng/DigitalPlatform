using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using LiveCharts;
using LiveCharts.Defaults;
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
            initConstructor();

            //添加默认的轴
            AxisX = new TrendAxisInfo() { IsShowSeperator = true, LabelRotation = 45 };

            TrendAxisInfo axixY = new TrendAxisInfo()
            {
                IsShowSeperator = true
            };
            AxisYCollection.Add(axixY);
        }

        //从数据库加载的数据
        public TrendChartInfo(string chartNum)
        {
            if (string.IsNullOrWhiteSpace(chartNum)) 
                throw new ArgumentNullException(nameof(chartNum));
            ChartNum = chartNum;
            initConstructor();
        }

        private void initConstructor()
        {
            AxisYCollection.CollectionChanged += AxisCollection_CollectionChanged;
            Series.CollectionChanged += Series_CollectionChanged;

            AddAxisYCommand = new RelayCommand(doAddAxisYCommand);
            RemoveAxisYCommand = new RelayCommand<TrendAxisInfo>(doRemoveAxisYCommand, canRemoveAxisYCommand);
        }

        //确保当前趋势图至少有一个X轴和Y轴
        public void EnsureAxis()
        {
            if (AxisX == null)
            {
                //添加默认的x轴
                AxisX = new TrendAxisInfo() 
                { 
                    Minimum = 0,
                    Maximum = 20,
                    IsShowSeperator = true,
                    LabelRotation = 45,
                };
            }
            if (AxisYCollection == null || !AxisYCollection.Any())
            {
                //添加默认的y轴
                TrendAxisInfo axixY = new TrendAxisInfo()
                {
                    IsShowSeperator = true
                };
                AxisYCollection.Add(axixY);
            }
        }

        private void AxisCollection_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (sender == null || sender is not ObservableCollection<TrendAxisInfo>)
                return;
            AxesCollection rawAxisCollection = RawAxisYCollection;

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

        private TrendAxisInfo _axisX;
        public TrendAxisInfo AxisX 
        {
            get { return _axisX; }
            set
            {
                if (value == null || value.RawAxis == null)
                    throw new NullReferenceException($"趋势图不能没有X轴！");
                if (SetProperty(ref _axisX, value))
                {
                    RawAxisXCollection.Clear();
                    RawAxisXCollection.Add(value.RawAxis);
                }
            }
        }

        private ObservableCollection<TrendAxisInfo> _axisYCollection = new ObservableCollection<TrendAxisInfo>();
        public ObservableCollection<TrendAxisInfo> AxisYCollection
        {
            get { return _axisYCollection; }
        }

        public RelayCommand AddAxisYCommand { get; set; }

        public RelayCommand<TrendAxisInfo> RemoveAxisYCommand { get; set; }

        private void doAddAxisYCommand()
        {
            var axisY = new TrendAxisInfo();
            AxisYCollection.Add(axisY);
        }

        private void doRemoveAxisYCommand(TrendAxisInfo axisInfo)
        {
            if (canRemoveAxisYCommand(axisInfo))
            {
                AxisYCollection.Remove(axisInfo);
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

        private Dictionary<string, Tuple<Task, CancellationTokenSource>> _seriesTaskDict = new Dictionary<string, Tuple<Task, CancellationTokenSource>>(); //每个关联趋势图序列的设备一个刷新任务

        //多线程共享变量，字典保存当前正在使用的序列及其关联的点位信息
        private Dictionary<TrendSeriesInfo, Variable> _seriesVariableDict = new Dictionary<TrendSeriesInfo, Variable>();

        private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();

        public void BeginRefreshSeries()
        {
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
            
            if (_lock.TryEnterWriteLock(2000))
            {
                try
                {
                    _seriesVariableDict = seriesVariableDict;
                }
                finally
                {
                    _lock.ExitWriteLock();
                }
            }


            //每个设备监听开启一个线程
            foreach (var device in deviceList)
            {
                var seriesList = Series.Where(s => s.TrendDeviceNum == device.DeviceNum).ToList();
                bool hasSeriesOnDevice = false;
                foreach (var series in seriesList)
                {
                    if (seriesVariableDict.ContainsKey(series))
                    {
                        hasSeriesOnDevice = true;
                        break;
                    }
                }
                //只有当设备与选中的趋势图序列有关联时，才启动监听线程
                if (!hasSeriesOnDevice)
                    continue;

                if (!_seriesTaskDict.ContainsKey(device.DeviceNum))
                {
                    var cts = new CancellationTokenSource();
                    var token = cts.Token;
                    var task = Task.Run(async () =>
                    {
                        
                        while (true)
                        {
                            //if (!_seriesTaskDict.TryGetValue(device.DeviceNum, out Tuple<Task, CancellationTokenSource> pair))
                            //    break;
                            //var cts = pair.Item2;
                            //if (cts.IsCancellationRequested)
                            //    break;
                            if (token.IsCancellationRequested)
                                break;

                            Dictionary<TrendSeriesInfo, object> seriesValueDict = new Dictionary<TrendSeriesInfo, object>();
                            if (_lock.TryEnterReadLock(2000))
                            {
                                try
                                {
                                    var seriesList = Series.Where(s => s.TrendDeviceNum == device.DeviceNum).ToList();
                                    foreach (var series in seriesList)
                                    {
                                        if (_seriesVariableDict.TryGetValue(series, out var variable))
                                        {
                                            seriesValueDict.Add(series, variable.FinalValue);
                                        }
                                    }
                                }
                                finally
                                {
                                    _lock.ExitReadLock();
                                }
                            }
                            else
                            {
                                continue;
                            }

                            var axisX = AxisX;
                            int minX = (int)Math.Ceiling(axisX.Minimum);
                            int maxX = (int)Math.Ceiling(axisX.Maximum);

                            await Application.Current.Dispatcher.InvokeAsync(() =>
                            {
                                int labelCount = axisX.GetLableCount();
                                if (labelCount < maxX - minX)
                                {
                                    axisX.AddLabel(DateTime.Now.ToString("HH:mm:ss"));
                                }
                                else
                                {
                                    for (int i = 0; i < labelCount; i++)
                                    {
                                        if (i < labelCount - 1)
                                        {
                                            axisX.UpdateLabelAt(i, axisX.GetLabelAt(i + 1));
                                        }
                                        else
                                        {
                                            axisX.UpdateLabelAt(i, DateTime.Now.ToString("HH:mm:ss"));
                                        }
                                    }
                                }
                            });

                            await Application.Current.Dispatcher.InvokeAsync(() =>
                            {
                                foreach (var pair in seriesValueDict)
                                {
                                    var series = pair.Key;
                                    if (pair.Value == null)
                                        continue;
                                    int deltaX = series.GetValueCount();
                                    series.AddValue(pair.Value, minX + deltaX);
                                    if (minX + deltaX > maxX)
                                    {
                                        series.RemoveFirstValue();
                                        series.RefreshValuePositions(-1, SeriesValuePostionRefreshTypes.Range, 0, series.GetValueCount());
                                    }
                                }
                            });
                            await Task.Delay(RefreshSpan);
                        }
                    }, cts.Token);
                    _seriesTaskDict.Add(device.DeviceNum, new Tuple<Task, CancellationTokenSource>(task, cts));
                }
            }

            var keysInDict = _seriesTaskDict.Keys.ToList();
            var deviceNames = deviceList.Select(d => d.DeviceNum).ToList();
            var removingTaskList = new List<Tuple<Task, string>>();
            foreach (var key in keysInDict)
            {
                if (deviceNames.Contains(key))
                    continue;
                if (!_seriesTaskDict.TryGetValue(key, out Tuple<Task, CancellationTokenSource> tuple))
                    continue;
                var cts = tuple.Item2;
                cts.Cancel();
                removingTaskList.Add(new Tuple<Task, string>(tuple.Item1, key));
            }

            Task.WaitAll(removingTaskList.Select(t=>t.Item1).ToArray());
            foreach (var item in removingTaskList)
            {
                _seriesTaskDict.Remove(item.Item2);
            }
        }

        public void StopRefreshSeries()
        {
            if (_seriesTaskDict != null && _seriesTaskDict.Any())
            {
                var removingTaskList = new List<Task>();
                foreach (var item in _seriesTaskDict)
                {
                    var cts = item.Value.Item2 as CancellationTokenSource;
                    var task = item.Value.Item1 as Task;
                    removingTaskList.Add(task);
                    cts.Cancel();
                }

                Task.WaitAll(removingTaskList.ToArray());
                _seriesTaskDict.Clear();
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
            AxisNum = shortid.ShortId.Generate(new shortid.Configuration.GenerationOptions(true, true, 18));

            initConstructor(hasDefaultMinOrMax);
        }

        public TrendAxisInfo(string axisNum, bool hasDefaultMinOrMax = true)
        {
            if (string.IsNullOrWhiteSpace(axisNum))
                throw new ArgumentNullException(nameof(axisNum));
            AxisNum = axisNum;

            initConstructor(hasDefaultMinOrMax);
        }

        private void initConstructor(bool hasDefaultMinOrMax)
        {
            AddSectionCommand = new RelayCommand(doAddSectionCommand);
            RemoveSectionCommand = new RelayCommand<TrendSectionInfo>(doRemoveSectionCommand, canRemoveSectionCommand);

            _sections.CollectionChanged += _sections_CollectionChanged;

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

        public IReadOnlyCollection<string> Labels
        {
            get { return new ReadOnlyCollection<string>(RawAxis.Labels); }
        }

        public string GetLabelAt(int index)
        {
            if (index < 0 || index >= RawAxis.Labels.Count)
                return null;
            return RawAxis.Labels[index];
        }

        public int GetLableCount()
        {
            return RawAxis.Labels.Count;
        }

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

        public void UpdateLabelAt(int index, string newLabel)
        {
            if (index < 0 || index >= RawAxis.Labels.Count)
                return;
            RawAxis.Labels[index] = newLabel;
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

        internal Axis RawAxis { get; private set; }

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

        // 是否根据用户需要沿着X轴定位序列值
        protected TrendSeriesInfo(bool isPositionOnAxisX = false)
        {
            _isPositionOnAxisX = isPositionOnAxisX;   
        }

        protected bool _isPositionOnAxisX;

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

        public virtual void AddValue(object value, double? valuePosX = null)
        {
            if (_isPositionOnAxisX)
            {
                if (valuePosX.HasValue)
                {
                    AddValue(valuePosX.Value, value);
                }
                else
                {
                    throw new Exception("自定义X轴坐标模式下必须指定当前值在X轴的坐标！");
                }
            }
            else
            {
                RawSeries.Values.Add(value);
            }
        }

        protected virtual void AddValue(double valueX, object valueY)
        {
            double dVal = 0.0;
            if (valueY != null)
            {
                if (valueY.GetType() == typeof(bool))
                {
                    dVal = (bool)valueY ? 1.0 : 0.0;
                }
                else if (valueY.GetType() == typeof(bool?))
                {
                    dVal = ((valueY as bool?).HasValue && (valueY as bool?).Value) ? 1.0 : 0.0;
                }
                else if (valueY is double)
                {
                    dVal = (double)valueY;
                }
                else
                {
                    if (double.TryParse(valueY.ToString(), out double d))
                    {
                        dVal = d;
                    }
                }
            }

            RawSeries.Values.Add(new LiveCharts.Defaults.ObservablePoint(valueX, dVal));
        }

        public virtual bool RemoveValueAt(int index)
        {
            lock (lockObj)
            {
                if (index < 0 || index >= RawSeries.Values.Count)
                    return false;
                RawSeries.Values.RemoveAt(index);
                return true;
            }
        }

        public virtual bool RemoveFirstValue()
        {
            return RemoveValueAt(0);
        }

        public virtual bool RemoveLastValue()
        {
            int lastIndex = RawSeries.Values.Count - 1;
            return RemoveValueAt(lastIndex);
        }

        public virtual int GetValueCount()
        {
            return RawSeries.Values.Count;
        }

        public virtual void RefreshValuePositions(double deltaX, SeriesValuePostionRefreshTypes posRefreshType = SeriesValuePostionRefreshTypes.All, int start = 0, int length = 1)
        {
            if (!_isPositionOnAxisX)
                return;
            if (posRefreshType == SeriesValuePostionRefreshTypes.Range)
            {
                for(int i = 0; i < RawSeries.Values.Count; i++)
                {
                    if (i < start || i >= start + length)
                        continue;
                    if (RawSeries.Values[i] is ObservablePoint point)
                    {
                        point.X += deltaX;
                    }
                }
            }
            else
            {
                foreach (var item in RawSeries.Values)
                {
                    if (item is ObservablePoint point)
                    {
                        point.X += deltaX;
                    }
                }
            }
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
        public TrendLineSeriesInfo(bool isPositionOnAxisX = false) : base(isPositionOnAxisX)
        {
            _rawSeries = new LineSeries()
            {
                Values = isPositionOnAxisX ? new ChartValues<ObservableObject>() : new ChartValues<T>(),
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

    public class TrendStepLineSeriesInfo<T> : TrendSeriesInfo
    {
        public TrendStepLineSeriesInfo(bool isPositionOnAxisX = false) : base(isPositionOnAxisX)
        {
            _rawSeries = new StepLineSeries()
            {
                Values = isPositionOnAxisX ? new ChartValues<ObservableObject>() : new ChartValues<double>(),
                Fill = Brushes.Transparent,
                Stroke = Brushes.Black,
                StrokeThickness = 2,
                PointGeometrySize = 8,
                PointGeometry = DefaultGeometries.Diamond
            };
        }

        private readonly StepLineSeries _rawSeries;

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

        public override void AddValue(object value, double? valuePosX = null)
        {
            double dVal = 0.0;
            if (ValueType == typeof(bool))
            {
                dVal = (bool)value ? 1.0 : 0.0;
            }
            else if (ValueType == typeof(bool?))
            {
                dVal = value != null ? (((value as bool?).HasValue && (value as bool?).Value) ? 1.0 : 0.0) : 0.0;
            }
            else if (value is double)
            {
                dVal = (double)value;
            }
            else
            {
                if (value != null && double.TryParse(value.ToString(), out double d))
                {
                    dVal = d;
                }
            }
            base.AddValue(dVal);
        }
    }

    public enum SeriesValuePostionRefreshTypes
    {
        All,
        Range
    }
}
