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
                    var services = new List<IService>();
                    foreach (var childDescriptor in enumerableDescriptor.Descriptors)
                    {
                        IService service;
                        if (TryCreateService(childDescriptor, out service))
                        {
                            services.Add(service);
                        }
                    }

                    Add(typeof(IEnumerable<>).MakeGenericType(enumerableDescriptor.ServiceType),
                        new ClosedIEnumerableService(enumerableDescriptor.ServiceType,services));
                }
                else
                {
                    if (serviceTypeInfo.IsGenericTypeDefinition)
                    {
                        var genericService = CreateGenericService(descriptor);
                        Add(descriptor.ServiceType, genericService);
                    }
                    else
                    {
                        IService service;
                        if (TryCreateService(descriptor, out service))
                        {
                            Add(descriptor.ServiceType, service);
                        }
                    }
                }
            }
        }

        private static GenericService CreateGenericService(ServiceDescriptor descriptor)
        {
            var typeServiceDescriptor = (descriptor as TypeServiceDescriptor);
            var implementationTypeInfo = typeServiceDescriptor?.ImplementationType?.GetTypeInfo();

            if (typeServiceDescriptor == null ||
                implementationTypeInfo == null ||
                !implementationTypeInfo.IsGenericTypeDefinition)
            {
                throw new ArgumentException(
                    Resources.FormatOpenGenericServiceRequiresOpenGenericImplementation(
                        descriptor.ServiceType),
                    "descriptors");
            }

            if (implementationTypeInfo.IsAbstract ||
                implementationTypeInfo.IsInterface)
            {
                throw new ArgumentException(
                    Resources.FormatTypeCannotBeActivated(
                        typeServiceDescriptor.ImplementationType,
                        typeServiceDescriptor.ServiceType));
            }

            return new GenericService(typeServiceDescriptor);
        }

        private bool TryCreateService(ServiceDescriptor descriptor, out IService service)
        {
            service = null;

            var instanceServiceDescriptor = descriptor as InstanceServiceDescriptor;
            if (instanceServiceDescriptor != null)
            {
                service = new InstanceService(instanceServiceDescriptor);
            }

            var factoryServiceDescriptor = descriptor as FactoryServiceDescriptor;
            if (factoryServiceDescriptor != null)
            {
                service = new FactoryService(factoryServiceDescriptor);
            }

            var typeServiceDescriptor = descriptor as TypeServiceDescriptor;
            if (typeServiceDescriptor != null)
            {
                var implementationTypeInfo = typeServiceDescriptor.ImplementationType.GetTypeInfo();

                if (implementationTypeInfo.IsGenericTypeDefinition ||
                    implementationTypeInfo.IsAbstract ||
                    implementationTypeInfo.IsInterface)
                {
                    throw new ArgumentException(
                        Resources.FormatTypeCannotBeActivated(
                            typeServiceDescriptor.ImplementationType,
                            typeServiceDescriptor.ServiceType));
                }

                service = new Service(typeServiceDescriptor);
            }

            return service != null;
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
                if (serviceType.GetTypeInfo().IsGenericType)
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
