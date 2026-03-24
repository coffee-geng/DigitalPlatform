using Coffee.DigitalPlatform.CommWPF;
using Coffee.DigitalPlatform.Entities;
using Coffee.DigitalPlatform.IDataAccess;
using Coffee.DigitalPlatform.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Win32;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Coffee.DigitalPlatform.ViewModels
{
    public class ReportViewModel : ObservableObject, INavigationService, IDisposable
    {
        ILocalDataAccess _localDataAccess;

        public ReportViewModel(ILocalDataAccess localDataAccess)
        {
            _localDataAccess = localDataAccess;

            // 初始化列
            AllColumms.Add(new ReportColumn { ColumnName = "设备编号", ColumnToField = "DeviceNum", ColumnWidth = 70 });
            AllColumms.Add(new ReportColumn { ColumnName = "设备名称", ColumnToField = "DeviceName", ColumnWidth = 90, IsChecked = true });
            AllColumms.Add(new ReportColumn { ColumnName = "变量编号", ColumnToField = "VariableNum", ColumnWidth = 70 });
            AllColumms.Add(new ReportColumn { ColumnName = "变量名称", ColumnToField = "VariableName", ColumnWidth = 90, IsChecked = true });
            AllColumms.Add(new ReportColumn { ColumnName = "最新值", ColumnToField = "LastValue", ColumnWidth = 90 });
            AllColumms.Add(new ReportColumn { ColumnName = "平均值", ColumnToField = "AvgValue", ColumnWidth = 90 });
            AllColumms.Add(new ReportColumn { ColumnName = "最大值", ColumnToField = "MaxValue", ColumnWidth = 90 });
            AllColumms.Add(new ReportColumn { ColumnName = "最小值", ColumnToField = "MinValue", ColumnWidth = 90 });
            AllColumms.Add(new ReportColumn { ColumnName = "报警触发次数", ColumnToField = "AlarmCount", ColumnWidth = 90 });
            AllColumms.Add(new ReportColumn { ColumnName = "联控触发次数", ColumnToField = "UnionCount", ColumnWidth = 90 });
            AllColumms.Add(new ReportColumn { ColumnName = "记录次数", ColumnToField = "RecordCount", ColumnWidth = 80 });
            AllColumms.Add(new ReportColumn { ColumnName = "最后记录时间", ColumnToField = "LastTime", ColumnWidth = 120 });

            ChooseColumnCommand = new RelayCommand<ReportColumn>(doChooseColumnCommand);
            RefreshCommand = new RelayCommand(doRefreshCommand);
            ExportCommand = new RelayCommand(doExportCommand);

            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            _persistTask = Task.Factory.StartNew(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    writeRecords();

                    await Task.Delay(1000);
                }
            }, token);

            WeakReferenceMessenger.Default.Register<ReportRecordsMessage>(this, new MessageHandler<object, ReportRecordsMessage>((s, msg) =>
            {
                if (msg.Value == null || !msg.Value.Any())
                    return;
                foreach (var record in msg.Value)
                {
                    _reportRecordQueue.Enqueue(record);
                }
            }));

            // 添加默认列
            foreach (var item in AllColumms)
            {
                doChooseColumnCommand(item);
            }
        }

        #region Write records
        private ConcurrentQueue<RecordItem> _reportRecordQueue = new ConcurrentQueue<RecordItem>(); //暂存要写入数据库的报表原始数据

        private Task _persistTask; //该任务定时读取写入队列，进行报表数据的持久化工作

        private CancellationTokenSource _cts;

        //从报表原始数据队列中读取并写入数据库
        private void writeRecords()
        {
            int maxCount = 100; //一次最多写100条
            int count = 0;
            IList<RecordEntity> recordEntities = new List<RecordEntity>();
            while (count < maxCount)
            {
                if (_reportRecordQueue.TryDequeue(out RecordItem item))
                {
                    recordEntities.Add(new RecordEntity
                    {
                        DeviceNum = item.DeviceNum,
                        VariableNum = item.VariableNum,
                        DeviceName = item.DeviceName,
                        VariableName = item.VariableName,
                        RecordValue = item.RecordValue,
                        State = Enum.GetName(typeof(RecordStatus), item.State),
                        AlarmNum = item.AlarmNum,
                        LinkageNum = item.LinkageNum,
                        RecordTime = DateTime.TryParseExact(item.RecordTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime recordTime) ? recordTime : DateTime.Now,
                        UserName = item.UserName
                    });
                }
                else
                {
                    break;
                }
                count++;
            }

            _localDataAccess.WriteRecords(recordEntities);
        }

        //从数据库中读取最新的报表原始数据。即同一条记录（相同设备相同点位信息）只读取最后一次更改状态的记录
        private IEnumerable<RecordItem> readRecentRecords()
        {
            var recordEntities = _localDataAccess.ReadRecentRecords();
            if (recordEntities != null && recordEntities.Any())
            {
                return recordEntities.Select(r => new RecordItem
                {
                    DeviceNum = r.DeviceNum,
                    VariableNum = r.VariableNum,
                    DeviceName = r.DeviceName,
                    VariableName = r.VariableName,
                    RecordValue = r.RecordValue,
                    State = Enum.TryParse<RecordStatus>(r.State, out RecordStatus recordStatus) ? recordStatus : RecordStatus.Normal,
                    AlarmNum = r.AlarmNum,
                    LinkageNum = r.LinkageNum,
                    RecordTime = r.RecordTime.ToString("yyyy-MM-dd HH:mm:ss"),
                    UserName = r.UserName
                });
            }
            else
            {
                return Enumerable.Empty<RecordItem>();
            }
        }
        #endregion

        private ObservableCollection<ReportItem> _allDatas = new ObservableCollection<ReportItem>();
        public ObservableCollection<ReportItem> AllDatas
        {
            get { return _allDatas; }
            set { SetProperty(ref _allDatas, value); }
        }

        // 所有可能显示的列，可供选择的列
        public ObservableCollection<ReportColumn> AllColumms { get; set; } =
            new ObservableCollection<ReportColumn>();

        public ObservableCollection<DataGridColumn> Columns { get; set; } =
            new ObservableCollection<DataGridColumn>();

        public RelayCommand<ReportColumn> ChooseColumnCommand { get; set; }

        public RelayCommand RefreshCommand { get; set; }

        public RelayCommand ExportCommand { get; set; }

        private void doChooseColumnCommand(ReportColumn model)
        {
            if (model.IsChecked)
            {
                Style style = new System.Windows.Style(typeof(TextBlock));
                style.Setters.Add(new Setter(TextBlock.TextAlignmentProperty, TextAlignment.Center));
                style.Setters.Add(new Setter(TextBlock.VerticalAlignmentProperty, VerticalAlignment.Center));
                Columns.Add(new DataGridTextColumn
                {
                    Header = model.ColumnName,
                    Binding = new Binding(model.ColumnToField),
                    MinWidth = model.ColumnWidth,
                    ElementStyle = style
                });
            }
            else
            {
                var column = Columns.FirstOrDefault(c => string.Equals(c.Header.ToString(), model.ColumnName));
                Columns.Remove(column);
            }
        }

        private void doRefreshCommand()
        {
            // 从数据库来的
            var datas = _localDataAccess.ReadReports();
            if (datas != null)
            {
                AllDatas = new ObservableCollection<ReportItem>(datas.Select(d => new ReportItem
                {
                    DeviceNum = d.DeviceNum,
                    VariableNum = d.VariableNum,
                    DeviceName = d.DeviceName,
                    VariableName = d.VariableName,
                    LastValue = d.LastValue,
                    AvageValue = d.AvageValue,
                    MinValue = d.MinValue,
                    MaxValue = d.MaxValue,
                    TotalCount = d.TotalCount,
                    AlarmCount = d.AlarmCount,
                    LinkageCount = d.LinkageCount,
                    LastTime = d.LastTime
                }));
            }
        }

        private void doExportCommand()
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "Report-" + DateTime.Now.ToString("yyyyMMddHHmmssFFF") + ".csv";
            if (saveFileDialog.ShowDialog() == true)
            {
                // 灵活配置的列，列没有顺序
                //List<ReportColumn> cs = AllColumms.Where(c => c.IsChecked).OrderBy(c => c.Index).ToList();
                List<ReportColumn> activeColumns = AllColumms.Where(c => c.IsChecked).ToList();
                // 列头
                string datas = string.Join(",", activeColumns.Select(c => c.ColumnName)) + "\r\n";

                // 数据
                // 遍历所有数据行
                foreach (var item in AllDatas)
                {
                    // 遍历所有数据列
                    foreach (var col in activeColumns)
                    {
                        // 确定当前列绑定的属性名称
                        // 需求：根据字符串名称获取对应名称的属性值
                        PropertyInfo pi = item.GetType().GetProperty(col.ColumnToField, BindingFlags.Instance | BindingFlags.Public);
                        var val = pi.GetValue(item);
                        datas += val == null ? "," : val.ToString() + ",";
                    }
                    datas.Remove(datas.Length - 1);
                    datas += "\r\n";
                }

                System.IO.File.WriteAllText(saveFileDialog.FileName, datas, Encoding.UTF8);
            }
        }

        public void OnNavigateTo(NavigationContext context = null)
        {
            if (this.RefreshCommand != null)
            {
                RefreshCommand.Execute(context);
            }
        }

        public void OnNavigateFrom(NavigationContext context = null)
        {
            
        }

        public void Dispose()
        {
            if (_persistTask != null && _cts != null)
            {
                _cts.Cancel();

                Task.WaitAll(_persistTask);

                _cts.Dispose();
                _persistTask.Dispose();
            }
        }
    }
}
