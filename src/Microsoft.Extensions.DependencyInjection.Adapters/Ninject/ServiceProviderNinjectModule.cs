using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using Ninject.Modules;
using Ninject.Syntax;

namespace Microsoft.Extensions.DependencyInjection.Adapters.Ninject
{
    internal class ServiceProviderNinjectModule : NinjectModule
    {
        private readonly IEnumerable<ServiceDescriptor> _serviceDescriptors;

        public ServiceProviderNinjectModule(
            IEnumerable<ServiceDescriptor> serviceDescriptors)
        {
            _serviceDescriptors = serviceDescriptors;
        }

        public override void Load()
        {
            foreach (var descriptor in _serviceDescriptors)
            {
                var enumerableServiceDescriptor = descriptor as EnumerableServiceDescriptor;
                if (enumerableServiceDescriptor != null)
                {
                    foreach (var serviceDescriptor in enumerableServiceDescriptor.Descriptors)
                    {
                        Add(serviceDescriptor);
                    }
                }
                else
                {
                    Add(descriptor);
                }
            }

            Bind<IServiceProvider>().ToMethod(context =>
            {
                var resolver = context.Kernel.Get<IResolutionRoot>();
                var inheritedParams = context.Parameters.Where(p => p.ShouldInherit);

                var scopeParam = new ScopeParameter();
                inheritedParams = inheritedParams.AddOrReplaceScopeParameter(scopeParam);

                return new NinjectServiceProvider(resolver, inheritedParams.ToArray());
            }).InRequestScope();

            Bind<IServiceScopeFactory>().ToMethod(context =>
            {
                return new NinjectServiceScopeFactory(context);
            }).InRequestScope();
        }

        private void Add(ServiceDescriptor descriptor)
        {
            IBindingWhenInNamedWithOrOnSyntax<object> binding = null;

            var typeServiceDescriptor = descriptor as TypeServiceDescriptor;
            if (typeServiceDescriptor != null)
            {
                binding = Bind(descriptor.ServiceType).To(typeServiceDescriptor.ImplementationType);
            }
            var factoryServiceDescriptor = descriptor as FactoryServiceDescriptor;
            if (factoryServiceDescriptor != null)
            {
                binding = Bind(descriptor.ServiceType).ToMethod(context =>
                {
                    var serviceProvider = context.Kernel.Get<IServiceProvider>();
                    return factoryServiceDescriptor.ImplementationFactory(serviceProvider);
                });
            }

            var instanceServiceDescriptor = descriptor as InstanceServiceDescriptor;
            if (instanceServiceDescriptor != null)
            {
                binding = Bind(descriptor.ServiceType).ToConstant(instanceServiceDescriptor.ImplementationInstance);
            }

            switch (descriptor.Lifetime)
            {
                case ServiceLifetime.Singleton:
                    binding?.InSingletonScope();
                    break;
                case ServiceLifetime.Scoped:
                    binding?.InRequestScope();
                    break;
                case ServiceLifetime.Transient:
                    binding?.InTransientScope();
                    break;
            }
        }
    }
}