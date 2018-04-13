using System;
using System.Collections.Generic;

namespace VDS.Mapping.Converters
{
    internal sealed class ConverterMatchContext
    {
        public ConverterMatchContext(Type sourceType, Type targetType)
        {
            SourceType = sourceType;
            TargetType = targetType;
        }

        public Type SourceType { get; }

        public Type TargetType { get; }

        private readonly IDictionary<object, object> _properties = new Dictionary<object, object>();

        public object GetProperty(object key)
        {
            return _properties.TryGetValue(key, out var value) ? value : null;
        }

        public void SetProperty(object key, object value)
        {
            _properties[key] = value;
        }
    }
}
