using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using VDS.Helpers.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace VDS.Dependency.Registration
{
    public class BasedOnDescriptor : IRegistrar
    {
        private readonly List<Type> _potentialBases;
	    private ServiceLifetime _lifetimeConfiguration = ServiceLifetime.Transient;
        private readonly FromDescriptor _from;
        private readonly ServiceDescriptor _service;
        private Predicate<Type> _ifFilter;
        private Predicate<Type> _unlessFilter;

        internal BasedOnDescriptor(IEnumerable<Type> basedOn, FromDescriptor from, Predicate<Type> additionalFilters)
        {
            _potentialBases = basedOn.ToList();
            _from = from;
            _service = new ServiceDescriptor(this);
            If(additionalFilters);
        }

        /// <summary>
		///   Gets the service descriptor.
		/// </summary>
		public ServiceDescriptor WithService => _service;

	    /// <summary>
        ///   Allows a type to be registered multiple times.
        /// </summary>
        public FromDescriptor AllowMultipleMatches()
        {
            return _from.AllowMultipleMatches();
        }

        /// <summary>
        ///   Returns the descriptor for accepting a new type.
        /// </summary>
        /// <typeparam name = "T"> The base type. </typeparam>
        /// <returns> The descriptor for the type. </returns>
        [Obsolete("Calling this method resets registration. If that's what you want, start anew, with Classes.FromAssembly..")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public BasedOnDescriptor BasedOn<T>()
        {
            return _from.BasedOn<T>();
        }

        /// <summary>
        ///   Returns the descriptor for accepting a new type.
        /// </summary>
        /// <param name = "basedOn"> The base type. </param>
        /// <returns> The descriptor for the type. </returns>
        [Obsolete("Calling this method resets registration. If that's what you want, start anew, with Classes.FromAssembly...")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public BasedOnDescriptor BasedOn(Type basedOn)
        {
            return _from.BasedOn(basedOn);
        }

        /// <summary>
        ///   Adds another type to be accepted as base.
        /// </summary>
        /// <param name = "basedOn"> The base type. </param>
        /// <returns> The descriptor for the type. </returns>
        public BasedOnDescriptor OrBasedOn(Type basedOn)
        {
            _potentialBases.Add(basedOn);
            return this;
        }
        
        /// <summary>
        ///   Assigns a conditional predication which must be satisfied.
        /// </summary>
        /// <param name = "ifFilter"> The predicate to satisfy. </param>
        /// <returns> </returns>
        public BasedOnDescriptor If(Predicate<Type> ifFilter)
        {
            _ifFilter += ifFilter;
            return this;
        }

        /// <summary>
        ///   Assigns a conditional predication which must not be satisfied.
        /// </summary>
        /// <param name = "unlessFilter"> The predicate not to satisify. </param>
        /// <returns> </returns>
        public BasedOnDescriptor Unless(Predicate<Type> unlessFilter)
        {
            _unlessFilter += unlessFilter;
            return this;
        }

        /// <summary>
        ///   Returns the descriptor for accepting a type based on a condition.
        /// </summary>
        /// <param name = "accepted"> The accepting condition. </param>
        /// <returns> The descriptor for the type. </returns>
        [Obsolete("Calling this method resets registration. If that's what you want, start anew, with Classes.FromAssembly..."
            )]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public BasedOnDescriptor Where(Predicate<Type> accepted)
        {
            return _from.Where(accepted);
        }

        /// <summary>
        ///   Uses all interfaces implemented by the type (or its base types) as well as their base interfaces.
        /// </summary>
        /// <returns> </returns>
        public BasedOnDescriptor WithServiceAllInterfaces()
        {
            return WithService.AllInterfaces();
        }

        /// <summary>
        ///   Uses the base type matched on.
        /// </summary>
        /// <returns> </returns>
        public BasedOnDescriptor WithServiceBase()
        {
            return WithService.Base();
        }

        /// <summary>
        ///   Uses all interfaces that have names matched by implementation type name.
        ///   Matches Foo to IFoo, SuperFooExtended to IFoo and IFooExtended etc
        /// </summary>
        /// <returns> </returns>
        public BasedOnDescriptor WithServiceDefaultInterfaces()
        {
            return WithService.DefaultInterfaces();
        }

        /// <summary>
        ///   Uses the first interface of a type. This method has non-deterministic behavior when type implements more than one interface!
        /// </summary>
        /// <returns> </returns>
        public BasedOnDescriptor WithServiceFirstInterface()
        {
            return WithService.FirstInterface();
        }

        /// <summary>
        ///   Uses <paramref name = "implements" /> to lookup the sub interface.
        ///   For example: if you have IService and 
        ///   IProductService : ISomeInterface, IService, ISomeOtherInterface.
        ///   When you call FromInterface(typeof(IService)) then IProductService
        ///   will be used. Useful when you want to register _all_ your services
        ///   and but not want to specify all of them.
        /// </summary>
        /// <param name = "implements"> </param>
        /// <returns> </returns>
        public BasedOnDescriptor WithServiceFromInterface(Type implements)
        {
            return WithService.FromInterface(implements);
        }

        /// <summary>
        ///   Uses base type to lookup the sub interface.
        /// </summary>
        /// <returns> </returns>
        public BasedOnDescriptor WithServiceFromInterface()
        {
            return WithService.FromInterface();
        }

        /// <summary>
        ///   Assigns a custom service selection strategy.
        /// </summary>
        /// <param name = "selector"> </param>
        /// <returns> </returns>
        public BasedOnDescriptor WithServiceSelect(ServiceDescriptor.ServiceSelector selector)
        {
            return WithService.Select(selector);
        }

        /// <summary>
        ///   Uses the type itself.
        /// </summary>
        /// <returns> </returns>
        public BasedOnDescriptor WithServiceSelf()
        {
            return WithService.Self();
        }

		/// <summary>
		///   Sets component lifestyle to scoped per explicit scope.
		/// </summary>
		/// <returns> </returns>
		public BasedOnDescriptor LifestyleScoped()
        {
            _lifetimeConfiguration = ServiceLifetime.Scoped;
            return this;
        }

        /// <summary>
        ///   Sets component lifestyle to singleton.
        /// </summary>
        /// <returns> </returns>
        public BasedOnDescriptor LifestyleSingleton()
        {
            _lifetimeConfiguration = ServiceLifetime.Singleton;
            return this;
        }

        /// <summary>
        ///   Sets component lifestyle to transient.
        /// </summary>
        /// <returns> </returns>
        public BasedOnDescriptor LifestyleTransient()
        {
            _lifetimeConfiguration = ServiceLifetime.Transient;
            return this;
        }

        /// <summary>
        ///   Assigns the supplied service types.
        /// </summary>
        /// <param name = "types"> </param>
        /// <returns> </returns>
        public BasedOnDescriptor WithServices(IEnumerable<Type> types)
        {
            return WithService.Select(types);
        }

        /// <summary>
        ///   Assigns the supplied service types.
        /// </summary>
        /// <param name = "types"> </param>
        /// <returns> </returns>
        public BasedOnDescriptor WithServices(params Type[] types)
        {
            return WithService.Select(types);
        }

        protected virtual bool Accepts(Type type, out Type[] baseTypes)
        {
            return IsBasedOn(type, out baseTypes)
                   && ExecuteIfCondition(type)
                   && ExecuteUnlessCondition(type) == false;
        }

        protected bool ExecuteIfCondition(Type type)
        {
            if (_ifFilter == null)
            {
                return true;
            }

            return _ifFilter.GetInvocationList().Cast<Predicate<Type>>().All(filter => filter(type));
        }

        protected bool ExecuteUnlessCondition(Type type)
        {
            if (_unlessFilter == null)
            {
                return false;
            }
            return _unlessFilter.GetInvocationList().Cast<Predicate<Type>>().Any(filter => filter(type));
        }

        protected bool IsBasedOn(Type type, out Type[] baseTypes)
        {
            var actuallyBasedOn = new List<Type>();
            foreach (var potentialBase in _potentialBases)
            {
                if (potentialBase.GetTypeInfo().IsAssignableFrom(type))
                {
                    actuallyBasedOn.Add(potentialBase);
                }
                else if (potentialBase.GetTypeInfo().IsGenericTypeDefinition)
                {
                    if (potentialBase.GetTypeInfo().IsInterface)
                    {
                        if (IsBasedOnGenericInterface(type, potentialBase, out baseTypes))
                        {
                            actuallyBasedOn.AddRange(baseTypes);
                        }
                    }

                    if (IsBasedOnGenericClass(type, potentialBase, out baseTypes))
                    {
                        actuallyBasedOn.AddRange(baseTypes);
                    }
                }
            }
            baseTypes = actuallyBasedOn.Distinct().ToArray();
            return baseTypes.Length > 0;
        }

	    internal bool TryRegister(Type type, IServiceCollection services)
	    {
		    if (!Accepts(type, out var baseTypes))
		    {
			    return false;
		    }

		    var serviceTypes = _service.GetServices(type, baseTypes);
		    if (serviceTypes.Count == 0)
		    {
		        serviceTypes = Type.EmptyTypes;
		    }

		    var serviceDescriptors = serviceTypes.Select(
			    s => new Microsoft.Extensions.DependencyInjection.ServiceDescriptor(s, type, _lifetimeConfiguration));

			serviceDescriptors.ForEach(services.Add);
		    return true;
	    }


		private static bool IsBasedOnGenericClass(Type type, Type basedOn, out Type[] baseTypes)
        {
            while (type != null)
            {
                if (type.GetTypeInfo().IsGenericType &&
                    type.GetGenericTypeDefinition() == basedOn)
                {
                    baseTypes = new[] { type };
                    return true;
                }

                type = type.GetTypeInfo().BaseType;
            }
            baseTypes = null;
            return false;
        }

        private static bool IsBasedOnGenericInterface(Type type, Type basedOn, out Type[] baseTypes)
        {
            var types = new List<Type>(4);
            foreach (var @interface in type.GetInterfaces())
            {
                if (@interface.GetTypeInfo().IsGenericType &&
                    @interface.GetGenericTypeDefinition() == basedOn)
                {
                    if (@interface.DeclaringType == null &&
                        @interface.GetTypeInfo().ContainsGenericParameters)
                    {
                        types.Add(@interface.GetGenericTypeDefinition());
                    }
                    else
                    {
                        types.Add(@interface);
                    }
                }
            }
            baseTypes = types.ToArray();
            return baseTypes.Length > 0;
        }

        void IRegistrar.Register(IServiceCollection services)
        {
            ((IRegistrar)_from).Register(services);
        }
    }
}