using System;
using System.Collections.Generic;
using StructureMap;

namespace Microsoft.Framework.DependencyInjection.Structuremap
{
    public class StructureMapServiceProvider : IServiceProvider
    {
        private readonly IContainer _container;

        public StructureMapServiceProvider(IContainer container)
        {
            _container = container;
        }

        public object GetService(Type serviceType)
        {
            var instance = _container.TryGetInstance(serviceType);
            if (instance != null)
            {
                return instance;
            }
            if (serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof (IEnumerable<>))
            {
                // running TryGetInstance here causes an error in MVC
                return _container.GetInstance(serviceType);
            }

            return null;
        }
    }
}