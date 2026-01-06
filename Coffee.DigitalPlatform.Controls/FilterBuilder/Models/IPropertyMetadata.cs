using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Coffee.DigitalPlatform.Controls.FilterBuilder
{
    public interface IPropertyMetadata
    {
        string DisplayName { get; set; }

        string Name { get; }

        Type OwnerType { get; }

        Type Type { get; }

        object? GetValue(object instance);

        TValue? GetValue<TValue>(object instance);

        void SetValue(object instance, object? value);
    }

    public class PropertyMetadata : IPropertyMetadata
    {
        private readonly PropertyInfo? _propertyInfo;
        private string? _displayName;

        public PropertyMetadata(Type ownerType, PropertyInfo propertyInfo)
        {
            ArgumentNullException.ThrowIfNull(ownerType);
            ArgumentNullException.ThrowIfNull(propertyInfo);

            _propertyInfo = propertyInfo;

            OwnerType = ownerType;
            Name = propertyInfo.Name;
            DisplayName = propertyInfo.GetDisplayName() ?? Name;
            Type = propertyInfo.PropertyType;
        }

        public string DisplayName
        {
            get => _displayName ?? Name;
            set => _displayName = value;
        }

        public string Name { get; }

        public Type OwnerType { get; }

        public Type Type { get; }

        private bool Equals(PropertyMetadata other)
        {
            return string.Equals(Name, other.Name) && Type == other.Type;
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
                   && Equals((PropertyMetadata)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Name.GetHashCode();
                hashCode = (hashCode * 397) ^ Type.GetHashCode();
                return hashCode;
            }
        }

        public object? GetValue(object instance)
        {
            return GetValue<object?>(instance);
        }

        public TValue? GetValue<TValue>(object instance)
        {
            ArgumentNullException.ThrowIfNull(instance);

            object? value = null;

            if (_propertyInfo is not null)
            {
                value = _propertyInfo.GetValue(instance, null);
            }

            if (value is null)
            {
                return default;
            }

            if (typeof(TValue) == typeof(string))
            {
                value = ObjectToStringHelper.ToString(value);
            }

            return (TValue)value;
        }

        public void SetValue(object instance, object? value)
        {
            ArgumentNullException.ThrowIfNull(instance);

            _propertyInfo?.SetValue(instance, value, null);
        }
    }
}
