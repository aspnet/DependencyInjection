using System;
using System.Collections.Generic;

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
                        value = (TService) ActivatorUtilities.CreateInstance(_serviceProvider, typeServiceDescriptor.ImplementationType);
                        disposable = value as IDisposable;
                    }
                    else if (descriptor is InstanceServiceDescriptor)
                    {
                        var instanceServiceDescriptor = (InstanceServiceDescriptor) descriptor;
                        value = (TService) instanceServiceDescriptor.ImplementationInstance;
                    }
                    else
                    {
                        throw new NotSupportedException(Resources.FormatUnsupportedServiceDescriptorType(descriptor.GetType()));
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
}