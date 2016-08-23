using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.Specification;
using Microsoft.Practices.Unity;
using Microsoft.Extensions.DependencyInjection.Adapters.Unity;
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Adapters.Tests
{
    public class ServiceProviderContainerTests : DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {
            var container = new UnityContainer();
            container.Populate(serviceCollection);
            return container.Resolve<IServiceProvider>();
        }
    }
}
