using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace Coffee.DigitalPlatform.CommWPF
{
    public class DataGridExtension
    {
        //字典保存附加属性依赖的对象及其上下文，其中键是某附加属性依赖的依赖对象，值是一组于这个依赖对象关联的上下文，可以是任何数据，用键值对表示
        private static Dictionary<object, Dictionary<string, object>> _dependencyContextDict = new Dictionary<object, Dictionary<string, object>>();

        public static ObservableCollection<DataGridColumn> GetColumns(DependencyObject obj)
        {
            return (ObservableCollection<DataGridColumn>)obj.GetValue(ColumnsProperty);
        }

        public static void SetColumns(DependencyObject obj, ObservableCollection<DataGridColumn> value)
        {
            obj.SetValue(ColumnsProperty, value);
        }

        public static readonly DependencyProperty ColumnsProperty =
            DependencyProperty.RegisterAttached("Columns", typeof(ObservableCollection<DataGridColumn>), typeof(DataGridExtension), new PropertyMetadata(null, new PropertyChangedCallback(OnPropertyChanged)));

        private static void OnPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            DataGrid dataGrid = (d as DataGrid);
            ObservableCollection<DataGridColumn> datagridColumns = DataGridExtension.GetColumns(d);
            if (datagridColumns == null)
            {
                var oldDatagridColumns = _dependencyContextDict.Keys.Where(k => k is ObservableCollection<DataGridColumn>).Cast<ObservableCollection<DataGridColumn>>().FirstOrDefault();
                if (oldDatagridColumns != null)
                {
                    oldDatagridColumns.CollectionChanged -= DatagridColumns_CollectionChanged;
                    _dependencyContextDict.Remove(oldDatagridColumns);
                }
                return;
            }
            else
            {
                datagridColumns.CollectionChanged += DatagridColumns_CollectionChanged;
                //这个字典的键是附加属性依赖的对象，一个键是一种类型的依赖对象，也就是说如果有多个相同类型的依赖对象要存入字典，则只存第一个
                if (!_dependencyContextDict.Keys.Any(k => k is ObservableCollection<DataGridColumn>))
                {
                    _dependencyContextDict.Add(datagridColumns, new Dictionary<string, object>()
                    {
                        { "dataGrid", dataGrid }
                    });
                }
            }

            foreach (var item in DataGridExtension.GetColumns(d))
            {
                dataGrid.Columns.Add(item);
            }
        }

        private static void DatagridColumns_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (!_dependencyContextDict.TryGetValue(sender, out Dictionary<string, object> dict))
                return;
            if (!dict.TryGetValue("dataGrid", out object instance1) || instance1 is not DataGrid)
                return;

            DataGrid dataGrid = instance1 as DataGrid;
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
            {
                foreach (var newItem in e.NewItems)
                {
                    if (newItem == null || newItem is not DataGridColumn)
                        continue;
                    dataGrid.Columns.Add(newItem as DataGridColumn);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
            {
                foreach (var oldItem in e.OldItems)
                {
                    if (oldItem == null || oldItem is not DataGridColumn)
                        continue;
                    dataGrid.Columns.Remove(oldItem as DataGridColumn);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                dataGrid.Columns.Clear();
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
            {
                foreach (var oldItem in e.OldItems)
                {
                    if (oldItem == null || oldItem is not DataGridColumn)
                        continue;
                    dataGrid.Columns.Remove(oldItem as DataGridColumn);
                }
                foreach (var newItem in e.NewItems)
                {
                    if (newItem == null || newItem is not DataGridColumn)
                        continue;
                    dataGrid.Columns.Add(newItem as DataGridColumn);
                }
            }
        }
    }
}
