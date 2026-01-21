using Coffee.DigitalPlatform.Controls.FilterBuilder;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Models
{
    public class ConditionChain : ICondition
    {
        public ConditionChain(ConditionChainOperators @operator, string conditionNum = null)
        {
            Operator = @operator;
            ConditionItems.CollectionChanged += ConditionItems_CollectionChanged;
            if (!string.IsNullOrWhiteSpace(conditionNum))
                ConditionNum = conditionNum;
            else
                ConditionNum = shortid.ShortId.Generate(new shortid.Configuration.GenerationOptions(true, true, 18));
        }

        private ConditionChain(FilterScheme filterScheme, string conditionNum = null)
        {
            if (filterScheme == null)
                throw new ArgumentNullException(nameof(filterScheme));
            _filterScheme = filterScheme;
            if (filterScheme.ConditionItems.Count == 0)
                throw new ArgumentException("No condition found.");
            var group = filterScheme.ConditionItems.Where(c => c is ConditionGroup).FirstOrDefault();
            if (group == null)
                throw new ArgumentException("No condition found.");
            _rawConditionGroup = group as ConditionGroup;

            ConditionItems.CollectionChanged += ConditionItems_CollectionChanged;
            if (!string.IsNullOrWhiteSpace(conditionNum))
                ConditionNum = conditionNum;
            else
                ConditionNum = shortid.ShortId.Generate(new shortid.Configuration.GenerationOptions(true, true, 18));
            initChain(_rawConditionGroup);  
        }

        private ConditionChain(ConditionGroup conditionGroup, string conditionNum = null)
        {
            if (conditionGroup != null)
            {
                _rawConditionGroup = conditionGroup;

                ConditionItems.CollectionChanged += ConditionItems_CollectionChanged;
                if (!string.IsNullOrWhiteSpace(conditionNum))
                    ConditionNum = conditionNum;
                else
                    ConditionNum = shortid.ShortId.Generate(new shortid.Configuration.GenerationOptions(true, true, 18));
                initChain(conditionGroup);
            }
        }

        private void initChain(ConditionGroup conditionGroup)
        {
            if (conditionGroup == null)
                return;
            foreach (var conditionItem in conditionGroup.Items)
            {
                if (conditionItem is PropertyExpression expression)
                {
                    var childCondition = Condition.ConditionFactory.CreateCondition(expression);
                    ConditionItems.Add(childCondition);
                }
                else if (conditionItem is ConditionGroup group)
                {
                    var childConditionGroup = ConditionChain.ConditionChainFactory.CreateCondition(group);
                    ConditionItems.Add(childConditionGroup);
                }
            }
        }

        // 同步设备编号到当前条件项及其所有子条件
        public void SyncDeviceNum(string deviceNum)
        {
            if (this.ConditionItems == null || this.ConditionItems.Count == 0)
                return;
            foreach (var conditionItem in this.ConditionItems)
            {
                if (conditionItem is ConditionChain conditionChain)
                {
                    conditionChain.SyncDeviceNum(deviceNum);
                }
                else if (conditionItem is Condition condition)
                {
                    condition.SyncDeviceNum(deviceNum);
                }
            }
        }

        public ConditionTreeItem Raw
        {
            get
            {
                if (_rawConditionGroup == null)
                {
                    WrapperToRaw();
                }
                return _rawConditionGroup;
            }
        }

        private void WrapperToRaw()
        {
            ConditionGroup rawConditionGroup = new ConditionGroup();
            rawConditionGroup.Type = (ConditionGroupType)Operator;
            _rawConditionGroup = rawConditionGroup;
            foreach (var conditionItem in ConditionItems)
            {
                var rawCondition = conditionItem.Raw;
                if (!rawConditionGroup.Items.Contains(rawCondition))
                {
                    rawConditionGroup.Items.Add(rawCondition);
                }
            }
        }

        public ConditionChainOperators Operator { get; set; }

        public ObservableCollection<ICondition> ConditionItems { get; } = new ObservableCollection<ICondition>();

        private void ConditionItems_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Remove && e.OldItems != null)
            {
                foreach(var oldItem in e.OldItems)
                {
                    (oldItem as ICondition).SetParent(null);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add && e.NewItems != null)
            {
                foreach(var newItem in e.NewItems)
                {
                    (newItem as ICondition).SetParent(this);
                }
            }
            else if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            {
                foreach(var item in (sender as ObservableCollection<ICondition>))
                {
                    item.SetParent(this);
                }
            }
        }

        private string _conditionNum;
        public string ConditionNum
        {
            get { return _conditionNum; }
            private set { _conditionNum = value; }
        }

        string ICondition.ConditionNum
        {
            get { return ConditionNum; }
            set { ConditionNum = value; }
        }

        public ConditionChain Parent { get; private set; }

        void ICondition.SetParent(ConditionChain conditionGroup)
        {
            this.Parent = conditionGroup;
        }

        private ConditionGroup _rawConditionGroup;

        private FilterScheme _filterScheme;

        public bool IsMatch()
        {
            if (ConditionItems.Count == 0) 
                return true;
            var firstCondition = ConditionItems.First();
            var otherConditions = ConditionItems.Skip(1);

            bool isMatch = firstCondition.IsMatch();
            foreach (var item in otherConditions)
            {
                if (Operator == ConditionChainOperators.AND)
                {
                    isMatch = isMatch && item.IsMatch();
                }
                else if (Operator == ConditionChainOperators.OR)
                {
                    isMatch = isMatch || item.IsMatch();
                }
            }
            return isMatch;
        }

        public override string ToString()
        {
            if (_filterScheme != null)
                return _filterScheme.ToString();
            if (_rawConditionGroup != null)
                return _rawConditionGroup.ToString();
            return base.ToString();
        }

        public static class ConditionChainFactory
        {
            public static ConditionChain CreateCondition(FilterScheme filterScheme)
            {
                return new ConditionChain(filterScheme);
            }

            public static ConditionChain CreateCondition(ConditionGroup conditionGroup)
            {
                return new ConditionChain(conditionGroup);
            }
        }
    }

    public enum ConditionChainOperators
    {
        [Display(Name = "并且")]
        AND,
        [Display(Name = "或者")]
        OR
    }
}
