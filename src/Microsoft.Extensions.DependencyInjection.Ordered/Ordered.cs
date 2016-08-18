// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection.Ordered
{
    internal class OrderedScopeProvider<TService> : IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly OrderedEnumerableServiceDescriptorContainer<TService> _descriptorContainer;
        private readonly ServiceLifetime _lifetime;
        private List<KeyValuePair<int, TService>> _values;
        private List<IDisposable> _dispose;

        public OrderedScopeProvider(IServiceProvider serviceProvider,
            OrderedEnumerableServiceDescriptorContainer<TService> descriptorContainer,
            ServiceLifetime lifetime)
        {
            _serviceProvider = serviceProvider;
            _descriptorContainer = descriptorContainer;
            _lifetime = lifetime;
        }

        public IEnumerable<KeyValuePair<int, TService>> GetValues()
        {
            if (_values != null)
            {
                return _values;
            }

            _values = new List<KeyValuePair<int, TService>>();
            _dispose = new List<IDisposable>();
            var descriptors = _descriptorContainer.ServiceDescriptor.Descriptors;
            for (int i = 0; i < descriptors.Count; i++)
            {
                var descriptor = descriptors[i];
                if (descriptor.Lifetime == _lifetime)
                {
                    TService value;
                    IDisposable disposable = null;

                    if (descriptor is FactoryServiceDescriptor)
                    {
                        var factoryServiceDescriptor = (FactoryServiceDescriptor) descriptor;
                        value = (TService) factoryServiceDescriptor.ImplementationFactory(_serviceProvider);
                        disposable = value as IDisposable;
                    }
                    else if (descriptor is TypeServiceDescriptor)
                    {
                        var typeServiceDescriptor = (TypeServiceDescriptor) descriptor;
                        value =
                            (TService)
                            ActivatorUtilities.CreateInstance(_serviceProvider,
                                typeServiceDescriptor.ImplementationType);
                        disposable = value as IDisposable;
                    }
                    else if (descriptor is InstanceServiceDescriptor)
                    {
                        var instanceServiceDescriptor = (InstanceServiceDescriptor) descriptor;
                        value = (TService) instanceServiceDescriptor.ImplementationInstance;
                    }
                    else
                    {
                        throw new NotSupportedException(
                            Resources.FormatUnsupportedServiceDescriptorType(descriptor.GetType()));
                    }

                    _values.Add(new KeyValuePair<int, TService>(i, value));
                    if (disposable != null)
                    {
                        _dispose.Add(disposable);
                    }
                }
            }
            return _values;
        }

        public void Dispose()
        {
            if (_dispose != null)
            {
                foreach (var value in _dispose)
                {
                    value?.Dispose();
                }
            }
            _dispose = null;
        }

        internal class TransientOrderedScopeProvider : OrderedScopeProvider<TService>
        {
            public TransientOrderedScopeProvider(IServiceProvider serviceProvider,
                OrderedEnumerableServiceDescriptorContainer<TService> descriptorContainer)
                : base(serviceProvider, descriptorContainer, ServiceLifetime.Transient)
            {
            }
        }
        internal class ScopedOrderedScopeProvider : OrderedScopeProvider<TService>
        {
            public ScopedOrderedScopeProvider(IServiceProvider serviceProvider,
                OrderedEnumerableServiceDescriptorContainer<TService> descriptorContainer)
                : base(serviceProvider, descriptorContainer, ServiceLifetime.Scoped)
            {
            }
        }
        internal class SingletonOrderedScopeProvider : OrderedScopeProvider<TService>
        {
            public SingletonOrderedScopeProvider(IServiceProvider serviceProvider,
                OrderedEnumerableServiceDescriptorContainer<TService> descriptorContainer)
                : base(serviceProvider, descriptorContainer, ServiceLifetime.Singleton)
            {
            }
        }
    }

    internal class Ordered<TService> : IOrdered<TService>, IDisposable
    {
        private readonly object _valuesLock = new object();
        private bool _disposed;
        private List<TService> _values;

        private readonly OrderedScopeProvider<TService>.TransientOrderedScopeProvider _transientOrderedScopeProvider;
        private readonly OrderedScopeProvider<TService>.ScopedOrderedScopeProvider _scopedOrderedScopeProvider;
        private readonly OrderedScopeProvider<TService>.SingletonOrderedScopeProvider _singletonOrderedScopeProvider;

        public Ordered(
            OrderedScopeProvider<TService>.TransientOrderedScopeProvider transientOrderedScopeProvider,
            OrderedScopeProvider<TService>.ScopedOrderedScopeProvider scopedOrderedScopeProvider,
            OrderedScopeProvider<TService>.SingletonOrderedScopeProvider singletonOrderedScopeProvider)
        {
            _transientOrderedScopeProvider = transientOrderedScopeProvider;
            _scopedOrderedScopeProvider = scopedOrderedScopeProvider;
            _singletonOrderedScopeProvider = singletonOrderedScopeProvider;
        }

        private void EnsureValues()
        {
            lock (_valuesLock)
            {
                if (_disposed)
                {
                    throw new ObjectDisposedException(Resources.FormatObjectDisposedException(this.GetType()));
                }
                if (_values != null)
                {
                    return;
                }

                var values = new SortedDictionary<int, TService>();
                foreach (var keyValuePair in _transientOrderedScopeProvider.GetValues())
                {
                    values.Add(keyValuePair.Key, keyValuePair.Value);
                }
                foreach (var keyValuePair in _scopedOrderedScopeProvider.GetValues())
                {
                    values.Add(keyValuePair.Key, keyValuePair.Value);
                }
                foreach (var keyValuePair in _singletonOrderedScopeProvider.GetValues())
                {
                    values.Add(keyValuePair.Key, keyValuePair.Value);
                }

                _values = values.Values.ToList();
            }
        }

        public IEnumerator<TService> GetEnumerator()
        {
            EnsureValues();
            return _values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            lock (_valuesLock)
            {
                if (_disposed)
                {
                    return;
                }

                _transientOrderedScopeProvider.Dispose();
                _scopedOrderedScopeProvider.Dispose();
                _singletonOrderedScopeProvider.Dispose();

                _disposed = true;
            }
        }
    }
}