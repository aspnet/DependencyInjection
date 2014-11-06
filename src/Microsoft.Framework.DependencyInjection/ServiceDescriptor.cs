// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Framework.DependencyInjection
{
    public class ServiceDescriptor : IServiceDescriptor
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ServiceDescriptor"/> with the specified <paramref name="implementationType"/>.
        /// </summary>
        /// <param name="serviceType">The <see cref="Type"/> of the service.</param>
        /// <param name="implementationType">The <see cref="Type"/> implementing the service.</param>
        /// <param name="lifecycle">The <see cref="LifecycleKind"/> of the service.</param>
        /// <param name="mode">The <see cref="OverrideMode"/> of the service.</param>
        public ServiceDescriptor([NotNull] Type serviceType, 
                                 [NotNull] Type implementationType, 
                                 LifecycleKind lifecycle,
                                 bool isFallback = false)
            : this(serviceType, lifecycle, isFallback)
        {
            ImplementationType = implementationType;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ServiceDescriptor"/> with the specified <paramref name="instance"/>
        /// as a <see cref="LifecycleKind.Singleton"/>.
        /// </summary>
        /// <param name="serviceType">The <see cref="Type"/> of the service.</param>
        /// <param name="instance">The instance implementing the service.</param>
        public ServiceDescriptor([NotNull] Type serviceType,
                                 [NotNull] object instance,
                                 bool isFallback = false)
            : this(serviceType, LifecycleKind.Singleton, isFallback)
        {
            ImplementationInstance = instance;
        }

        /// <summary>
        /// Initializes a new instance of <see cref="ServiceDescriptor"/> with the specified <paramref name="factory"/>.
        /// </summary>
        /// <param name="serviceType">The <see cref="Type"/> of the service.</param>
        /// <param name="factory">A factory used for creating service instances.</param>
        /// <param name="lifecycle">The <see cref="LifecycleKind"/> of the service.</param>
        public ServiceDescriptor([NotNull] Type serviceType,
                                 [NotNull] Func<IServiceProvider, object> factory,
                                 LifecycleKind lifecycle,
                                 bool isFallback = false)
            : this(serviceType, lifecycle, isFallback)
        {
            ImplementationFactory = factory;
        }

        private ServiceDescriptor(Type serviceType, LifecycleKind lifecycle, bool isFallback)
        {
            Lifecycle = lifecycle;
            ServiceType = serviceType;
            IsFallback = isFallback;
        }

        /// <inheritdoc />
        public LifecycleKind Lifecycle { get; private set; }

        /// <inheritdoc />
        public Type ServiceType { get; private set; }

        /// <inheritdoc />
        public Type ImplementationType { get; private set; }

        /// <inheritdoc />
        public object ImplementationInstance { get; private set; }

        /// <inheritdoc />
        public Func<IServiceProvider, object> ImplementationFactory { get; private set; }

        /// <inheritdoc />
        public bool IsFallback { get; private set; }
    }
}