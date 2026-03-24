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
            DataGridExtension.GetColumns(d).CollectionChanged += (se, ev) =>
            {
                dataGrid = (d as DataGrid);

                if (ev.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                {
                    foreach(var newItem in ev.NewItems)
                    {
                        if (newItem == null || newItem is not DataGridColumn)
                            continue;
                        dataGrid.Columns.Add(newItem as DataGridColumn);
                    }
                }
                else if (ev.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove)
                {
                    foreach (var oldItem in ev.OldItems)
                    {
                        if (oldItem == null || oldItem is not DataGridColumn)
                            continue;
                        dataGrid.Columns.Remove(oldItem as DataGridColumn);
                    }
                }
                else if (ev.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
                {
                    dataGrid.Columns.Clear();
                }
                else if (ev.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Replace)
                {
                    foreach (var oldItem in ev.OldItems)
                    {
                        if (oldItem == null || oldItem is not DataGridColumn)
                            continue;
                        dataGrid.Columns.Remove(oldItem as DataGridColumn);
                    }
                    foreach (var newItem in ev.NewItems)
                    {
                        if (newItem == null || newItem is not DataGridColumn)
                            continue;
                        dataGrid.Columns.Add(newItem as DataGridColumn);
                    }
                }
            };

            foreach (var item in DataGridExtension.GetColumns(d))
            {
                dataGrid.Columns.Add(item);
            }
        }
    }
}
