using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Models
{
    public class ReportColumn : ObservableObject
    {
        private bool _isChecked;
        public bool IsChecked
        {
            get { return _isChecked; }
            set { SetProperty(ref _isChecked, value); }
        }

        public int ColumnIndex { get; set; }

        private string _columnName;
        public string ColumnName
        {
            get { return _columnName; }
            set { SetProperty(ref _columnName, value); }
        }

        private double _columnWidth;
        public double ColumnWidth
        {
            get { return _columnWidth; }
            set { SetProperty(ref _columnWidth, value); }
        }

        private string _columnToField;
        public string ColumnToField
        {
            get { return _columnToField; }
            set { SetProperty(ref _columnToField, value); }
        }
    }
}
