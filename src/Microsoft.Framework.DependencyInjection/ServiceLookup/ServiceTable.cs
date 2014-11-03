// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Framework.DependencyInjection.ServiceLookup
{
    internal class ServiceTable
    {
        private readonly object _sync = new object();

        private readonly Dictionary<Type, ServiceEntry> _services;
        private readonly Dictionary<Type, List<IGenericService>> _genericServices;
        private readonly ConcurrentDictionary<Type, Func<ServiceProvider, object>> _realizedServices = new ConcurrentDictionary<Type, Func<ServiceProvider, object>>();

        public ServiceTable(IEnumerable<IServiceDescriptor> descriptors)
        {
            _services = new Dictionary<Type, ServiceEntry>();
            _genericServices = new Dictionary<Type, List<IGenericService>>();

            // For each service type
            foreach (var serviceType in descriptors.Select(d => d.ServiceType).Distinct())
            {
                // REVIEW: whether this can be reused
                var serviceTypeDescriptors = descriptors.Where(d => d.ServiceType == serviceType);

                // Look for override single and stop
                var overrideSingle = serviceTypeDescriptors.Where(d => d.OverrideMode == OverrideMode.OverrideSingle).LastOrDefault();
                if (overrideSingle != null)
                {
                    Add(overrideSingle);
                    continue;
                }

                // Override Many and stop if any found
                bool foundAny = false;
                foreach (var descriptor in serviceTypeDescriptors.Where(d => d.OverrideMode == OverrideMode.OverrideMany))
                {
                    foundAny = true;
                    Add(descriptor);
                }
                if (foundAny)
                {
                    continue;
                }

                // Look for DefaultSingle and stop
                var defaultSingle = serviceTypeDescriptors.Where(d => d.OverrideMode == OverrideMode.DefaultSingle).LastOrDefault();
                if (defaultSingle != null)
                {
                    Add(defaultSingle);
                    continue;
                }

                // Finally add any DefaultMany
                var defaultMany = serviceTypeDescriptors.Where(d => d.OverrideMode == OverrideMode.DefaultMany);
                foreach (var descriptor in defaultMany)
                {
                    Add(descriptor);
                }
            }
        }

        private void Add(IServiceDescriptor descriptor)
        {
            var serviceTypeInfo = descriptor.ServiceType.GetTypeInfo();
            if (serviceTypeInfo.IsGenericTypeDefinition)
            {
                Add(descriptor.ServiceType, new GenericService(descriptor));
            }
            else if (descriptor.ImplementationInstance != null)
            {
                Add(descriptor.ServiceType, new InstanceService(descriptor));
            }
            else if (descriptor.ImplementationFactory != null)
            {
                Add(descriptor.ServiceType, new FactoryService(descriptor));
            }
            else
            {
                Add(descriptor.ServiceType, new Service(descriptor));
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
