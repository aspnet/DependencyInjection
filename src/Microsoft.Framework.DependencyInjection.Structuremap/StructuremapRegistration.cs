using System;
using System.Collections.Generic;
using System.Reflection;
using StructureMap;
using StructureMap.Configuration.DSL;
using StructureMap.Pipeline;

namespace Microsoft.Framework.DependencyInjection.Structuremap
{
    public static class StructuremapRegistration
    {
        public static IContainer Populate(
            IEnumerable<ServiceDescriptor> descriptors)
        {
            var registry = new Registry();

            registry.For<IServiceScope>().Use<StructureMapServiceScope>();
            registry.For<IServiceProvider>().Use<StructureMapServiceProvider>();
            registry.For<IServiceScopeFactory>().Use<StructureMapServiceScopeFactory>();
            return BuildRegistry(registry, descriptors);
        }

        public static Container BuildRegistry(Registry registry, IEnumerable<ServiceDescriptor> descriptors)
        {
            foreach (var descriptor in descriptors)
            {
                if (descriptor.ImplementationType != null)
                {
                    // Test if the an open generic type is being registered
                    var serviceTypeInfo = descriptor.ServiceType.GetTypeInfo();
                    if (serviceTypeInfo.IsGenericTypeDefinition)
                    {
                        registry.For(descriptor.ServiceType).Use(descriptor.ImplementationType);
                    }
                    else
                    {
                        registry.For(descriptor.ServiceType)
                            .Use(descriptor.ImplementationType)
                            .SetLifecycleTo(LifetimeConvertor(descriptor.Lifetime));
                    }
                }
                else if (descriptor.ImplementationFactory != null)
                {
                    registry.For(descriptor.ServiceType)
                        .Use($"Implementation Factory for {descriptor.ServiceType.Name}", context =>
                            descriptor.ImplementationFactory(context.GetInstance<IServiceProvider>()));
                }
                else
                {
                    registry.For(descriptor.ServiceType)
                        .Use(descriptor.ImplementationInstance);
                }
            }
            return new Container(registry);
        }

        private static LifecycleBase LifetimeConvertor(ServiceLifetime lifetime)
        {
            switch (lifetime)
            {
                case ServiceLifetime.Singleton:
                    return new SingletonLifecycle();
                case ServiceLifetime.Transient:
                case ServiceLifetime.Scoped:
                    return new TransientLifecycle();
            }
            return new TransientLifecycle();
        }
    }
}