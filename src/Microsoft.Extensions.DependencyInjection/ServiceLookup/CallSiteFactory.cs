// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal class CallSiteFactory
    {
        private readonly List<ServiceDescriptor> _descriptors;
        private readonly Dictionary<Type, IServiceCallSite> _callSiteCache = new Dictionary<Type, IServiceCallSite>();

        public CallSiteFactory(IEnumerable<ServiceDescriptor> descriptors)
        {
            _descriptors = descriptors.ToList();
            Validate(descriptors);
        }

        private static void Validate(IEnumerable<ServiceDescriptor> descriptors)
        {
            foreach (var descriptor in descriptors)
            {
                var serviceTypeInfo = descriptor.ServiceType.GetTypeInfo();
                if (serviceTypeInfo.IsGenericTypeDefinition)
                {
                    var implementationTypeInfo = descriptor.ImplementationType?.GetTypeInfo();

                    if (implementationTypeInfo == null || !implementationTypeInfo.IsGenericTypeDefinition)
                    {
                        throw new ArgumentException(
                            Resources.FormatOpenGenericServiceRequiresOpenGenericImplementation(descriptor.ServiceType),
                            nameof(descriptors));
                    }

                    if (implementationTypeInfo.IsAbstract || implementationTypeInfo.IsInterface)
                    {
                        throw new ArgumentException(
                            Resources.FormatTypeCannotBeActivated(descriptor.ImplementationType, descriptor.ServiceType));
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
                            Resources.FormatTypeCannotBeActivated(descriptor.ImplementationType, descriptor.ServiceType));
                    }
                }
            }
        }

        internal IServiceCallSite CreateCallSite(Type serviceType, ISet<Type> callSiteChain)
        {
            lock (_callSiteCache)
            {
                if (_callSiteCache.TryGetValue(serviceType, out var cachedCallSite))
                {
                    return cachedCallSite;
                }

                IServiceCallSite callSite;
                try
                {
                    // ISet.Add returns false if serviceType already present in call Site Chain
                    if (!callSiteChain.Add(serviceType))
                    {
                        throw new InvalidOperationException(Resources.FormatCircularDependencyException(serviceType));
                    }

                    callSite = TryCreateExact(serviceType, callSiteChain) ??
                               TryCreateOpenGeneric(serviceType, callSiteChain) ??
                               TryCreateEnumerable(serviceType, callSiteChain);
                }
                finally
                {
                    callSiteChain.Remove(serviceType);
                }

                _callSiteCache[serviceType] = callSite;

                return callSite;
            }
        }

        private IServiceCallSite TryCreateExact(Type serviceType, ISet<Type> callSiteChain)
        {
            for (var i = _descriptors.Count - 1; i >= 0; i--)
            {
                var descriptor = _descriptors[i];
                var callSite = TryCreateExact(descriptor, serviceType, callSiteChain);
                if (callSite != null)
                {
                    return callSite;
                }
            }

            return null;
        }

        private IServiceCallSite TryCreateOpenGeneric(Type serviceType, ISet<Type> callSiteChain)
        {
            for (var i = _descriptors.Count - 1; i >= 0; i--)
            {
                var descriptor = _descriptors[i];
                var callSite = TryCreateOpenGeneric(descriptor, serviceType, callSiteChain);
                if (callSite != null)
                {
                    return callSite;
                }
            }

            return null;
        }

        private IServiceCallSite TryCreateEnumerable(Type serviceType, ISet<Type> callSiteChain)
        {
            if (serviceType.IsConstructedGenericType &&
                serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var itemType = serviceType.GenericTypeArguments.Single();
                var callSites = new List<IServiceCallSite>();
                foreach (var descriptor in _descriptors)
                {
                    var callSite = TryCreateExact(descriptor, itemType, callSiteChain) ??
                                   TryCreateOpenGeneric(descriptor, itemType, callSiteChain);

                    if (callSite != null)
                    {
                        callSites.Add(callSite);
                    }
                }

                return new ClosedIEnumerableCallSite(itemType, callSites.ToArray());
            }

            return null;
        }

        private IServiceCallSite TryCreateExact(ServiceDescriptor descriptor, Type serviceType, ISet<Type> callSiteChain)
        {
            if (serviceType == descriptor.ServiceType)
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

                return ApplyLifetime(callSite, descriptor.Lifetime);
            }

            return null;
        }

        private IServiceCallSite TryCreateOpenGeneric(ServiceDescriptor descriptor, Type type, ISet<Type> callSiteChain)
        {
            if (type.IsConstructedGenericType &&
                type.GetGenericTypeDefinition() == descriptor.ServiceType)
            {
                Debug.Assert(descriptor.ImplementationType != null, "descriptor.ImplementationType != null");

                var closedType = descriptor.ImplementationType.MakeGenericType(type.GenericTypeArguments);
                var constructorCallSite = CreateConstructorCallSite(type, closedType, callSiteChain);

                return ApplyLifetime(constructorCallSite, descriptor.Lifetime);
            }

            return null;
        }

        private IServiceCallSite ApplyLifetime(IServiceCallSite serviceCallSite, ServiceLifetime descriptorLifetime)
        {
            if (serviceCallSite is ConstantCallSite)
            {
                return serviceCallSite;
            }

            switch (descriptorLifetime)
            {
                case ServiceLifetime.Transient:
                    return new TransientCallSite(serviceCallSite);
                case ServiceLifetime.Scoped:
                    return new ScopedCallSite(serviceCallSite);
                case ServiceLifetime.Singleton:
                    return new SingletonCallSite(serviceCallSite);
                default:
                    throw new ArgumentOutOfRangeException(nameof(descriptorLifetime));
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
                var callSite = CreateCallSite(parameters[index].ParameterType, callSiteChain);

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
