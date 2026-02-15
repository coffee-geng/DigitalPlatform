using Coffee.DigitalPlatform.Common;
using Coffee.DigitalPlatform.Controls.FilterBuilder;
using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
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
using System.Windows;
using System.Windows.Media;

namespace Coffee.DigitalPlatform.Models
{
    public class Condition : ICondition
    {
        public Condition(Variable source, object targetValue, ConditionOperator @operator, string conditionNum = null)
        {
            if (source == null) 
                throw new ArgumentNullException("source");

            Source = source;
            TargetValue = targetValue;
            Operator = @operator;
            if (!string.IsNullOrWhiteSpace(conditionNum))
                ConditionNum = conditionNum;
            else
                ConditionNum = shortid.ShortId.Generate(new shortid.Configuration.GenerationOptions(true, true, 18));
        }

        private Condition(FilterScheme filterScheme, string conditionNum = null)
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

            if (!string.IsNullOrWhiteSpace(conditionNum))
                ConditionNum = conditionNum;
            else
                ConditionNum = shortid.ShortId.Generate(new shortid.Configuration.GenerationOptions(true, true, 18));
            RawToWrapper();
        }

        private Condition(PropertyExpression expression, string conditionNum = null)
        {
            if (expression == null)
                throw new ArgumentNullException(nameof(expression));
            _rawPropertyExpression = expression;

            if (!string.IsNullOrWhiteSpace(conditionNum))
                ConditionNum = conditionNum;
            else
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
                    VarNum = propertyMetadata.Name,
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

        public void SyncDeviceNum(string deviceNum)
        {
            if (Source != null)
            {
                Source.DeviceNum = deviceNum;
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

        public Variable Source { get; private set; }

        public ConditionOperator Operator { get; private set; }

        public object TargetValue {  get; private set; }

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

            var propertyMetadata = new Controls.FilterBuilder.PropertyMetadata(Source.OwnerTypeInFilterScheme, Source.PropertyInFilterScheme);
            propertyMetadata.DisplayName = Source.VarName;
            var rawPropertyExpression = new PropertyExpression()
            {
                Property = propertyMetadata
            };
            syncValueToPropertyExpression(rawPropertyExpression);

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

        // 将包装类的值同步到表达式PropertyExpression中
        private void syncValueToPropertyExpression(PropertyExpression propertyExpression)
        {
            if (propertyExpression == null || propertyExpression.DataTypeExpression == null)
                return;
            var dataTypeExp = propertyExpression.DataTypeExpression;
            dataTypeExp.SelectedCondition = (Controls.FilterBuilder.Condition)this.Operator.Operator;

            if (dataTypeExp is BooleanExpression boolExp)
            {
                if (TargetValue is bool)
                {
                    boolExp.Value = (bool)TargetValue;
                }
                else if (TargetValue != null && bool.TryParse(TargetValue.ToString(), out bool boolResult))
                {
                    boolExp.Value = boolResult;
                }
            }
            else if (dataTypeExp is TimeSpanExpression timespanExp)
            {
                if (Source.IsNullableVar && TargetValue == null)
                {
                    timespanExp.Value = default;
                }
                else
                {
                    if (TargetValue is TimeSpan)
                    {
                        timespanExp.Value = (TimeSpan)TargetValue;
                    }
                    else if (TargetValue != null && TimeSpan.TryParse(TargetValue.ToString(), out TimeSpan timeSpanResult))
                    {
                        timespanExp.Value = timeSpanResult;
                    }
                }
            }
            else if (dataTypeExp is StringExpression stringExp)
            {
                stringExp.Value = TargetValue != null ? TargetValue.ToString() : null;
            }
            else if (dataTypeExp is ByteExpression byteExp)
            {
                if (Source.IsNullableVar && TargetValue == null)
                {
                    byteExp.Value = default;
                }
                else
                {
                    if (TargetValue is byte)
                    {
                        byteExp.Value = (byte)TargetValue;
                    }
                    else if (TargetValue != null && byte.TryParse(TargetValue.ToString(), out byte byteResult))
                    {
                        byteExp.Value = byteResult;
                    }
                }
            }
            else if (dataTypeExp is ShortExpression shortExp)
            {
                if (Source.IsNullableVar && TargetValue == null)
                {
                    shortExp.Value = default;
                }
                else
                {
                    if (TargetValue is short)
                    {
                        shortExp.Value = (short)TargetValue;
                    }
                    else if (TargetValue != null && short.TryParse(TargetValue.ToString(), out short shortResult))
                    {
                        shortExp.Value = shortResult;
                    }
                }
            }
            else if (dataTypeExp is UnsignedShortExpression ushortExp)
            {
                if (Source.IsNullableVar && TargetValue == null)
                {
                    ushortExp.Value = default;
                }
                else
                {
                    if (TargetValue is ushort)
                    {
                        ushortExp.Value = (ushort)TargetValue;
                    }
                    else if (TargetValue != null && ushort.TryParse(TargetValue.ToString(), out ushort ushortResult))
                    {
                        ushortExp.Value = ushortResult;
                    }
                }
            }
            else if (dataTypeExp is IntegerExpression intExp)
            {
                if (Source.IsNullableVar && TargetValue == null)
                {
                    intExp.Value = default;
                }
                else
                {
                    if (TargetValue is int)
                    {
                        intExp.Value = (int)TargetValue;
                    }
                    else if (TargetValue != null && int.TryParse(TargetValue.ToString(), out int intResult))
                    {
                        intExp.Value = intResult;
                    }
                }
            }
            else if (dataTypeExp is UnsignedIntegerExpression uintExp)
            {
                if (Source.IsNullableVar && TargetValue == null)
                {
                    uintExp.Value = default;
                }
                else
                {
                    if (TargetValue is uint)
                    {
                        uintExp.Value = (uint)TargetValue;
                    }
                    else if (TargetValue != null && uint.TryParse(TargetValue.ToString(), out uint uintResult))
                    {
                        uintExp.Value = uintResult;
                    }
                }
            }
            else if (dataTypeExp is LongExpression longExp)
            {
                if (Source.IsNullableVar && TargetValue == null)
                {
                    longExp.Value = default;
                }
                else
                {
                    if (TargetValue is long)
                    {
                        longExp.Value = (long)TargetValue;
                    }
                    else if (TargetValue != null && long.TryParse(TargetValue.ToString(), out long longResult))
                    {
                        longExp.Value = longResult;
                    }
                }
            }
            else if (dataTypeExp is UnsignedLongExpression ulongExp)
            {
                if (Source.IsNullableVar && TargetValue == null)
                {
                    ulongExp.Value = default;
                }
                else
                {
                    if (TargetValue is ulong)
                    {
                        ulongExp.Value = (ulong)TargetValue;
                    }
                    else if (TargetValue != null && ulong.TryParse(TargetValue.ToString(), out ulong ulongResult))
                    {
                        ulongExp.Value = ulongResult;
                    }
                }
            }
            else if (dataTypeExp is FloatExpression floatExp)
            {
                if (Source.IsNullableVar && TargetValue == null)
                {
                    floatExp.Value = default;
                }
                else
                {
                    if (TargetValue is float)
                    {
                        floatExp.Value = (float)TargetValue;
                    }
                    else if (TargetValue != null && float.TryParse(TargetValue.ToString(), out float floatResult))
                    {
                        floatExp.Value = floatResult;
                    }
                }
            }
            else if (dataTypeExp is DoubleExpression doubleExp)
            {
                if (Source.IsNullableVar && TargetValue == null)
                {
                    doubleExp.Value = default;
                }
                else
                {
                    if (TargetValue is double)
                    {
                        doubleExp.Value = (double)TargetValue;
                    }
                    else if (TargetValue != null && double.TryParse(TargetValue.ToString(), out double doubleResult))
                    {
                        doubleExp.Value = doubleResult;
                    }
                }
            }
            else if (dataTypeExp is DecimalExpression decimalExp)
            {
                if (Source.IsNullableVar && TargetValue == null)
                {
                    decimalExp.Value = default;
                }
                else
                {
                    if (TargetValue is decimal)
                    {
                        decimalExp.Value = (decimal)TargetValue;
                    }
                    else if (TargetValue != null && decimal.TryParse(TargetValue.ToString(), out decimal decimalResult))
                    {
                        decimalExp.Value = decimalResult;
                    }
                }
            }
            else if (dataTypeExp is SByteExpression sbyteExp)
            {
                if (Source.IsNullableVar && TargetValue == null)
                {
                    sbyteExp.Value = default;
                }
                else
                {
                    if (TargetValue is sbyte)
                    {
                        sbyteExp.Value = (sbyte)TargetValue;
                    }
                    else if (TargetValue != null && sbyte.TryParse(TargetValue.ToString(), out sbyte sbyteResult))
                    {
                        sbyteExp.Value = sbyteResult;
                    }
                }
            }
            else if (dataTypeExp is DateTimeExpression dateTimeExp)
            {
                if (Source.IsNullableVar && TargetValue == null)
                {
                    dateTimeExp.Value = default;
                }
                else
                {
                    if (TargetValue is DateTime)
                    {
                        dateTimeExp.Value = (DateTime)TargetValue;
                    }
                    else if (TargetValue != null && DateTime.TryParse(TargetValue.ToString(), out DateTime datetimeResult))
                    {
                        dateTimeExp.Value = datetimeResult;
                    }
                }
            }
            else if (IsEnumExpressionInstance(dataTypeExp))
            {
                try
                {
                    dataTypeExp.GetType().GetProperty("Value").SetValue(dataTypeExp, TargetValue);
                }
                catch { }
            }
        }

        private static bool IsEnumExpressionInstance(object obj)
        {
            if (obj == null) return false;

            Type type = obj.GetType();

            // 检查是否是泛型类型
            if (!type.IsGenericType) return false;

            // 获取泛型类型定义（去掉类型参数）
            Type genericType = type.GetGenericTypeDefinition();

            // 检查是否匹配 EnumExpression<> 类型定义
            return genericType == typeof(EnumExpression<>);
        }

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
                var sourceValue = (TValue?)Source.FinalValue;
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
                var sourceValue = (TValue)Source.FinalValue;
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
            var sourceValue = (bool)Source.FinalValue;
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

            var sourceValue = Source.FinalValue;
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

            var sourceValue = Source.FinalValue as string;

            if (sourceValue is null && Source.VarType.IsEnum)
            {
                if (Source.Value is not null)
                {
                    sourceValue = Source.FinalValue.ToString();
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
            if (Source.Value == null || !(Source.FinalValue is TimeSpan sourceValue))
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

        public IList<Variable> GetSourceVariables()
        {
            List<Variable> variables = new List<Variable>();
            if (Source != null)
            {
                variables.Add(Source);
            }
            return variables;
        }

        public HtmlNode GetExpressionResult(Dictionary<string, object> valueDict, ExpressionFormatSetting formater)
        {
            if (valueDict == null || !valueDict.TryGetValue(Source.VarNum, out object value))
                return null;
            if (formater != null)
            {
                var htmlEle = HtmlNode.CreateNode($"<span></span>");
                string key = Enum.GetName(typeof(ExpressionFormatBlocks), ExpressionFormatBlocks.Expression);
                StringBuilder styleBuilder = new StringBuilder();
                double fontSize = formater.GetFontSize(key);
                styleBuilder.Append($"font-size:{fontSize}px;");
                if (IsMatch())
                {
                    Color color = formater.GetFontForeground(key);
                    styleBuilder.Append($"color:{string.Format("#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B)};");
                    FontWeight fontWeight = formater.GetFontWeight(key);
                    styleBuilder.Append($"font-weight:{fontWeight.ToString().ToLower()};");
                    htmlEle.SetAttributeValue("style", styleBuilder.ToString());
                }
                htmlEle.InnerHtml = $"{Source.VarName} {Operator.DisplayName} {TargetValue}";
                return htmlEle;
            }
            else
            {
                var htmlNode = HtmlNode.CreateNode($"<span></span>");
                htmlNode.InnerHtml = $"{Source.VarName} {Operator.DisplayName} {TargetValue}";
                return htmlNode;
            }
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

        void SyncDeviceNum(string deviceNum);

        IList<Variable> GetSourceVariables();

        HtmlNode GetExpressionResult(Dictionary<string, object> valueDict, ExpressionFormatSetting formater);
    }

    /// <summary>
    /// 对表达式的哪一块区域进行格式化
    /// </summary>
    public enum ExpressionFormatBlocks
    {
        Expression = 0, //整个表达式
        Source, //表达式的左侧，即变量
        Operator, //表达式的中间，即条件运算符
        TargetValue //表达式的右侧，即目标值
    }
}
