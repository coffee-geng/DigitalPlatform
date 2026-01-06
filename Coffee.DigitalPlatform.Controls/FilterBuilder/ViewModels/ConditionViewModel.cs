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
    public class ConditionViewModel : ObservableObject
    {
        private readonly FilterScheme _originalFilterScheme;

        public ConditionViewModel(FilterSchemeEditInfo filterSchemeEditInfo)
        {
            ArgumentNullException.ThrowIfNull(filterSchemeEditInfo);

            RawCollection = filterSchemeEditInfo.RawCollection;
            
            var filterScheme = filterSchemeEditInfo.FilterScheme;
            _originalFilterScheme = filterScheme;

            InstanceProperties = new InstanceProperties(_originalFilterScheme.TargetType).Properties;

            FilterScheme = new FilterScheme(_originalFilterScheme.TargetType);

            FilterSchemeTitle = string.Empty;

            AddGroupCommand = new RelayCommand<ConditionTreeItem>(OnAddGroup);
            AddExpressionCommand = new RelayCommand<ConditionTreeItem>(OnAddExpression);
            DeleteConditionItem = new RelayCommand<ConditionTreeItem>(OnDeleteCondition, OnDeleteConditionCanExecute);

            TranslateCommand = new RelayCommand(() =>
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentUICulture = new CultureInfo("en-US");
            });
        }

        public string Title
        {
            get { return "Filter scheme"; }
        }

        public string FilterSchemeTitle { get; set; }
        public FilterScheme FilterScheme { get; }
       
        public IEnumerable RawCollection { get; }
        public IList PreviewItems { get; }

        public List<IPropertyMetadata> InstanceProperties { get; }

        public RelayCommand<ConditionTreeItem> AddGroupCommand { get; }
        public RelayCommand<ConditionTreeItem> AddExpressionCommand { get; }
        public RelayCommand<ConditionTreeItem> DeleteConditionItem { get; }

        public RelayCommand TranslateCommand { get; }

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
    }
}
