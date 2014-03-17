using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace Microsoft.AspNet.DependencyInjection
{
    /// <summary>
    /// The default IServiceProvider.
    /// </summary>
    internal class ServiceProvider : IServiceProvider, IDisposable
    {
        private readonly IServiceProvider _defaultServiceProvider;
        private readonly IDictionary<Type, IMultiServiceFactory> _factories;
        private readonly ConcurrentBag<IDisposable> _disposables = new ConcurrentBag<IDisposable>();

        public ServiceProvider(IEnumerable<IServiceDescriptor> serviceDescriptors)
            : this(serviceDescriptors, defaultServiceProvider: null)
        {
        }

        public ServiceProvider(
                IEnumerable<IServiceDescriptor> serviceDescriptors,
                IServiceProvider defaultServiceProvider)
        {
            _defaultServiceProvider = defaultServiceProvider;

            var groupedDescriptors = serviceDescriptors.GroupBy(descriptor => descriptor.ServiceType);
            _factories = groupedDescriptors.ToDictionary(
                grouping => grouping.Key,
                grouping => (IMultiServiceFactory)new MultiServiceFactory(this, grouping.ToArray()));

            _factories[typeof(IServiceProvider)] = new ServiceProviderFactory(this);
            _factories[typeof(IServiceScopeFactory)] = new ServiceScopeFactoryFactory(this);
        }

        // This constructor is called exclusively to create a chile scope from the parent
        private ServiceProvider(ServiceProvider parent)
        {
            _defaultServiceProvider = parent._defaultServiceProvider;

            // Rescope all the factories
            _factories = parent._factories.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Scope(this));

            // Dispose this along with the parent scope
            parent._disposables.Add(this);
        }

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public virtual object GetService(Type serviceType)
        {
            return GetSingleService(serviceType) ??
                GetMultiService(serviceType) ??
                GetDefaultService(serviceType);
        }

        private object GetSingleService(Type serviceType)
        {
            IMultiServiceFactory serviceFactory;
            return _factories.TryGetValue(serviceType, out serviceFactory)
                ? serviceFactory.GetSingleService()
                : null;
        }

        private IList GetMultiService(Type collectionType)
        {
            if (collectionType.GetTypeInfo().IsGenericType &&
                collectionType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                Type serviceType = collectionType.GetTypeInfo().GenericTypeArguments.Single();

                IMultiServiceFactory serviceFactory;

                if (_factories.TryGetValue(serviceType, out serviceFactory))
                {
                    IList services;
                    services = serviceFactory.GetMultiService();

                    // Try to get more services from _defaultServiceProvider
                    IEnumerable extraServices = GetDefaultService(collectionType) as IEnumerable;
                    if (extraServices != null)
                    {
                        foreach (var extraService in extraServices)
                        {
                            services.Add(extraService);
                        }
                    }
                    else
                    {
                        var extraService = GetDefaultService(serviceType);

                        if (extraService != null)
                        {
                            services.Add(extraService);
                        }
                    }

                    return services;
                }
            }

            return null;
        }

        private object GetDefaultService(Type serviceType)
        {
            return _defaultServiceProvider != null ? _defaultServiceProvider.GetService(serviceType) : null;
        }

        public void Dispose()
        {
            foreach (var disposable in _disposables)
            {
                disposable.Dispose();
            }
        }

        private Func<object> CreateSingletonServiceFactory(IServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationType != null)
            {
                Func<IServiceProvider, object> serviceFactory =
                    ActivatorUtilities.CreateFactory(descriptor.ImplementationType);

                var singletonServiceFactory = new Lazy<object>(() => CaptureDisposableService(serviceFactory(this)));
                return () => singletonServiceFactory.Value;
            }
            else
            {
                Debug.Assert(descriptor.ImplementationInstance != null);

                CaptureDisposableService(descriptor.ImplementationInstance);
                return () => descriptor.ImplementationInstance;
            }
        }

        private Func<object> CreateTransientServiceFactory(IServiceDescriptor descriptor)
        {
            // A service with an ImplementationInsance cannot be transient
            Debug.Assert(descriptor.ImplementationType != null);

            Func<IServiceProvider, object> serviceFactory =
                ActivatorUtilities.CreateFactory(descriptor.ImplementationType);

            return () => CaptureDisposableService(serviceFactory(this));
        }

        private object CaptureDisposableService(object service)
        {
            IDisposable disposable = service as IDisposable;
            if (disposable != null)
            {
                _disposables.Add(disposable);
            }

            return service;
        }

        private interface IMultiServiceFactory
        {
            IMultiServiceFactory Scope(ServiceProvider scopedProvider);
            object GetSingleService();
            IList GetMultiService();
        }

        private class MultiServiceFactory : IMultiServiceFactory
        {
            private readonly ServiceProvider _provider;
            private readonly IServiceDescriptor[] _descriptors;
            private readonly Func<object>[] _factories;

            public MultiServiceFactory(ServiceProvider provider, IServiceDescriptor[] descriptors)
            {
                Debug.Assert(provider != null);
                Debug.Assert(descriptors.Length > 0);
                Debug.Assert(descriptors.All(d => d.ServiceType == descriptors[0].ServiceType));

                _provider = provider;
                _descriptors = descriptors;
                _factories = new Func<object>[descriptors.Length];
                CreateSingletonFactories();
                CreateNonSingletonFactories();
            }

            // Copy constructor
            private MultiServiceFactory(
                    ServiceProvider provider,
                    IServiceDescriptor[] descriptors,
                    Func<object>[] factories)
            {
                _provider = provider;
                _descriptors = descriptors;
                _factories = factories;
            }

            public IMultiServiceFactory Scope(ServiceProvider scopedProvider)
            {
                // Shallow-copy the _factories array since we replace the non-singletons
                var factories = _factories.ToArray();
                var scope = new MultiServiceFactory(scopedProvider, _descriptors, factories);

                // Regenerate all factories that are not singletons
                scope.CreateNonSingletonFactories();

                return scope;
            }

            public object GetSingleService()
            {
                return _factories[0]();
            }

            public IList GetMultiService()
            {
                Type serviceType = _descriptors[0].ServiceType;
                Type listType = typeof(List<>).MakeGenericType(serviceType);
                var services = (IList)Activator.CreateInstance(listType);

                foreach (var factory in _factories)
                {
                    services.Add(factory());
                }

                return services;
            }

            // CreateSingletonFactories is only called for the top-level container
            // These top-level factories get copied by MultiServiceFactory.Scope 
            private void CreateSingletonFactories()
            {
                for (int i = 0; i < _descriptors.Length; i++)
                {
                    if (_descriptors[i].Lifecycle == LifecycleKind.Singleton)
                    {
                        _factories[i] = _provider.CreateSingletonServiceFactory(_descriptors[i]);
                    }
                }
            }

            private void CreateNonSingletonFactories()
            {
                for (int i = 0; i < _descriptors.Length; i++)
                {
                    if (_descriptors[i].Lifecycle == LifecycleKind.Scoped)
                    {
                        // A scoped service is a singleton from the perspective of the scoped ServiceProvider
                        _factories[i] = _provider.CreateSingletonServiceFactory(_descriptors[i]);
                    }
                    else if (_descriptors[i].Lifecycle == LifecycleKind.Transient)
                    {
                        _factories[i] = _provider.CreateTransientServiceFactory(_descriptors[i]);
                    }
                }
            }
        }

        private class ServiceProviderFactory : IMultiServiceFactory
        {
            private readonly ServiceProvider _provider;

            public ServiceProviderFactory(ServiceProvider provider)
            {
                _provider = provider;
            }

            public IMultiServiceFactory Scope(ServiceProvider scopedProvider)
            {
                return new ServiceProviderFactory(scopedProvider);
            }

            public object GetSingleService()
            {
                return _provider;
            }

            public IList GetMultiService()
            {
                return new List<IServiceProvider> { _provider };
            }
        }

        private class ServiceScopeFactoryFactory : IMultiServiceFactory
        {
            private readonly ServiceProvider _provider;

            public ServiceScopeFactoryFactory(ServiceProvider provider)
            {
                _provider = provider;
            }

            public IMultiServiceFactory Scope(ServiceProvider scopedProvider)
            {
                return new ServiceScopeFactoryFactory(scopedProvider);
            }

            public object GetSingleService()
            {
                return new ServiceScopeFactory(_provider);
            }

            public IList GetMultiService()
            {
                return new List<IServiceScopeFactory> { new ServiceScopeFactory(_provider) };
            }

            private class ServiceScopeFactory : IServiceScopeFactory
            {
                private readonly ServiceProvider _provider;

                public ServiceScopeFactory(ServiceProvider provider)
                {
                    _provider = provider;
                }

                public IServiceScope CreateScope()
                {
                    return new ServiceScope(new ServiceProvider(_provider));
                }

                private class ServiceScope : IServiceScope
                {
                    private readonly ServiceProvider _scopedProvider;

                    public ServiceScope(ServiceProvider scopedProvider)
                    {
                        _scopedProvider = scopedProvider;
                    }

                    public IServiceProvider ServiceProvider
                    {
                        get { return _scopedProvider.GetService<IServiceProvider>(); }
                    }

                    public void Dispose()
                    {
                        _scopedProvider.Dispose();
                    }
                }
            }
        }
    }
}
