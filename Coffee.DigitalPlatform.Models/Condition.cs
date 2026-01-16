using Coffee.DigitalPlatform.Common;
using Coffee.DigitalPlatform.Controls.FilterBuilder;
using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Models
{
    public class Condition : ICondition
    {
        public Condition(Variable source, object targetValue, ConditionOperator @operator)
        {
            if (source == null) 
                throw new ArgumentNullException("source");

            Source = source;
            TargetValue = targetValue;
            Operator = @operator;
            ConditionNum = shortid.ShortId.Generate(new shortid.Configuration.GenerationOptions(true, true, 18));
        }

        private Condition(FilterScheme filterScheme)
        {
            if (filterScheme == null)
                throw new ArgumentNullException(nameof(filterScheme));
            if (filterScheme.ConditionItems.Count == 0)
                throw new ArgumentException("No condition found.");
            var expression = filterScheme.ConditionItems.Where(c => c is PropertyExpression).FirstOrDefault();
            if (expression == null)
                throw new ArgumentException("No condition found.");
            _rawPropertyExpression = expression as PropertyExpression;
            _filterScheme = filterScheme;

            ConditionNum = shortid.ShortId.Generate(new shortid.Configuration.GenerationOptions(true, true, 18));
            RawToWrapper();
        }

        private Condition(PropertyExpression expression)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));
            _rawPropertyExpression = expression;

            ConditionNum = shortid.ShortId.Generate(new shortid.Configuration.GenerationOptions(true, true, 18));
            RawToWrapper();
        }

        //初始化表达式PropertyExpression的包装类Condition，即将表达式的属性映射到包装类
        private void RawToWrapper()
        {
            if (_rawPropertyExpression != null)
            {
                var propertyMetadata = _rawPropertyExpression.Property;
                var expressionMetadata =_rawPropertyExpression.DataTypeExpression;
                if (propertyMetadata == null || expressionMetadata == null)
                    return;
                var propInfo = propertyMetadata.OwnerType.GetProperty(propertyMetadata.Name);
                var @var = new Variable()
                {
                    VarType = propertyMetadata.Type,
                    VarName = propertyMetadata.DisplayName,
                    OwnerTypeInFilterScheme = propertyMetadata.OwnerType,
                    PropertyInFilterScheme = propInfo
                };
                Source = @var;
                
                var operatorDict = getAllConditionOperators();
                if (operatorDict.TryGetValue((ConditionOperators)expressionMetadata.SelectedCondition, out ConditionOperator conditionOperator))
                {
                    Operator = conditionOperator;
                }

                var valuePropertyInfo = expressionMetadata.GetType().GetProperty("Value");
                if (valuePropertyInfo != null)
                {
                    TargetValue = valuePropertyInfo.GetValue(expressionMetadata);
                }
            }
        }

        private Dictionary<ConditionOperators, ConditionOperator> getAllConditionOperators()
        {
            Dictionary<ConditionOperators, ConditionOperator> dict = new Dictionary<ConditionOperators, ConditionOperator>();
            var array = Enum.GetValues(typeof(ConditionOperators));
            foreach (var item in array)
            {
                var conditionOperator = new ConditionOperator((ConditionOperators)item);
                dict.Add((ConditionOperators)item, conditionOperator);
            }
            return dict;
        }

        public string ConditionNum {  get; private set; }

        public Variable Source { get; private set; }

        public ConditionOperator Operator { get; private set; }

        public object TargetValue {  get; private set; }

        string ICondition.ConditionNum { get; set; }

        public ConditionChain Parent { get; private set; }

        void ICondition.SetParent(ConditionChain conditionGroup)
        {
            this.Parent = conditionGroup;
        }

        public ConditionTreeItem Raw
        {
            get
            {
                if (_rawPropertyExpression == null)
                {
                    WrapperToRaw();
                }
                return _rawPropertyExpression;
            }
        }

        private void WrapperToRaw()
        {
            if (Source == null)
                throw new NullReferenceException(nameof(Source));
            if (Source.OwnerTypeInFilterScheme == null)
                throw new NullReferenceException(nameof(Source.OwnerTypeInFilterScheme));
            if (Source.PropertyInFilterScheme == null)
                throw new NullReferenceException(nameof(Source.PropertyInFilterScheme));

            var propertyMetadata = new PropertyMetadata(Source.OwnerTypeInFilterScheme.GetType(), Source.PropertyInFilterScheme);
            propertyMetadata.DisplayName = Source.VarName;
            var rawPropertyExpression = new PropertyExpression()
            {
                Property = propertyMetadata
            };

            if (Parent != null && Parent.Raw != null)
            {
                if (!Parent.Raw.Items.Contains(rawPropertyExpression))
                {
                    Parent.Raw.Items.Add(rawPropertyExpression);
                }
                rawPropertyExpression.Parent = Parent.Raw;
            }

            _rawPropertyExpression = rawPropertyExpression;
        }

        private PropertyExpression _rawPropertyExpression;

        private FilterScheme _filterScheme;

        public bool IsMatch()
        {
            if (Source.VarType == typeof(bool))
            {
                return calculateBooleanResult();
            }
            else if (Source.VarType.IsEnum)
            {
                return calculateEnumResult();
            }
            else if (Source.VarType == typeof(TimeSpan))
            {
                return CalculateTimeSpanResult();
            }
            else if (Source.VarType == typeof(string))
            {
                return calculateStringResult();
            }
            else if (Source.VarType == typeof(byte))
            {
                return calculateValueDateResult<byte>();
            }
            else if (Source.VarType == typeof(short))
            {
                return calculateValueDateResult<short>();
            }
            else if (Source.VarType == typeof(ushort))
            {
                return calculateValueDateResult<ushort>();
            }
            else if (Source.VarType == typeof(int))
            {
                return calculateValueDateResult<int>();
            }
            else if (Source.VarType == typeof(uint))
            {
                return calculateValueDateResult<uint>();
            }
            else if (Source.VarType == typeof(long))
            {
                return calculateValueDateResult<long>();
            }
            else if (Source.VarType == typeof(ulong))
            {
                return calculateValueDateResult<ulong>();
            }
            else if (Source.VarType == typeof(float))
            {
                return calculateValueDateResult<float>();
            }
            else if (Source.VarType == typeof(double))
            {
                return calculateValueDateResult<double>();
            }
            else if (Source.VarType == typeof(decimal))
            {
                return calculateValueDateResult<decimal>();
            }
            else if (Source.VarType == typeof(sbyte))
            {
                return calculateValueDateResult<sbyte>();
            }
            else if (Source.VarType == typeof(DateTime))
            {
                return calculateValueDateResult<DateTime>();
            }
            else
            {
                return false;
            }
        }

        private bool calculateValueDateResult<TValue>() where TValue : struct, IComparable, IFormattable, IComparable<TValue>, IEquatable<TValue>
        {
            if (TargetValue == null || !(TargetValue is TValue targetValue))
            {
                throw new NotSupportedException("TargetValue is invalid.");
            }
            var _comparer = Comparer<TValue>.Default;
            if (Source.IsNullableVar)
            {
                var sourceValue = (TValue?)Source.Value;
                return Operator.Operator switch
                {
                    ConditionOperators.EqualTo => Equals(sourceValue, TargetValue),
                    ConditionOperators.NotEqualTo => !Equals(sourceValue, TargetValue),
                    ConditionOperators.GreaterThan => sourceValue is not null && _comparer.Compare(sourceValue.Value, targetValue) > 0,
                    ConditionOperators.LessThan => sourceValue is not null && _comparer.Compare(sourceValue.Value, targetValue) < 0,
                    ConditionOperators.GreaterThanOrEqualTo => sourceValue is not null && _comparer.Compare(sourceValue.Value, targetValue) >= 0,
                    ConditionOperators.LessThanOrEqualTo => sourceValue is not null && _comparer.Compare(sourceValue.Value, targetValue) <= 0,
                    ConditionOperators.IsNull => sourceValue is null,
                    ConditionOperators.NotIsNull => sourceValue is not null,
                    _ => throw new NotSupportedException($"Operator {Enum.GetName(typeof(ConditionOperators), Operator.Operator)} is not support in type of {typeof(TValue).Name}")
                };
            }
            else
            {
                var sourceValue = (TValue)Source.Value;
                return Operator.Operator switch
                {
                    ConditionOperators.EqualTo => Equals(sourceValue, TargetValue),
                    ConditionOperators.NotEqualTo => !Equals(sourceValue, TargetValue),
                    ConditionOperators.GreaterThan => _comparer.Compare(sourceValue, targetValue) > 0,
                    ConditionOperators.LessThan => _comparer.Compare(sourceValue, targetValue) < 0,
                    ConditionOperators.GreaterThanOrEqualTo => _comparer.Compare(sourceValue, targetValue) >= 0,
                    ConditionOperators.LessThanOrEqualTo => _comparer.Compare(sourceValue, targetValue) <= 0,
                    _ => throw new NotSupportedException($"Operator {Enum.GetName(typeof(ConditionOperators), Operator.Operator)} is not support in type of {typeof(TValue).Name}")
                };
            }
        }

        private bool calculateBooleanResult()
        {
            var sourceValue = (bool)Source.Value;
            var targetValue = (bool)TargetValue;

            return Operator.Operator switch
            {
                ConditionOperators.EqualTo => sourceValue == targetValue,
                ConditionOperators.NotEqualTo => sourceValue != targetValue,
                _ => throw new NotSupportedException($"Operator {Enum.GetName(typeof(ConditionOperators), Operator.Operator)} is not support in type of {Source.VarType.Name}")
            };
        }

        private bool calculateEnumResult()
        {
            if (!Source.IsNullableVar && Operator.Operator is ConditionOperators.IsNull or ConditionOperators.NotIsNull)
            {
                throw new NotSupportedException($"Operator {Enum.GetName(typeof(ConditionOperators), Operator.Operator)} is not support in type of {Source.VarType.Name}");
            }

            var sourceValue = Source.Value;
            var targetValue = TargetValue;

            return Operator.Operator switch
            {
                ConditionOperators.EqualTo => Equals(sourceValue, targetValue),
                ConditionOperators.GreaterThan => Comparer.Default.Compare(sourceValue, targetValue) > 0,
                ConditionOperators.GreaterThanOrEqualTo => Comparer.Default.Compare(sourceValue, targetValue) >= 0,
                ConditionOperators.LessThan => Comparer.Default.Compare(sourceValue, targetValue) < 0,
                ConditionOperators.LessThanOrEqualTo => Comparer.Default.Compare(sourceValue, targetValue) <= 0,
                ConditionOperators.NotEqualTo => !Equals(sourceValue, targetValue),
                ConditionOperators.IsNull => sourceValue is null,
                ConditionOperators.NotIsNull => sourceValue is not null,
                _ => throw new NotSupportedException($"Operator {Enum.GetName(typeof(ConditionOperators), Operator.Operator)} is not support in type of {typeof(bool).Name}")
            };
        }

        private bool calculateStringResult()
        {
            if (TargetValue == null || !(TargetValue is string targetValue))
            {
                throw new NotSupportedException("TargetValue is invalid.");
            }

            var sourceValue = Source.Value as string;

            if (sourceValue is null && Source.VarType.IsEnum)
            {
                if (Source.Value is not null)
                {
                    sourceValue = Source.Value.ToString();
                }
            }

            return Operator.Operator switch
            {
                ConditionOperators.Contains => sourceValue is not null && sourceValue.IndexOf(targetValue, StringComparison.CurrentCultureIgnoreCase) != -1,
                ConditionOperators.DoesNotContain => sourceValue is not null && sourceValue.IndexOf(targetValue, StringComparison.CurrentCultureIgnoreCase) == -1,
                ConditionOperators.EndsWith => sourceValue is not null && sourceValue.EndsWith(targetValue, StringComparison.CurrentCultureIgnoreCase),
                ConditionOperators.DoesNotEndWith => sourceValue is not null && !sourceValue.EndsWith(targetValue, StringComparison.CurrentCultureIgnoreCase),
                ConditionOperators.EqualTo => sourceValue == targetValue,
                ConditionOperators.GreaterThan => string.Compare(sourceValue, targetValue, StringComparison.OrdinalIgnoreCase) > 0,
                ConditionOperators.GreaterThanOrEqualTo => string.Compare(sourceValue, targetValue, StringComparison.OrdinalIgnoreCase) >= 0,
                ConditionOperators.IsEmpty => sourceValue == string.Empty,
                ConditionOperators.IsNull => sourceValue is null,
                ConditionOperators.LessThan => string.Compare(sourceValue, targetValue, StringComparison.OrdinalIgnoreCase) < 0,
                ConditionOperators.LessThanOrEqualTo => string.Compare(sourceValue, targetValue, StringComparison.OrdinalIgnoreCase) <= 0,
                ConditionOperators.NotEqualTo => sourceValue != targetValue,
                ConditionOperators.NotIsEmpty => sourceValue != string.Empty,
                ConditionOperators.NotIsNull => sourceValue is not null,
                ConditionOperators.StartsWith => sourceValue is not null && sourceValue.StartsWith(targetValue, StringComparison.CurrentCultureIgnoreCase),
                ConditionOperators.DoesNotStartWith => sourceValue is not null && !sourceValue.StartsWith(targetValue, StringComparison.CurrentCultureIgnoreCase),
                ConditionOperators.Matches => sourceValue is not null && RegexHelper.IsValid(targetValue) && new Regex(targetValue, RegexOptions.Compiled, TimeSpan.FromSeconds(1)).IsMatch(sourceValue),
                ConditionOperators.DoesNotMatch => sourceValue is not null && RegexHelper.IsValid(targetValue) && !new Regex(targetValue, RegexOptions.Compiled, TimeSpan.FromSeconds(1)).IsMatch(sourceValue),
                _ => throw new NotSupportedException($"Operator {Enum.GetName(typeof(ConditionOperators), Operator.Operator)} is not support in type of {typeof(string).Name}")
            };
        }

        private bool CalculateTimeSpanResult()
        {
            if (TargetValue == null || !(TargetValue is TimeSpan targetValue))
            {
                throw new NotSupportedException("TargetValue is invalid.");
            }
            if (Source.Value == null || !(Source.Value is TimeSpan sourceValue))
            {
                throw new NotSupportedException("Source.Value is invalid.");
            }

            return Operator.Operator switch
            {
                ConditionOperators.EqualTo => sourceValue == targetValue,
                ConditionOperators.NotEqualTo => sourceValue != targetValue,
                ConditionOperators.GreaterThan => sourceValue > targetValue,
                ConditionOperators.LessThan => sourceValue < targetValue,
                ConditionOperators.GreaterThanOrEqualTo => sourceValue >= targetValue,
                ConditionOperators.LessThanOrEqualTo => sourceValue <= targetValue,
                _ => throw new NotSupportedException($"Operator {Enum.GetName(typeof(ConditionOperators), Operator.Operator)} is not support in type of {typeof(TimeSpan).Name}")
            };
        }

        public override string ToString()
        {
            if (_filterScheme != null)
                return _filterScheme.ToString();
            if (_rawPropertyExpression != null)
                return _rawPropertyExpression.ToString();
            return base.ToString();
        }

        public static class ConditionFactory
        {
            public static Condition CreateCondition(FilterScheme filterScheme)
            {
                return new Condition(filterScheme);
            }

            public static Condition CreateCondition(PropertyExpression expression)
            {
                return new Condition(expression);
            }
        }
    }

    public class ConditionFactory
    {
        public static ICondition CreateCondition(FilterScheme filterScheme)
        {
            if (filterScheme == null)
                throw new ArgumentNullException(nameof(filterScheme));
            if (filterScheme.ConditionItems.Count == 0)
                return Condition.ConditionFactory.CreateCondition(filterScheme);
            if (filterScheme.ConditionItems.Count == 1)
            {
                if (filterScheme.ConditionItems.First() is PropertyExpression propertyExpression)
                    return Condition.ConditionFactory.CreateCondition(propertyExpression);
                else
                    return ConditionChain.ConditionChainFactory.CreateCondition(filterScheme.ConditionItems.First() as ConditionGroup);
            }
            else
            {
                var rootCondition = ConditionChain.ConditionChainFactory.CreateCondition((ConditionGroup)null);
                foreach (var item in filterScheme.ConditionItems)
                {
                    if (item is PropertyExpression expression)
                    {
                        var condition = Condition.ConditionFactory.CreateCondition(expression);
                        rootCondition.ConditionItems.Add(condition);
                    }
                    else if (item is ConditionGroup group)
                    {
                        var conditionGroup = ConditionChain.ConditionChainFactory.CreateCondition(group);
                        rootCondition.ConditionItems.Add(conditionGroup);
                    }
                }
                return rootCondition;
            }
        }
    }

    public interface ICondition
    {
        bool IsMatch();

        ConditionChain Parent { get; }

        void SetParent(ConditionChain conditionGroup);

        string ConditionNum { get; set; }

        ConditionTreeItem Raw {  get; }
    }
}
