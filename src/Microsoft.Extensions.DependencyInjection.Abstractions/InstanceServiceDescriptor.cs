// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Microsoft.Extensions.DependencyInjection
{
    [DebuggerDisplay("Lifetime = {Lifetime}, ServiceType = {ServiceType}, ImplementationInstance = {ImplementationInstance}")]
    public class InstanceServiceDescriptor : ServiceDescriptor
    {
        /// <summary>
        /// Initializes a new instance of <see cref="InstanceServiceDescriptor"/> with the specified <paramref name="instance"/>
        /// as a <see cref="ServiceLifetime.Singleton"/>.
        /// </summary>
        /// <param name="serviceType">The <see cref="Type"/> of the service.</param>
        /// <param name="instance">The instance implementing the service.</param>
        public InstanceServiceDescriptor(Type serviceType, object instance)
            : base(serviceType, ServiceLifetime.Singleton)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (instance == null)
            {
                throw new ArgumentNullException(nameof(instance));
            }

            ImplementationInstance = instance;
        }

        /// <inheritdoc />
        public object ImplementationInstance { get; }

        internal override Type GetImplementationType()
        {
            return ImplementationInstance.GetType();
        }

        public static InstanceServiceDescriptor Singleton<TService>(TService implementationInstance)
            where TService : class
        {
            if (implementationInstance == null)
            {
                throw new ArgumentNullException(nameof(implementationInstance));
            }

            return Singleton(typeof(TService), implementationInstance);
        }

        public static InstanceServiceDescriptor Singleton(
            Type serviceType,
            object implementationInstance)
        {
            return new InstanceServiceDescriptor(serviceType, implementationInstance);
        }
    }
}