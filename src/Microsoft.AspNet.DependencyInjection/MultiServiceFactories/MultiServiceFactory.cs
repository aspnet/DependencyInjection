using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.AspNet.DependencyInjection.MultiServiceFactories
{
    internal class MultiServiceFactory : IMultiServiceFactory
    {
        private readonly ServiceProvider _provider;
        private readonly IServiceDescriptor[] _descriptors;
        private readonly SingletonAccessor[] _singletons;
        private readonly ServiceFactory[] _factories;

        public MultiServiceFactory(ServiceProvider provider, IServiceDescriptor[] descriptors)
            : this(provider, descriptors, singletons: null)
        {
        }

        private MultiServiceFactory(
                ServiceProvider provider,
                IServiceDescriptor[] descriptors,
                SingletonAccessor[] singletons)
        {
            _provider = provider;
            _descriptors = descriptors;

            if (singletons != null)
            {
                _singletons = singletons;
            }
            else
            {
                _singletons = new SingletonAccessor[descriptors.Length];

                for (int i = 0; i < _singletons.Length; i++)
                {
                    _singletons[i] = new SingletonAccessor();
                }
            }

            _factories = new ServiceFactory[descriptors.Length];

            for (int i = 0; i < _factories.Length; i++)
            {
                _factories[i] = new ServiceFactory(
                    _provider,
                    _descriptors[i],
                    _singletons[i]);
            }
        }

        public IMultiServiceFactory Scope(ServiceProvider scopedProvider)
        {
            return new MultiServiceFactory(scopedProvider, _descriptors, _singletons);
        }

        public object GetSingleService()
        {
            return _factories[0].Resolve();
        }

        public object GetMultiService()
        {
            Type serviceType = _descriptors[0].ServiceType;
            Type listType = typeof(List<>).MakeGenericType(serviceType);
            var services = (IList)Activator.CreateInstance(listType);

            foreach (var factory in _factories)
            {
                services.Add(factory.Resolve());
            }

            return services;
        }
    }
}
