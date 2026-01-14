using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public class ConditionViewModel : ObservableObject, IDisposable
    {
        public ConditionViewModel(FilterSchemeEditInfo filterSchemeEditInfo)
        {
            ArgumentNullException.ThrowIfNull(filterSchemeEditInfo);

            RawCollection = filterSchemeEditInfo.RawCollection;

            _origionFilterScheme = filterSchemeEditInfo.FilterScheme;

            InstanceProperties = new InstanceProperties(_origionFilterScheme.TargetType).Properties;

            FilterScheme = new FilterScheme(_origionFilterScheme.TargetType)
            {
                Title = _origionFilterScheme.Title
            };
            FilterSchemeTitle = string.Empty;

            AddGroupCommand = new RelayCommand<ConditionTreeItem>(OnAddGroup);
            AddExpressionCommand = new RelayCommand<ConditionTreeItem>(OnAddExpression);
            DeleteConditionItem = new RelayCommand<ConditionTreeItem>(OnDeleteCondition, OnDeleteConditionCanExecute);

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Interval = TimeSpan.FromMicroseconds(1000);
            dispatcherTimer.Tick += DispatcherTimer_Tick;
            dispatcherTimer.Start();
        }

        private void DispatcherTimer_Tick(object? sender, EventArgs e)
        {
            if (!checkSameFilterScheme(_origionFilterScheme, FilterScheme))
            {
                FilterSchemeUpdateTime = DateTime.Now;
            }
        }

        private DispatcherTimer dispatcherTimer;

        private FilterScheme _origionFilterScheme;

        private bool checkSameFilterScheme(FilterScheme oldScheme, FilterScheme newScheme)
        {
            if (!ReferenceEquals(oldScheme, newScheme)) return false;
            if (oldScheme == newScheme || newScheme == null) return false;
            return oldScheme.ToString().Equals(newScheme.ToString());
        }

        private DateTime _filterSchemeUpdateTime = DateTime.Now;
        public DateTime FilterSchemeUpdateTime
        {
            get { return _filterSchemeUpdateTime; }
            set { SetProperty(ref _filterSchemeUpdateTime, value); }
        }

        public string Title
        {
            get { return "Filter scheme"; }
        }

        public string FilterSchemeTitle { get; set; }
        public FilterScheme FilterScheme { get; private set; }
       
        public IEnumerable RawCollection { get; }
        public IList PreviewItems { get; }

        public List<IPropertyMetadata> InstanceProperties { get; }

        public RelayCommand<ConditionTreeItem> AddGroupCommand { get; }
        public RelayCommand<ConditionTreeItem> AddExpressionCommand { get; }
        public RelayCommand<ConditionTreeItem> DeleteConditionItem { get; }

        public RelayCommand TogglePreview { get; }

        private bool OnDeleteConditionCanExecute(ConditionTreeItem? item)
        {
            if (item is null)
            {
                return false;
            }

            if (!item.IsRoot())
            {
                return true;
            }

            return FilterScheme.ConditionItems.Count > 1;
        }

        private void OnDeleteCondition(ConditionTreeItem? item)
        {
            if (item is null)
            {
                return;
            }

            if (item.Parent is null)
            {
                FilterScheme.ConditionItems.Remove(item);

                foreach (var conditionItem in FilterScheme.ConditionItems)
                {
                    conditionItem.Items.Remove(item);
                }
            }
            else
            {
                item.Parent.Items.Remove(item);
            }
        }

        private void OnAddExpression(ConditionTreeItem group)
        {
            if (group is null)
            {
                return;
            }

            var propertyExpression = new PropertyExpression
            {
                Property = InstanceProperties.FirstOrDefault()
            };

            group.Items.Add(propertyExpression);
            propertyExpression.Parent = group;
        }

        private void OnAddGroup(ConditionTreeItem? group)
        {
            if (group is null)
            {
                return;
            }

            var newGroup = new ConditionGroup();
            group.Items.Add(newGroup);
            newGroup.Parent = group;
        }

        

        private void ApplyFilterScheme()
        {
            FilterScheme.Apply(RawCollection, PreviewItems);
        }

        public void Dispose()
        {
            if (dispatcherTimer != null)
            {
                dispatcherTimer.Stop();
                dispatcherTimer.Tick -= DispatcherTimer_Tick;
                dispatcherTimer = null;
            }
        }
    }
}
