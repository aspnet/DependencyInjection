// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Extensions.DependencyInjection.Extensions
{
    internal class Ordered<T>: IOrdered<T>, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly OrderedServiceDescriptorHolder<T> _descriptorHolder;
        private readonly Lazy<List<T>> _values;

        public Ordered(IServiceProvider serviceProvider, OrderedServiceDescriptorHolder<T> descriptorHolder)
        {
            _serviceProvider = serviceProvider;
            _descriptorHolder = descriptorHolder;
            _values = new Lazy<List<T>>(() => GenerateValues().ToList());
        }

        private  IEnumerable<T> GenerateValues()
        {
            foreach (var descriptor in _descriptorHolder.ServiceDescriptor.Descriptors)
            {
                var factoryServiceDescriptor = descriptor as FactoryServiceDescriptor;
                if (factoryServiceDescriptor != null)
                {
                    yield return (T)factoryServiceDescriptor.ImplementationFactory(_serviceProvider);
                }
                else
                {
                    var typeServiceDescriptor = descriptor as TypeServiceDescriptor;
                    if (typeServiceDescriptor != null)
                    {
                        yield return (T) ActivatorUtilities.CreateInstance(_serviceProvider, typeServiceDescriptor.ImplementationType);
                    }
                    else
                    {
                        var instanceServiceDescriptor = descriptor as InstanceServiceDescriptor;
                        if (instanceServiceDescriptor != null)
                        {
                            yield return (T)instanceServiceDescriptor.ImplementationInstance;
                        }
                    }
                }
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _values.Value.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Dispose()
        {
            if (_values.IsValueCreated)
            {
                foreach (var value in _values.Value)
                {
                    (value as IDisposable)?.Dispose();
                }
            }
        }
    }
}