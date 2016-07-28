// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;

namespace Microsoft.Extensions.DependencyInjection
{
    [DebuggerDisplay("Lifetime = {Lifetime}, ServiceType = {ServiceType}, ImplementationFactory = {ImplementationFactory}")]
    public class FactoryServiceDescriptor : ServiceDescriptor
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ServiceDescriptor"/> with the specified <paramref name="factory"/>.
        /// </summary>
        /// <param name="serviceType">The <see cref="Type"/> of the service.</param>
        /// <param name="factory">A factory used for creating service instances.</param>
        /// <param name="lifetime">The <see cref="ServiceLifetime"/> of the service.</param>
        public FactoryServiceDescriptor(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime)
            : base(serviceType, lifetime)
        {
            if (serviceType == null)
            {
                throw new ArgumentNullException(nameof(serviceType));
            }

            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            ImplementationFactory = factory;
        }

        /// <inheritdoc />
        public Func<IServiceProvider, object> ImplementationFactory { get; }

        internal override Type GetImplementationType()
        {
            var typeArguments = ImplementationFactory.GetType().GenericTypeArguments;

            Debug.Assert(typeArguments.Length == 2);

            return typeArguments[1];
        }
    }
}