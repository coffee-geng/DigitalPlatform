using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public class PropertyExpression : ConditionTreeItem, ICloneable
    {
        internal string? PropertySerializationValue { get; set; }

        private IPropertyMetadata? _propertyMetadata;
        public IPropertyMetadata? Property
        {
            get { return _propertyMetadata; }
            set 
            { 
                if (SetProperty(ref _propertyMetadata, value))
                {
                    OnPropertyChanged();
                }
            }
        }

        private DataTypeExpression? _dataTypeExpression;
        public DataTypeExpression? DataTypeExpression
        {
            get { return _dataTypeExpression; }
            set { SetProperty(ref _dataTypeExpression, value); }
        }

        private void OnPropertyChanged()
        {
            var dataTypeExpression = DataTypeExpression;
            if (dataTypeExpression is not null)
            {
                dataTypeExpression.PropertyChanged -= OnDataTypeExpressionPropertyChanged;
            }

            if (Property is not null)
            {
                CreateDataTypeExpression();
            }

            dataTypeExpression = DataTypeExpression;
            if (dataTypeExpression is not null)
            {
                dataTypeExpression.PropertyChanged += OnDataTypeExpressionPropertyChanged;
            }
        }

        private void CreateDataTypeExpression()
        {
            var property = Property;
            if (property is null)
            {
                var err = new InvalidOperationException("Cannot create data type expression without valid property");
                //_logger.LogError(err, err.Message, null);
                throw err;
            }

            var propertyType = property.Type;
            var isNullable = propertyType.IsNullableType();
            if (isNullable)
            {
                propertyType = propertyType.GetNonNullable();
            }

            if (TryCreateDataTypeExpressionForEnum(propertyType, isNullable))
            {
                return;
            }

            if (!TryCreateDataTypeExpressionForSystemType(propertyType, isNullable))
            {
                var err = new InvalidOperationException($"Unable to create data type expression for type '{propertyType}'");
                //_logger.LogError(err, err.Message, null);
                throw err;
            }
        }

        private bool TryCreateDataTypeExpressionForSystemType(Type propertyType, bool isNullable)
        {
            switch (propertyType)
            {
                case not null when propertyType == typeof(byte):
                    CreateDataTypeExpressionIfNotCompatible(() => new ByteExpression(isNullable));
                    break;

                case not null when propertyType == typeof(sbyte):
                    CreateDataTypeExpressionIfNotCompatible(() => new SByteExpression(isNullable));
                    break;

                case not null when propertyType == typeof(ushort):
                    CreateDataTypeExpressionIfNotCompatible(() => new UnsignedShortExpression(isNullable));
                    break;

                case not null when propertyType == typeof(short):
                    CreateDataTypeExpressionIfNotCompatible(() => new ShortExpression(isNullable));
                    break;

                case not null when propertyType == typeof(uint):
                    CreateDataTypeExpressionIfNotCompatible(() => new UnsignedIntegerExpression(isNullable));
                    break;

                case not null when propertyType == typeof(int):
                    CreateDataTypeExpressionIfNotCompatible(() => new IntegerExpression(isNullable));
                    break;

                case not null when propertyType == typeof(ulong):
                    CreateDataTypeExpressionIfNotCompatible(() => new UnsignedLongExpression(isNullable));
                    break;

                case not null when propertyType == typeof(long):
                    CreateDataTypeExpressionIfNotCompatible(() => new LongExpression(isNullable));
                    break;

                case not null when propertyType == typeof(string):
                    CreateDataTypeExpressionIfNotCompatible(() => new StringExpression());
                    break;

                case not null when propertyType == typeof(DateTime):
                    CreateDataTypeExpressionIfNotCompatible(() => new DateTimeExpression(isNullable));
                    break;

                case not null when propertyType == typeof(bool):
                    CreateDataTypeExpressionIfNotCompatible(() => new BooleanExpression());
                    break;

                case not null when propertyType == typeof(TimeSpan):
                    CreateDataTypeExpressionIfNotCompatible(() => new TimeSpanValueExpression(isNullable));
                    break;

                case not null when propertyType == typeof(decimal):
                    CreateDataTypeExpressionIfNotCompatible(() => new DecimalExpression(isNullable));
                    break;

                case not null when propertyType == typeof(float):
                    CreateDataTypeExpressionIfNotCompatible(() => new FloatExpression(isNullable));
                    break;

                case not null when propertyType == typeof(double):
                    CreateDataTypeExpressionIfNotCompatible(() => new DoubleExpression(isNullable));
                    break;

                default:
                    return false;
            }

            return true;
        }

        private bool TryCreateDataTypeExpressionForEnum(Type propertyType, bool isNullable)
        {
            if (!propertyType.IsEnum)
            {
                return false;
            }

            if (DataTypeExpression is null)
            {
                return true;
            }

            var enumExpressionGenericType = typeof(EnumExpression<>).MakeGenericType(propertyType);
            if (enumExpressionGenericType.IsAssignableFrom(DataTypeExpression.GetType())
                && (DataTypeExpression as NullableDataTypeExpression)?.IsNullable == isNullable)
            {
                return true;
            }

            var constructorInfo = enumExpressionGenericType.GetConstructor(TypeArray.From<bool>());

            var dataTypeExpression = (DataTypeExpression?)constructorInfo?.Invoke(new object[] { isNullable });
            if (dataTypeExpression is null)
            {
                var err = new InvalidOperationException($"Cannot create data type expression for enum '{propertyType.Name}'");
                //_logger.LogError(err, err.Message, null);
                throw err;
            }

            DataTypeExpression = dataTypeExpression;

            return true;
        }

        private void CreateDataTypeExpressionIfNotCompatible<TDataExpression>(Func<TDataExpression> createFunc)
            where TDataExpression : DataTypeExpression
        {
            var dataTypeExpression = DataTypeExpression;

            switch (dataTypeExpression)
            {
                case TDataExpression _ when dataTypeExpression is not NullableDataTypeExpression || !typeof(NullableDataTypeExpression).IsAssignableFrom(typeof(TDataExpression)):
                    return;

                case TDataExpression _:
                    var oldDataTypeExpression = (NullableDataTypeExpression)dataTypeExpression;
                    if (createFunc() is not NullableDataTypeExpression newDataTypeExpression)
                    {
                        return;
                    }

                    if (newDataTypeExpression.IsNullable != oldDataTypeExpression.IsNullable)
                    {
                        DataTypeExpression = newDataTypeExpression;
                    }

                    return;
            }

            DataTypeExpression = createFunc();
        }

        private void OnDataTypeExpressionPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            RaiseUpdated();
        }

        public override bool CalculateResult(object entity)
        {
            if (!IsValid)
            {
                return true;
            }

            if (DataTypeExpression is null)
            {
                return true;
            }

            return Property is null || DataTypeExpression.CalculateResult(Property, entity);
        }

        public override string ToString()
        {
            var property = Property;
            if (property is null)
            {
                return string.Empty;
            }

            var dataTypeExpression = DataTypeExpression;
            if (dataTypeExpression is null)
            {
                return string.Empty;
            }

            var dataTypeExpressionString = dataTypeExpression.ToString();
            return $"{property.DisplayName} {dataTypeExpressionString}";
        }

        public object Clone()
        {
            var clone = new PropertyExpression();
            if (this.Property != null)
            {
                var ownerType = this.Property.OwnerType;
                var propertyInfo = ownerType.GetProperty(this.Property.Name);
                if (propertyInfo != null)
                {
                    clone.Property = new PropertyMetadata(ownerType, propertyInfo);

                    clone.DataTypeExpression.SelectedCondition = this.DataTypeExpression.SelectedCondition;
                    clone.DataTypeExpression.ValueControlType = this.DataTypeExpression.ValueControlType;
                    clone.DataTypeExpression.IsValueRequired = this.DataTypeExpression.IsValueRequired;

                    if (clone.DataTypeExpression is BooleanExpression boolExp)
                    {
                        boolExp.Value = (this.DataTypeExpression as BooleanExpression).Value;
                    }
                    else if (clone.DataTypeExpression is TimeSpanExpression timespanExp)
                    {
                        timespanExp.Value = (this.DataTypeExpression as TimeSpanExpression).Value;
                    }
                    else if (clone.DataTypeExpression is StringExpression stringExp)
                    {
                        stringExp.Value = (this.DataTypeExpression as StringExpression).Value;
                    }
                    else if (clone.DataTypeExpression is ByteExpression byteExp)
                    {
                        byteExp.Value = (this.DataTypeExpression as ByteExpression).Value;
                    }
                    else if (clone.DataTypeExpression is ShortExpression shortExp)
                    {
                        shortExp.Value = (this.DataTypeExpression as ShortExpression).Value;
                    }
                    else if (clone.DataTypeExpression is UnsignedShortExpression ushortExp)
                    {
                        ushortExp.Value = (this.DataTypeExpression as UnsignedShortExpression).Value;
                    }
                    else if (clone.DataTypeExpression is IntegerExpression intExp)
                    {
                        intExp.Value = (this.DataTypeExpression as IntegerExpression).Value;
                    }
                    else if (clone.DataTypeExpression is UnsignedIntegerExpression uintExp)
                    {
                        uintExp.Value = (this.DataTypeExpression as UnsignedIntegerExpression).Value;
                    }
                    else if (clone.DataTypeExpression is LongExpression longExp)
                    {
                        longExp.Value = (this.DataTypeExpression as LongExpression).Value;
                    }
                    else if (clone.DataTypeExpression is UnsignedLongExpression ulongExp)
                    {
                        ulongExp.Value = (this.DataTypeExpression as UnsignedLongExpression).Value;
                    }
                    else if (clone.DataTypeExpression is FloatExpression floatExp)
                    {
                        floatExp.Value = (this.DataTypeExpression as FloatExpression).Value;
                    }
                    else if (clone.DataTypeExpression is DoubleExpression doubleExp)
                    {
                        doubleExp.Value = (this.DataTypeExpression as DoubleExpression).Value;
                    }
                    else if (clone.DataTypeExpression is DecimalExpression decimalExp)
                    {
                        decimalExp.Value = (this.DataTypeExpression as DecimalExpression).Value;
                    }
                    else if (clone.DataTypeExpression is SByteExpression sbyteExp)
                    {
                        sbyteExp.Value = (this.DataTypeExpression as SByteExpression).Value;
                    }
                    else if (clone.DataTypeExpression is DateTimeExpression dateTimeExp)
                    {
                        dateTimeExp.Value = (this.DataTypeExpression as DateTimeExpression).Value;
                    }
                    else if (clone.Property.Type.IsEnum)
                    {
                        try
                        {
                            var sourceExpType = this.DataTypeExpression.GetType();
                            var targetExpType = clone.DataTypeExpression.GetType();
                            object val = sourceExpType.GetProperty(this.Property.Name).GetValue(this.DataTypeExpression);
                            targetExpType.GetProperty(clone.Property.Name).SetValue(clone.DataTypeExpression, val);
                        }
                        catch { }
                    }
                }
            }
            return clone;
        }
    }
}
