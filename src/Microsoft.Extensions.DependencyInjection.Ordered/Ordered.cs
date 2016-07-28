// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection.Ordered
{
    internal class Ordered<T>: IOrdered<T>, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly OrderedServiceDescriptorHolder<T> _descriptorHolder;
        private List<T> _values;
        private List<IDisposable> _dispose;

        public Ordered(IServiceProvider serviceProvider, OrderedServiceDescriptorHolder<T> descriptorHolder)
        {
            _serviceProvider = serviceProvider;
            _descriptorHolder = descriptorHolder;
        }

        private void EnsureValues()
        {
            lock (_descriptorHolder)
            {
                if (_values != null)
                {
                    return;
                }

                _values = new List<T>();
                _dispose = new List<IDisposable>();
                foreach (var descriptor in _descriptorHolder.ServiceDescriptor.Descriptors)
                {
                    var factoryServiceDescriptor = descriptor as FactoryServiceDescriptor;
                    T value;
                    IDisposable disposable = null;

                    if (factoryServiceDescriptor != null)
                    {
                        value = (T)factoryServiceDescriptor.ImplementationFactory(_serviceProvider);
                        disposable = value as IDisposable;
                    }
                    else
                    {
                        var typeServiceDescriptor = descriptor as TypeServiceDescriptor;
                        if (typeServiceDescriptor != null)
                        {
                            value = (T) ActivatorUtilities.CreateInstance(_serviceProvider, typeServiceDescriptor.ImplementationType);
                            disposable = value as IDisposable;
                        }
                        else
                        {
                            var instanceServiceDescriptor = descriptor as InstanceServiceDescriptor;
                            if (instanceServiceDescriptor != null)
                            {
                                value = (T) instanceServiceDescriptor.ImplementationInstance;
                            }
                            else
                            {
                                throw new NotSupportedException(Resources.FormatUnsupportedServiceDescriptorType(descriptor.GetType()));
                            }
                        }
                    }
                    _values.Add(value);
                    if (disposable != null)
                    {
                        _dispose.Add(disposable);
                    }
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
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
            lock (_descriptorHolder)
            {
                if (_dispose != null)
                {
                    foreach (var value in _dispose)
                    {
                        value?.Dispose();
                    }
                }
            }
        }
    }
}