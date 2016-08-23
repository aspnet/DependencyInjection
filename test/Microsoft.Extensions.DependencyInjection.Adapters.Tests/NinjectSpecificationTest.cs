using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Specification;
using Ninject;
using Microsoft.Extensions.DependencyInjection.Adapters.Ninject;
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Adapters.Tests
{
    public class NinjectContainerTests : DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection)
        {
            var container = new StandardKernel();
                container.Populate(serviceCollection);
            return container.Get<IServiceProvider>();
        }

        [Fact]
        public void MultipleSe1rviceCanBeIEnumerableResolved()
        {
            // Arrange
            var collection = new ServiceCollection();
            collection.AddEnumerable(typeof(IFakeMultipleService)).AddTransient(typeof(FakeOneMultipleService));
            collection.AddEnumerable(typeof(IFakeMultipleService)).AddTransient(typeof(FakeTwoMultipleService));
            var provider = CreateServiceProvider(collection);

            // Act
            var services = provider.GetService<IEnumerable<IFakeMultipleService>>();

            // Assert
            Assert.Collection(services.OrderBy(s => s.GetType().FullName),
                service => Assert.IsType<FakeOneMultipleService>(service),
                service => Assert.IsType<FakeTwoMultipleService>(service));
        }
    }
}
