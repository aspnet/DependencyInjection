// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Microsoft.Framework.DependencyInjection
{
    [DebuggerDisplay("Lifetime = {Lifetime}, ServiceType = {ServiceType}, ImplementationType = {ImplementationType}")]
    public class ServiceDescriptor
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ServiceDescriptor"/> with the specified <paramref name="implementationType"/>.
        /// </summary>
        /// <param name="serviceType">The <see cref="Type"/> of the service.</param>
        /// <param name="implementationType">The <see cref="Type"/> implementing the service.</param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the service.</param>
        public ServiceDescriptor([NotNull] Type serviceType, 
                                 [NotNull] Type implementationType, 
                                 ServiceLifetime lifetime)
            : this(serviceType, lifetime)
        {
            ImplementationType = implementationType;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ServiceDescriptor"/> with the specified <paramref name="instance"/>
        /// as a <see cref="ServiceLifetime.Singleton"/>.
        /// </summary>
        /// <param name="serviceType">The <see cref="Type"/> of the service.</param>
        /// <param name="instance">The instance implementing the service.</param>
        public ServiceDescriptor([NotNull] Type serviceType,
                                 [NotNull] object instance)
            : this(serviceType, ServiceLifetime.Singleton)
        {
            ImplementationInstance = instance;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ServiceDescriptor"/> with the specified <paramref name="factory"/>.
        /// </summary>
        /// <param name="serviceType">The <see cref="Type"/> of the service.</param>
        /// <param name="factory">A factory used for creating service instances.</param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the service.</param>
        public ServiceDescriptor([NotNull] Type serviceType,
                                 [NotNull] Func<IServiceProvider, object> factory, 
                                 ServiceLifetime lifetime)
            : this(serviceType, lifetime)
        {
            ImplementationFactory = factory;
        }

        private ServiceDescriptor(Type serviceType, ServiceLifetime lifetime)
        {
            Lifetime = lifetime;
            ServiceType = serviceType;
        }

        /// <inheritdoc />
        public ServiceLifetime Lifetime { get; }

        /// <inheritdoc />
        public Type ServiceType { get; }

        /// <inheritdoc />
        public Type ImplementationType { get; }

        /// <inheritdoc />
        public object ImplementationInstance { get; }

        /// <inheritdoc />
        public Func<IServiceProvider, object> ImplementationFactory { get; }
    }
}
