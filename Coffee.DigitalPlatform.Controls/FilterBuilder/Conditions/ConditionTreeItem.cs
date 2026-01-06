using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public abstract class ConditionTreeItem : ObservableObject
    {
        protected ConditionTreeItem()
        {
            Items = new ObservableCollection<ConditionTreeItem>();
            IsValid = true;
        }

        public ConditionTreeItem? Parent { get; set; }

        public bool IsValid { get; private set; }

        public ObservableCollection<ConditionTreeItem> Items { get; private set; }

        public event EventHandler<EventArgs>? Updated;

        private void OnConditionItemsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (sender is not IList listSender)
            {
                return;
            }

            if (e.OldItems is not null)
            {
                foreach (var item in e.OldItems)
                {
                    var conditionTreeItem = (ConditionTreeItem)item;

                    if (ReferenceEquals(conditionTreeItem, this))
                    {
                        conditionTreeItem.Parent = null;
                    }

                    conditionTreeItem.Updated -= OnConditionUpdated;
                }
            }

            var newCollection = e.Action == NotifyCollectionChangedAction.Reset
                ? listSender
                : e.NewItems;
            if (newCollection is null)
            {
                return;
            }

            foreach (var item in newCollection)
            {
                var conditionTreeItem = (ConditionTreeItem)item;

                conditionTreeItem.Parent = this;
                conditionTreeItem.Updated += OnConditionUpdated;
            }
        }

        private void OnItemsChanged()
        {
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            var items = Items;
            items.CollectionChanged += OnConditionItemsCollectionChanged;

            foreach (var item in items)
            {
                item.Updated += OnConditionUpdated;
            }
        }

        protected override void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);

            RaiseUpdated();
        }

        protected void RaiseUpdated()
        {
            Updated?.Invoke(this, EventArgs.Empty);
        }

        private void OnConditionUpdated(object? sender, EventArgs e)
        {
            RaiseUpdated();
        }

        public abstract bool CalculateResult(object entity);

        protected bool Equals(ConditionTreeItem other)
        {
            return Items.Equals(other.Items);
        }

        public override bool Equals(object? obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj.GetType() == GetType()
                   && Equals((ConditionTreeItem)obj);
        }

        public override int GetHashCode()
        {
            return Items.GetHashCode();
        }
    }
}
