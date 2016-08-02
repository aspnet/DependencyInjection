// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection.Ordered
{
    internal class Ordered<TService> : IOrdered<TService>, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly OrderedEnumerableServiceDescriptorContainer<TService> _descriptorContainer;
        private readonly object _valuesLock = new object();
        private List<TService> _values;
        private List<IDisposable> _dispose;
        private bool _disposed;

        public Ordered(IServiceProvider serviceProvider, OrderedEnumerableServiceDescriptorContainer<TService> descriptorContainer)
        {
            _serviceProvider = serviceProvider;
            _descriptorContainer = descriptorContainer;
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

                _values = new List<TService>();
                _dispose = new List<IDisposable>();
                foreach (var descriptor in _descriptorContainer.ServiceDescriptor.Descriptors)
                {
                    TService value;
                    IDisposable disposable = null;

                    if (descriptor is FactoryServiceDescriptor)
                    {
                        var factoryServiceDescriptor = (FactoryServiceDescriptor) descriptor;
                        value = (TService)factoryServiceDescriptor.ImplementationFactory(_serviceProvider);
                        disposable = value as IDisposable;
                    }
                    else if (descriptor is TypeServiceDescriptor)
                    {
                        var typeServiceDescriptor = (TypeServiceDescriptor) descriptor;
                        value = (TService)ActivatorUtilities.CreateInstance(_serviceProvider, typeServiceDescriptor.ImplementationType);
                        disposable = value as IDisposable;
                    }
                    else if (descriptor is InstanceServiceDescriptor)
                    {
                        var instanceServiceDescriptor = (InstanceServiceDescriptor) descriptor;
                        value = (TService)instanceServiceDescriptor.ImplementationInstance;
                    }
                    else
                    {
                        throw new NotSupportedException(
                            Resources.FormatUnsupportedServiceDescriptorType(descriptor.GetType()));
                    }

                    _values.Add(value);
                    if (disposable != null)
                    {
                        _dispose.Add(disposable);
                    }
                }
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

                if (_dispose != null)
                {
                    foreach (var value in _dispose)
                    {
                        value?.Dispose();
                    }
                }
                _disposed = true;
            }
        }
    }
}