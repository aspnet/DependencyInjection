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
        private readonly List<ServiceDescriptor> _descriptors;
        private readonly Dictionary<Type, IServiceCallSite> _callSiteCache = new Dictionary<Type, IServiceCallSite>();
        public ConcurrentDictionary<Type, Func<ServiceProvider, object>> RealizedServices { get; } = new ConcurrentDictionary<Type, Func<ServiceProvider, object>>();

        public ServiceTable(IEnumerable<ServiceDescriptor> descriptors)
        {
            _descriptors = descriptors.ToList();
            Validate(descriptors);
        }

        private void Validate(IEnumerable<ServiceDescriptor> descriptors)
        {
            foreach (var descriptor in descriptors)
            {
                var serviceTypeInfo = descriptor.ServiceType.GetTypeInfo();
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
                }
                else if (descriptor.ImplementationInstance == null && descriptor.ImplementationFactory == null)
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
                }
            }
        }

        internal IServiceCallSite GetCallSite(Type serviceType, ISet<Type> callSiteChain)
        {
            if (_callSiteCache.TryGetValue(serviceType, out var cachedCallSite))
            {
                return cachedCallSite;
            }

            var callSite = CreateCallSite(serviceType, callSiteChain);
            _callSiteCache[serviceType] = callSite;
            return callSite;
        }

        private IServiceCallSite CreateCallSite(Type serviceType, ISet<Type> callSiteChain)
        {
            try
            {
                // ISet.Add returns false if serviceType already present in call Site Chain
                if (!callSiteChain.Add(serviceType))
                {
                    throw new InvalidOperationException(Resources.FormatCircularDependencyException(serviceType));
                }

                for (int i = _descriptors.Count - 1; i >= 0; i--)
                {
                    var descriptor = _descriptors[i];

                    if (serviceType == descriptor.ServiceType)
                    {
                        return CreateCallSite(descriptor, callSiteChain);
                    }
                }

                for (int i = _descriptors.Count - 1; i >= 0; i--)
                {
                    var descriptor = _descriptors[i];

                    if (TryCreateOpenGeneric(descriptor, serviceType, callSiteChain, out var callSite))
                    {
                        return callSite;
                    }
                }

                if (serviceType.IsConstructedGenericType &&
                    serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    var itemType = serviceType.GenericTypeArguments.Single();
                    List<IServiceCallSite> callSites = new List<IServiceCallSite>();
                    foreach (var descriptor in _descriptors)
                    {
                        var callSite = TryCreateCallSite(descriptor, itemType, callSiteChain);
                        if (callSite != null)
                        {
                            callSites.Add(callSite);
                        }
                    }

                    return new ClosedIEnumerableCallSite(itemType, callSites.ToArray());
                }

                return null;
            }
            finally
            {
                callSiteChain.Remove(serviceType);
            }
        }

        private IServiceCallSite TryCreateCallSite(ServiceDescriptor descriptor, Type type, ISet<Type> callSiteChain)
        {
            IServiceCallSite serviceCallSite = null;
            if (type == descriptor.ServiceType)
            {
                serviceCallSite = CreateCallSite(descriptor, callSiteChain);
            }

            if (serviceCallSite == null)
            {
                TryCreateOpenGeneric(descriptor, type, callSiteChain, out serviceCallSite);
            }

            return serviceCallSite;
        }

        private bool TryCreateOpenGeneric(ServiceDescriptor descriptor, Type type, ISet<Type> callSiteChain,
            out IServiceCallSite serviceCallSite)
        {
            serviceCallSite = null;
            if (type.IsConstructedGenericType &&
                type.GetGenericTypeDefinition() == descriptor.ServiceType)
            {
                serviceCallSite = ApplyScope(CreateConstructorCallSite(
                    type,
                    descriptor.ImplementationType.MakeGenericType(type.GenericTypeArguments), callSiteChain), descriptor.Lifetime);
                return true;
            }
            return false;
        }

        private IServiceCallSite CreateCallSite(ServiceDescriptor descriptor, ISet<Type> callSiteChain)
        {
            IServiceCallSite callSite;
            if (descriptor.ImplementationInstance != null)
            {
                callSite = new ConstantCallSite(descriptor.ServiceType, descriptor.ImplementationInstance);
            }
            else if (descriptor.ImplementationFactory != null)
            {
                callSite = new FactoryService(descriptor.ServiceType, descriptor.ImplementationFactory);
            }
            else if (descriptor.ImplementationType != null)
            {
                callSite = CreateConstructorCallSite(descriptor.ServiceType, descriptor.ImplementationType, callSiteChain);
            }
            else
            {
                throw new InvalidOperationException("Invalid service descriptor");
            }

            return ApplyScope(callSite, descriptor.Lifetime);
        }

        private IServiceCallSite ApplyScope(IServiceCallSite serviceCallSite, ServiceLifetime descriptorLifetime)
        {
            if (serviceCallSite is ConstantCallSite)
            {
                return serviceCallSite;
            }
            if (descriptorLifetime == ServiceLifetime.Transient)
            {
                return new TransientCallSite(serviceCallSite);
            }
            else if (descriptorLifetime == ServiceLifetime.Scoped)
            {
                return new ScopedCallSite(serviceCallSite);
            }
            else
            {
                return new SingletonCallSite(serviceCallSite);
            }
        }

        private IServiceCallSite CreateConstructorCallSite(Type serviceType, Type implementationType, ISet<Type> callSiteChain)
        {
            var constructors = implementationType.GetTypeInfo()
                .DeclaredConstructors
                .Where(constructor => constructor.IsPublic)
                .ToArray();

            IServiceCallSite[] parameterCallSites = null;

            if (constructors.Length == 0)
            {
                throw new InvalidOperationException(Resources.FormatNoConstructorMatch(implementationType));
            }
            else if (constructors.Length == 1)
            {
                var constructor = constructors[0];
                var parameters = constructor.GetParameters();
                if (parameters.Length == 0)
                {
                    return new CreateInstanceCallSite(serviceType, implementationType);
                }

                parameterCallSites = CreateArgumentCallSites(
                    serviceType,
                    implementationType,
                    callSiteChain,
                    parameters,
                    throwIfCallSiteNotFound: true);

                return new ConstructorCallSite(serviceType, constructor, parameterCallSites);
            }

            Array.Sort(constructors,
                (a, b) => b.GetParameters().Length.CompareTo(a.GetParameters().Length));

            ConstructorInfo bestConstructor = null;
            HashSet<Type> bestConstructorParameterTypes = null;
            for (var i = 0; i < constructors.Length; i++)
            {
                var parameters = constructors[i].GetParameters();

                var currentParameterCallSites = CreateArgumentCallSites(
                    serviceType,
                    implementationType,
                    callSiteChain,
                    parameters,
                    throwIfCallSiteNotFound: false);

                if (currentParameterCallSites != null)
                {
                    if (bestConstructor == null)
                    {
                        bestConstructor = constructors[i];
                        parameterCallSites = currentParameterCallSites;
                    }
                    else
                    {
                        // Since we're visiting constructors in decreasing order of number of parameters,
                        // we'll only see ambiguities or supersets once we've seen a 'bestConstructor'.

                        if (bestConstructorParameterTypes == null)
                        {
                            bestConstructorParameterTypes = new HashSet<Type>(
                                bestConstructor.GetParameters().Select(p => p.ParameterType));
                        }

                        if (!bestConstructorParameterTypes.IsSupersetOf(parameters.Select(p => p.ParameterType)))
                        {
                            // Ambigious match exception
                            var message = string.Join(
                                Environment.NewLine,
                                Resources.FormatAmbigiousConstructorException(implementationType),
                                bestConstructor,
                                constructors[i]);
                            throw new InvalidOperationException(message);
                        }
                    }
                }
            }

            if (bestConstructor == null)
            {
                throw new InvalidOperationException(
                    Resources.FormatUnableToActivateTypeException(implementationType));
            }
            else
            {
                Debug.Assert(parameterCallSites != null);
                return parameterCallSites.Length == 0 ?
                    (IServiceCallSite)new CreateInstanceCallSite(serviceType, implementationType) :
                    new ConstructorCallSite(serviceType, bestConstructor, parameterCallSites);
            }
        }

        private IServiceCallSite[] CreateArgumentCallSites(
            Type serviceType,
            Type implementationType,
            ISet<Type> callSiteChain,
            ParameterInfo[] parameters,
            bool throwIfCallSiteNotFound)
        {
            var parameterCallSites = new IServiceCallSite[parameters.Length];
            for (var index = 0; index < parameters.Length; index++)
            {
                var callSite = GetCallSite(parameters[index].ParameterType, callSiteChain);

                if (callSite == null && parameters[index].HasDefaultValue)
                {
                    callSite = new ConstantCallSite(serviceType, parameters[index].DefaultValue);
                }

                if (callSite == null)
                {
                    if (throwIfCallSiteNotFound)
                    {
                        throw new InvalidOperationException(Resources.FormatCannotResolveService(
                            parameters[index].ParameterType,
                            implementationType));
                    }

                    return null;
                }

                parameterCallSites[index] = callSite;
            }

            return parameterCallSites;
        }

        public void Add(Type type, IServiceCallSite serviceCallSite)
        {
            _callSiteCache[type] = serviceCallSite;
        }
    }
}
