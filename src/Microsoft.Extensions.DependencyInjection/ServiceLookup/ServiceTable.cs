// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal class ServiceTable
    {
        private readonly object _sync = new object();

        private readonly Dictionary<Type, ServiceEntry> _services;
        private readonly Dictionary<Type, List<IGenericService>> _genericServices;
        private readonly ConcurrentDictionary<Type, Func<ServiceProvider, object>> _realizedServices = new ConcurrentDictionary<Type, Func<ServiceProvider, object>>();

        public ServiceTable(IEnumerable<ServiceDescriptor> descriptors)
        {
            _services = new Dictionary<Type, ServiceEntry>();
            _genericServices = new Dictionary<Type, List<IGenericService>>();

            foreach (var descriptor in descriptors)
            {
                var serviceTypeInfo = descriptor.ServiceType.GetTypeInfo();
                var enumerableDescriptor = descriptor as EnumerableServiceDescriptor;
                if (enumerableDescriptor != null)
                {
                    Add(typeof(IEnumerable<>).MakeGenericType(descriptor.ServiceType), new ClosedIEnumerableService(
                        descriptor.ServiceType,
                        //reverse to break assumptions
                        enumerableDescriptor.Descriptors.Select(CreateService).Reverse().ToArray()));
                }
                else
                {
                    if (serviceTypeInfo.IsGenericTypeDefinition)
                    {
                        var implementationTypeInfo = descriptor.ImplementationType?.GetTypeInfo();

                        if (implementationTypeInfo == null ||
                            !implementationTypeInfo.IsGenericTypeDefinition)
                        {
                            throw new ArgumentException(
                                Resources.FormatOpenGenericServiceRequiresOpenGenericImplementation(
                                    descriptor.ServiceType),
                                nameof(descriptors));
                        }

                        if (implementationTypeInfo.IsAbstract ||
                            implementationTypeInfo.IsInterface)
                        {
                            throw new ArgumentException(
                                Resources.FormatTypeCannotBeActivated(
                                    descriptor.ImplementationType,
                                    descriptor.ServiceType));
                        }

                        Add(descriptor.ServiceType, new GenericService(descriptor));
                    }
                    else
                    {
                        Add(descriptor.ServiceType, CreateService(descriptor));
                    }
                }
            }
        }

        private IService CreateService(ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationInstance != null)
            {
                return new InstanceService(descriptor);
            }
            else if (descriptor.ImplementationFactory != null)
            {
                return new FactoryService(descriptor);
            }
            else
            {
                Debug.Assert(descriptor.ImplementationType != null);
                var implementationTypeInfo = descriptor.ImplementationType.GetTypeInfo();

                if (implementationTypeInfo.IsGenericTypeDefinition ||
                    implementationTypeInfo.IsAbstract ||
                    implementationTypeInfo.IsInterface)
                {
                    throw new ArgumentException(
                        Resources.FormatTypeCannotBeActivated(
                            descriptor.ImplementationType,
                            descriptor.ServiceType));
                }

                return new Service(descriptor);
            }
        }

        public ConcurrentDictionary<Type, Func<ServiceProvider, object>> RealizedServices
        {
            get { return _realizedServices; }
        }

        public bool TryGetEntry(Type serviceType, out ServiceEntry entry)
        {
            lock (_sync)
            {
                if (_services.TryGetValue(serviceType, out entry))
                {
                    return true;
                }
                else if (serviceType.GetTypeInfo().IsGenericType)
                {
                    var openServiceType = serviceType.GetGenericTypeDefinition();

                    List<IGenericService> genericEntry;
                    if (_genericServices.TryGetValue(openServiceType, out genericEntry))
                    {
                        foreach (var genericService in genericEntry)
                        {
                            var closedService = genericService.GetService(serviceType);
                            if (closedService != null)
                            {
                                Add(serviceType, closedService);
                            }
                        }

                        return _services.TryGetValue(serviceType, out entry);
                    }
                }
            }
            return false;
        }

        public void Add(Type serviceType, IService service)
        {
            lock (_sync)
            {
                ServiceEntry entry;
                if (_services.TryGetValue(serviceType, out entry))
                {
                    entry.Add(service);
                }
                else
                {
                    _services[serviceType] = new ServiceEntry(service);
                }
            }
        }

        public void Add(Type serviceType, IGenericService genericService)
        {
            lock (_sync)
            {
                List<IGenericService> genericEntry;
                if (!_genericServices.TryGetValue(serviceType, out genericEntry))
                {
                    genericEntry = new List<IGenericService>();
                    _genericServices[serviceType] = genericEntry;
                }

                genericEntry.Add(genericService);
            }
        }
    }
}
