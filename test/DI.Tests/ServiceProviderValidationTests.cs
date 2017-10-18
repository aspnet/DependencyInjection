// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
            var serviceProvider = serviceCollection.BuildServiceProvider(validateScopes: true);

            // Act + Assert
            var exception = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetService(typeof(IFoo)));
            Assert.Equal($"Cannot consume scoped service '{typeof(IBar)}' from singleton '{typeof(IFoo)}'.", exception.Message);
        }

        [Fact]
        public void GetService_Throws_WhenScopedIsInjectedIntoSingletonThroughTransient()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IFoo, Foo>();
            serviceCollection.AddTransient<IBar, Bar2>();
            serviceCollection.AddScoped<IBaz, Baz>();
            var serviceProvider = serviceCollection.BuildServiceProvider(validateScopes: true);

            // Act + Assert
            var exception = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetService(typeof(IFoo)));
            Assert.Equal($"Cannot consume scoped service '{typeof(IBaz)}' from singleton '{typeof(IFoo)}'.", exception.Message);
        }

        [Fact]
        public void GetService_Throws_WhenScopedIsInjectedIntoSingletonThroughSingleton()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddSingleton<IFoo, Foo>();
            serviceCollection.AddSingleton<IBar, Bar2>();
            serviceCollection.AddScoped<IBaz, Baz>();
            var serviceProvider = serviceCollection.BuildServiceProvider(validateScopes: true);

            // Act + Assert
            var exception = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetService(typeof(IFoo)));
            Assert.Equal($"Cannot consume scoped service '{typeof(IBaz)}' from singleton '{typeof(IBar)}'.", exception.Message);
        }

        [Fact]
        public void GetService_Throws_WhenGetServiceForScopedServiceIsCalledOnRoot()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddScoped<IBar, Bar>();
            var serviceProvider = serviceCollection.BuildServiceProvider(validateScopes: true);

            // Act + Assert
            var exception = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetService(typeof(IBar)));
            Assert.Equal($"Cannot resolve scoped service '{typeof(IBar)}' from root provider.", exception.Message);
        }

        [Fact]
        public void GetService_Throws_WhenGetServiceForScopedServiceIsCalledOnRootViaTransient()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient<IFoo, Foo>();
            serviceCollection.AddScoped<IBar, Bar>();
            var serviceProvider = serviceCollection.BuildServiceProvider(validateScopes: true);

            // Act + Assert
            var exception = Assert.Throws<InvalidOperationException>(() => serviceProvider.GetService(typeof(IFoo)));
            Assert.Equal($"Cannot resolve '{typeof(IFoo)}' from root provider because it requires scoped service '{typeof(IBar)}'.", exception.Message);
        }

        [Fact]
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

        public static IEnumerable<object[]> InvalidImplementationTypes()
        {
            var addActions = new Action<IServiceCollection, Type, Type>[]
            {
                (collection, serviceType, implemetationType) => collection.AddSingleton(serviceType, implemetationType),
                (collection, serviceType, implemetationType) => collection.AddScoped(serviceType, implemetationType),
                (collection, serviceType, implemetationType) => collection.AddTransient(serviceType, implemetationType)
            };

            foreach (var action in addActions)
            {
                yield return new object[] { typeof(IFoo), typeof(Bar), action };
                yield return new object[] { typeof(IFoo), typeof(IBar), action };
                yield return new object[] { typeof(Foo), typeof(object), action };
                yield return new object[] { typeof(Foo), typeof(IFoo), action };
            }
        }

        [Theory]
        [MemberData(nameof(InvalidImplementationTypes))]
        public void AddWrongImplementationType(Type serviceType, Type implmentationType, Action<IServiceCollection, Type, Type> action)
        {
            var expectedMessage = $"Implementation type '{implmentationType}' cann't be assigned to service type '{serviceType}'.";
            var serviceCollection = new ServiceCollection();
            var exception = Assert.Throws<ArgumentException>(() => action(serviceCollection, serviceType, implmentationType));
            Assert.EndsWith(expectedMessage, exception.Message);
        }

        public static IEnumerable<object[]> InvalidImplementationInstances()
        {
            yield return new object[] { typeof(IFoo), new Bar() };
            yield return new object[] { typeof(IFoo), new object() };
            yield return new object[] { typeof(Foo), new object() };
        }

        [Theory]
        [MemberData(nameof(InvalidImplementationInstances))]
        public void AddWrongImplementationInstance(Type serviceType, Type implmentationInstance)
        {
            var implmentationType = implmentationInstance.GetType();
            var expectedMessage = $"Implementation type '{implmentationType}' cann't be assigned to service type '{serviceType}'.";
            var serviceCollection = new ServiceCollection();
            var exception = Assert.Throws<ArgumentException>(() => serviceCollection.AddSingleton(serviceType, implmentationInstance));
            Assert.EndsWith(expectedMessage, exception.Message);
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

        private class Bar2 : IBar
        {
            public Bar2(IBaz baz)
            {
            }
        }

        private interface IBaz
        {
        }

        private class Baz : IBaz
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
