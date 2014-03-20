using System;

namespace Microsoft.AspNet.DependencyInjection.MultiServiceFactories
{
    internal class ServiceFactory
    {
        private readonly ServiceProvider _provider;
        private readonly IServiceDescriptor _descriptor;
        private readonly SingletonAccessor _singletonAccesor;
        private object _scopedService;

        public ServiceFactory(
                ServiceProvider provider,    
                IServiceDescriptor descriptor,
                SingletonAccessor singletonAccessor)
        {
            _provider = provider;
            _descriptor = descriptor;
            _singletonAccesor = singletonAccessor;
        }

        public object Resolve()
        {
            lock (_singletonAccesor)
            {
                if (_singletonAccesor.Singleton != null)
                {
                    return _singletonAccesor.Singleton;
                }
                else if (_scopedService != null)
                {
                    return _scopedService;
                } 
                else if (_descriptor.ImplementationInstance != null)
                {
                    _singletonAccesor.Singleton = _descriptor.ImplementationInstance;
                    return _descriptor.ImplementationInstance;
                }
                else
                {
                    var serviceFactory =
                        ActivatorUtilities.CreateFactory(_descriptor.ImplementationType);

                    object resolvedService = serviceFactory(_provider);

                    var disposable = resolvedService as IDisposable;
                    if (disposable != null)
                    {
                        _provider.RegisterDisposable(disposable);
                    }

                    if (_descriptor.Lifecycle == LifecycleKind.Singleton)
                    {
                        _singletonAccesor.Singleton = resolvedService;
                    }
                    else if (_descriptor.Lifecycle == LifecycleKind.Scoped)
                    {
                        _scopedService = resolvedService;
                    }

                    return resolvedService;
                }
            }
        }
    }
}
