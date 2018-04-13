using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;


namespace VDS.Mapping.Converters
{
    internal sealed class ValueConverterCollection
    {
        private readonly MappingContainer _container;
        private readonly IList<ValueConverter> _converters = new List<ValueConverter>();
        private bool _readonly;
        
        private readonly ConcurrentDictionary<Pair<Type, Type>, ValueConverter> _resolvedConverters =
            new ConcurrentDictionary<Pair<Type, Type>, ValueConverter>();

        internal ValueConverter Get(Type sourceType, Type targetType)
        {
            if (sourceType == null)
            {
                throw new ArgumentNullException(nameof(sourceType));
            }
            if (targetType == null)
            {
                throw new ArgumentNullException(nameof(targetType));
            }
            return _resolvedConverters.GetOrAdd(Pair.Create(sourceType, targetType), key => Find(new ConverterMatchContext(key.First, key.Second)));
        }

        public ValueConverterCollection(MappingContainer container)
        {
            _container = container;
        }

        private void CheckReadOnly()
        {
            if (_readonly)
            {
                throw new NotSupportedException("The collection is read-only.");
            }
        }

        internal void SetReadOnly()
        {
            if (!_readonly)
            {
                _readonly = true;
            }
        }

        internal void Add(ValueConverter converter)
        {
            CheckReadOnly();
            if (converter == null)
            {
                throw new ArgumentNullException(nameof(converter));
            }
            converter.Container = _container;
            _converters.Add(converter);
            _resolvedConverters.Clear();
        }

        public void Add<TSource, TTarget>(Func<TSource, TTarget> expression)
        {
            Add(new LambdaValueConverter<TSource, TTarget>(expression));
        }

        internal void Compile(ModuleBuilder builder)
        {
            foreach (var converter in _converters)
            {
                converter.Compile(builder);
            }
        }

        internal void AddIntrinsic<TSource, TTarget>(Func<TSource, TTarget> expression)
        {
            Add(new LambdaValueConverter<TSource, TTarget>(expression) { Intrinsic = true });
        }

        internal ValueConverter Get<TSource, TTarget>()
        {
            return Get(typeof(TSource), typeof(TTarget));
        }

        internal ValueConverter Find(ConverterMatchContext context)
        {
            return (from converter in _converters
                    let score = converter.Match(context)
                    where score >= 0
                    orderby score, converter.Intrinsic ? 1 : 0
                    select converter).FirstOrDefault();
        }
    }
}
