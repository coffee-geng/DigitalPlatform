using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public class ConditionViewModel : ObservableObject
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

            LoadFilterSchemeCommand = new RelayCommand<FilterSchemeEditInfo>(doLoadFilterSchemeCommand);
            UnloadFilterSchemeCommand = new RelayCommand(doUnloadFilterSchemeCommand);

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

        private string _filterSchemeTitle;
        public string FilterSchemeTitle
        {
            get { return _filterSchemeTitle; }
            set { SetProperty(ref _filterSchemeTitle, value); }
        }

        private FilterScheme _filterScheme;
        public FilterScheme FilterScheme
        {
            get { return _filterScheme; }
            set { SetProperty(ref _filterScheme, value); }
        }

        private IEnumerable _rawCollection;
        public IEnumerable RawCollection 
        {
            get { return _rawCollection; }
            set { _rawCollection = value; }
        }

        public IList PreviewItems { get; }

        private List<IPropertyMetadata> _instanceProperties;
        public List<IPropertyMetadata> InstanceProperties
        {
            get { return _instanceProperties; }
            set { SetProperty(ref _instanceProperties, value); }
        }

        public RelayCommand<ConditionTreeItem> AddGroupCommand { get; }
        public RelayCommand<ConditionTreeItem> AddExpressionCommand { get; }
        public RelayCommand<ConditionTreeItem> DeleteConditionItem { get; }

        public RelayCommand TogglePreview { get; }

        public RelayCommand<FilterSchemeEditInfo> LoadFilterSchemeCommand { get; }

        public RelayCommand UnloadFilterSchemeCommand {  get; }

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

        private void doLoadFilterSchemeCommand(FilterSchemeEditInfo filterSchemeEditInfo)
        {
            ArgumentNullException.ThrowIfNull(filterSchemeEditInfo);

            RawCollection = filterSchemeEditInfo.RawCollection;

            _origionFilterScheme = filterSchemeEditInfo.FilterScheme;

            InstanceProperties = new InstanceProperties(_origionFilterScheme.TargetType).Properties;

            FilterScheme = (FilterScheme)_origionFilterScheme.Clone();
            FilterSchemeTitle = _origionFilterScheme.Title;

            if (dispatcherTimer != null)
            {
                dispatcherTimer.Start();
            }
        }

        private void doUnloadFilterSchemeCommand()
        {
            if (dispatcherTimer != null)
            {
                dispatcherTimer.Stop();
            }
        }
    }
}
