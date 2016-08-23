using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.ObjectBuilder;

namespace Microsoft.Extensions.DependencyInjection.Adapters.Unity
{
    public static class UnityServiceCollectionExtensions
    {
        public static void Populate(this IUnityContainer container,
            IServiceCollection descriptors)
        {
            container.RegisterType<IServiceProvider, UnityServiceProvider>();
            container.RegisterType<IServiceScopeFactory, UnityServiceScopeFactory>();

            foreach (var descriptor in descriptors)
            {
                Register(container, descriptor);
            }
        }

        private static void Register(IUnityContainer container, ServiceDescriptor descriptor, string name = null)
        {
            var typeServiceDescriptor = descriptor as TypeServiceDescriptor;
            if (typeServiceDescriptor != null)
            {
                container.RegisterType(descriptor.ServiceType,
                    typeServiceDescriptor.ImplementationType,
                    name,
                    GetLifetimeManager(descriptor.Lifetime));
                return;
            }

            var factoryServiceDescriptor = descriptor as FactoryServiceDescriptor;
            if (factoryServiceDescriptor != null)
            {
                container.RegisterType(descriptor.ServiceType,
                    name,
                    GetLifetimeManager(descriptor.Lifetime),
                    new InjectionFactory(unity =>
                    {
                        var provider = unity.Resolve<IServiceProvider>();
                        return factoryServiceDescriptor.ImplementationFactory(provider);
                    }));
                return;
            }

            var instanceServiceDescriptor = descriptor as InstanceServiceDescriptor;
            if (instanceServiceDescriptor != null)
            {
                container.RegisterInstance(instanceServiceDescriptor.ServiceType,
                    name,
                    instanceServiceDescriptor.ImplementationInstance,
                    GetLifetimeManager(instanceServiceDescriptor.Lifetime));
            }

            var enumerableServiceDescriptor = descriptor as EnumerableServiceDescriptor;
            if (enumerableServiceDescriptor != null)
            {
                var i = 0;
                foreach (var serviceDescriptor in enumerableServiceDescriptor.Descriptors)
                {
                    Register(container, serviceDescriptor, i.ToString());
                    i++;
                }
                container.RegisterType(
                    typeof(IEnumerable<>).MakeGenericType(descriptor.ServiceType),
                    descriptor.ServiceType.MakeArrayType()
                );
            }
        }

        private static LifetimeManager GetLifetimeManager(ServiceLifetime lifecycle)
        {
            switch (lifecycle)
            {
                case ServiceLifetime.Singleton:
                    return new ContainerControlledLifetimeManager();
                case ServiceLifetime.Scoped:
                    return new HierarchicalLifetimeManager();
                case ServiceLifetime.Transient:
                    return new TransientLifetimeManager();
                default:
                    throw new NotSupportedException(lifecycle.ToString());
            }
        }
    }
}
