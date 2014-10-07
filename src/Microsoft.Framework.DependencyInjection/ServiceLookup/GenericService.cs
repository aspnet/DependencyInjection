// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace Microsoft.Framework.DependencyInjection.ServiceLookup
{
    internal class GenericService : IGenericService
    {
        private readonly IServiceDescriptor _descriptor;
        private readonly TypeActivator _activator;

        public GenericService(IServiceDescriptor descriptor, TypeActivator activator)
        {
            _descriptor = descriptor;
            _activator = activator;
        }

        public LifecycleKind Lifecycle
        {
            get { return _descriptor.Lifecycle; }
        }

        public IService GetService(Type closedServiceType)
        {
            Type[] genericArguments = closedServiceType.GetTypeInfo().GenericTypeArguments;
            Type closedImplementationType =
                _descriptor.ImplementationType.MakeGenericType(genericArguments);

            var closedServiceDescriptor = new ServiceDescriptor
            {
                ServiceType = closedServiceType,
                ImplementationType = closedImplementationType,
                Lifecycle = Lifecycle
            };

            return new Service(closedServiceDescriptor, _activator);
        }
    }
}
