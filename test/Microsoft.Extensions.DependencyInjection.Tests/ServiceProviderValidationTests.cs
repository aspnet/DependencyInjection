using System;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class ServiceProviderValidationTests
    {
        [Fact]
        public void GetService_Throws_WhenScopedIsInjectedIntoSingleton()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IFoo, Foo>();
            serviceCollection.AddScoped<IBar, Bar>();
            var serviceProvider = serviceCollection.BuildServiceProvider(true);

            // Act + Assert
            var exception = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetService(typeof(IFoo)));
            Assert.Equal($"Cannot consume scoped service '{typeof(IBar)}' from singleton '{typeof(IFoo)}'.", exception.Message);
        }

        public void GetService_DoesNotThrow_WhenScopeFactoryIsInjectedIntoSingleton()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IBoo, Boo>();
            var serviceProvider = serviceCollection.BuildServiceProvider(true);

            // Act + Assert
            var result = serviceProvider.GetService(typeof(IBoo));
            Assert.NotNull(result);
        }

        private interface IFoo
        {
        }

        private class Foo : IFoo
        {
            public Foo(IBar bar)
            {
            }
        }

        private interface IBar
        {
        }

        private class Bar : IBar
        {
        }

        private interface IBoo
        {
        }

        private class Boo : IBoo
        {
            public Boo(IServiceScopeFactory scopeFactory)
            {
            }
        }

    }
}
