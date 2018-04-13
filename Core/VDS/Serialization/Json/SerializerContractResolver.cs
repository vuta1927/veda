using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using VDS.Helpers.Extensions;
using VDS.Reflection.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace VDS.Serialization.Json
{
    /// <summary>
    /// Represents a <see cref="IContractResolver"/> based on the <see cref="DefaultContractResolver"/> for resolving contracts for serialization
    /// </summary>
    public class SerializerContractResolver : DefaultContractResolver
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ISerializationOptions _options;

        /// <summary>
        /// Initializes a new instance of <see cref="SerializerContractResolver"/>
        /// </summary>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> to use for creating instances of types</param>
        /// <param name="options"><see cref="ISerializationOptions"/> to use during resolving</param>
        public SerializerContractResolver(IServiceProvider serviceProvider, ISerializationOptions options)
        {
            _serviceProvider = serviceProvider;
            _options = options;
        }
        
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            var properties = base.CreateProperties(type, memberSerialization);
            if( _options != null )
                return properties.Where(p => _options.ShouldSerializeProperty(type, p.PropertyName)).ToList();

            return properties;
        }

        public override JsonContract ResolveContract(Type type)
        {
            var contract = base.ResolveContract(type);
        
            if (contract is JsonObjectContract && 
                !type.GetTypeInfo().IsValueType &&
                !type.HasDefaultConstructor())
            {
                var defaultCreator = contract.DefaultCreator;
                contract.DefaultCreator = () =>
                                              {
                                                  try
                                                  {
                                                    // Todo: Structs without default constructor will fail with this and that will then try using the defaultCreator in the catch
                                                      return _serviceProvider.GetService(type);
                                                  }
                                                  catch
                                                  {
                                                      if (defaultCreator != null)
                                                        return defaultCreator();
                                                      return null;
                                                  }
                                              };
            }

            return contract;
        }

        protected override string ResolvePropertyName(string propertyName)
        {
            var result = base.ResolvePropertyName(propertyName);
            if (_options != null && _options.Flags.HasFlag(SerializationOptionsFlags.UseCamelCase))
            {
                result = result.ToCamelCase();
            }

            return result;
        }
    }
}