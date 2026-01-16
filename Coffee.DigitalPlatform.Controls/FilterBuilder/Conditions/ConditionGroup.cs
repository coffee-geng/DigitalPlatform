using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public class ConditionGroup : ConditionTreeItem, ICloneable
    {
        public ConditionGroupType Type { get; set; } = ConditionGroupType.And;

        public override bool CalculateResult(object entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            if (!Items.Any())
            {
                return true;
            }

            return Type == ConditionGroupType.And
                ? Items.Aggregate(true, (current, item) => current && item.CalculateResult(entity))
                : Items.Aggregate(false, (current, item) => current || item.CalculateResult(entity));
        }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();

            var groupType = Type.ToString().ToLower();

            var itemCount = Items.Count;
            if (itemCount > 1)
            {
                stringBuilder.Append('(');
            }

            for (var i = 0; i < itemCount; i++)
            {
                if (i > 0)
                {
                    stringBuilder.Append($" {groupType} ");
                }

                var item = Items[i];
                var itemString = item.ToString();
                stringBuilder.Append(itemString);
            }

            if (itemCount > 1)
            {
                stringBuilder.Append(')');
            }

            return stringBuilder.ToString();
        }

        public object Clone()
        {
            var clone = new ConditionGroup()
            {
                Type = this.Type
            };
            if (this.Items.Any())
            {
                foreach(var item in this.Items)
                {
                    ConditionTreeItem itemClone = null;
                    if (item is ConditionGroup group)
                    {
                        itemClone = (ConditionGroup)(group.Clone());
                        itemClone.Parent = clone;
                    }
                    else if (item is PropertyExpression propExpression)
                    {
                        itemClone = (PropertyExpression)(propExpression.Clone());
                        itemClone.Parent = clone;
                    }
                    if (itemClone != null)
                    {
                        clone.Items.Add(itemClone);
                    }
                }
            }
            return clone;
        }
    }
}
