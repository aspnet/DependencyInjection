using System;
using System.Diagnostics;

namespace Microsoft.Extensions.DependencyInjection
{
    [DebuggerDisplay("Lifetime = {Lifetime}, ServiceType = {ServiceType}, ImplementationType = {ImplementationType}")]
    public class TypeServiceDescriptor : ServiceDescriptor
    {
        /// <summary>
        /// Initializes a new instance of <see cref="TypeServiceDescriptor"/> with the specified <paramref name="implementationType"/>.
        /// </summary>
        /// <param name="serviceType">The <see cref="Type"/> of the service.</param>
        /// <param name="implementationType">The <see cref="Type"/> implementing the service.</param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the service.</param>
        public TypeServiceDescriptor(Type serviceType, Type implementationType, ServiceLifetime lifetime)
            : base(serviceType, lifetime)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (implementationType == null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }

            ImplementationType = implementationType;
        }

        /// <inheritdoc />
        public Type ImplementationType { get; }

        internal override Type GetImplementationType()
        {
            return ImplementationType;
        }

        public static TypeServiceDescriptor Transient<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            return new TypeServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Transient);
        }

        public static TypeServiceDescriptor Transient(Type service, Type implementationType)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (implementationType == null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }

            return new TypeServiceDescriptor(service, implementationType, ServiceLifetime.Transient);
        }

        public static TypeServiceDescriptor Scoped<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            return new TypeServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped);
        }

        public static TypeServiceDescriptor Scoped(Type service, Type implementationType)
        {
            return new TypeServiceDescriptor(service, implementationType, ServiceLifetime.Scoped);
        }

        public static TypeServiceDescriptor Singleton<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            return new TypeServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton);
        }

        public static TypeServiceDescriptor Singleton(Type service, Type implementationType)
        {
            if (service == null)
            {
                throw new ArgumentNullException(nameof(service));
            }

            if (implementationType == null)
            {
                throw new ArgumentNullException(nameof(implementationType));
            }

            return new TypeServiceDescriptor(service, implementationType, ServiceLifetime.Singleton);
        }
    }
}