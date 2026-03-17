using Coffee.DigitalPlatform.Models;
using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Coffee.DigitalPlatform.ViewModels
{
    public class ConfigAxisDialogViewModel : ObservableObject
    {
        public ConfigAxisDialogViewModel(TrendChartInfo currentTrend)
        {
            BrushNames = typeof(Brushes).GetProperties().Select(p => p.Name).ToList();
            var posArray = Enum.GetValues(typeof(LiveCharts.AxisPosition));
            var positions = new List<AxixPosition>();
            foreach (var pos in posArray)
            {
                string posName = "";
                switch (pos)
                {
                    case LiveCharts.AxisPosition.LeftBottom:
                        posName = "左下";
                        break;
                    case LiveCharts.AxisPosition.RightTop:
                        posName = "右上";
                        break;
                }
                positions.Add(new AxixPosition((LiveCharts.AxisPosition)pos, posName));
            }
            AxisPositions = positions;

            CurrentTrend = currentTrend;
        }

        private TrendChartInfo _currentTrend;
        public TrendChartInfo CurrentTrend
        {
            get { return _currentTrend; }
            set { SetProperty(ref _currentTrend, value); }
        }

        public IList<string> BrushNames { get; }

        public IEnumerable<AxixPosition> AxisPositions { get; }
    }

    public class AxixPosition
    {
        public AxixPosition(LiveCharts.AxisPosition position, string name)
        {
            Position = position;
            if (!string.IsNullOrWhiteSpace(name))
                Name = name;
            else
                Name = Enum.GetName(typeof(LiveCharts.AxisPosition), position);
        }

        public LiveCharts.AxisPosition Position { get;private set; }

        public string Name { get; private set; }
    }
}
