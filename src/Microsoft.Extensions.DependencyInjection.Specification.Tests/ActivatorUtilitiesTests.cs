// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Specification
{
    public abstract partial class DependencyInjectionSpecificationTests
    {
        public delegate object CreateInstanceFunc(IServiceProvider provider, Type type, object[] args);

        private static object CreateInstanceDirectly(IServiceProvider provider, Type type, object[] args)
        {
            return ActivatorUtilities.CreateInstance(provider, type, args);
        }

        private static object CreateInstanceFromFactory(IServiceProvider provider, Type type, object[] args)
        {
            var factory = ActivatorUtilities.CreateFactory(type, args.Select(a => a.GetType()).ToArray());
            return factory(provider, args);
        }

        private static T CreateInstance<T>(CreateInstanceFunc func, IServiceProvider provider, params object[] args)
        {
            return (T)func(provider, typeof(T), args);
        }

        public static IEnumerable<object[]> CreateInstanceFuncs
        {
            get
            {
                yield return new[] { (CreateInstanceFunc)CreateInstanceDirectly };
                yield return new[] { (CreateInstanceFunc)CreateInstanceFromFactory };
            }
        }

        [Theory]
        [MemberData(nameof(CreateInstanceFuncs))]
        public void TypeActivatorEnablesYouToCreateAnyTypeWithServicesEvenWhenNotInIocContainer(CreateInstanceFunc createFunc)
        {
            // Arrange
            var serviceCollection = new ServiceCollection()
                .AddTransient<IFakeService, FakeService>();
            var serviceProvider = CreateServiceProvider(serviceCollection);

            var anotherClass = CreateInstance<AnotherClass>(createFunc, serviceProvider);

            Assert.NotNull(anotherClass.FakeService);
        }

        [Theory]
        [MemberData(nameof(CreateInstanceFuncs))]
        public void TypeActivatorAcceptsAnyNumberOfAdditionalConstructorParametersToProvide(CreateInstanceFunc createFunc)
        {
            // Arrange
            var serviceCollection = new ServiceCollection()
                .AddTransient<IFakeService, FakeService>();
            var serviceProvider = CreateServiceProvider(serviceCollection);

            // Act
            var anotherClass = CreateInstance<AnotherClassAcceptingData>(createFunc, serviceProvider, "1", "2");

            // Assert
            Assert.NotNull(anotherClass.FakeService);
            Assert.Equal("1", anotherClass.One);
            Assert.Equal("2", anotherClass.Two);
        }

        [Theory]
        [MemberData(nameof(CreateInstanceFuncs))]
        public void TypeActivatorWorksWithStaticCtor(CreateInstanceFunc createFunc)
        {
            // Act
            var anotherClass = CreateInstance<ClassWithStaticCtor>(createFunc, provider: null);

            // Assert
            Assert.NotNull(anotherClass);
        }

        [Theory]
        [MemberData(nameof(CreateInstanceFuncs))]
        public void TypeActivatorWorksWithCtorWithOptionalArgs(CreateInstanceFunc createFunc)
        {
            // Arrange
            var provider = new ServiceCollection();
            var serviceProvider = CreateServiceProvider(provider);

            // Act
            var anotherClass = CreateInstance<ClassWithOptionalArgsCtor>(createFunc, serviceProvider);

            // Assert
            Assert.NotNull(anotherClass);
            Assert.Equal("BLARGH", anotherClass.Whatever);
        }

        [Theory]
        [MemberData(nameof(CreateInstanceFuncs))]
        public void TypeActivatorCanDisambiguateConstructorsWithUniqueArguments(CreateInstanceFunc createFunc)
        {
            // Arrange
            var serviceCollection = new ServiceCollection()
                .AddTransient<IFakeService, FakeService>();
            var serviceProvider = CreateServiceProvider(serviceCollection);

            // Act
            var instance = CreateInstance<ClassWithAmbiguousCtors>(createFunc, serviceProvider, "1", 2);

            // Assert
            Assert.NotNull(instance);
            Assert.NotNull(instance.FakeService);
            Assert.Equal("1", instance.Data1);
            Assert.Equal(2, instance.Data2);
        }

        public static IEnumerable<object[]> TypesWithNonPublicConstructorData =>
            CreateInstanceFuncs.Zip(
                    new[] { typeof(ClassWithPrivateCtor), typeof(ClassWithInternalConstructor), typeof(ClassWithProtectedConstructor) },
                    (a, b) => new object[] { a[0], b });

        [Theory]
        [MemberData(nameof(TypesWithNonPublicConstructorData))]
        public void TypeActivatorRequiresPublicConstructor(CreateInstanceFunc createFunc, Type type)
        {
            // Arrange
            var expectedMessage = $"A suitable constructor for type '{type}' could not be located. " +
                "Ensure the type is concrete and services are registered for all parameters of a public constructor.";

            // Act and Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                createFunc(provider: null, type: type, args: new object[0]));

            Assert.Equal(expectedMessage, ex.Message);
        }

        [Theory]
        [MemberData(nameof(CreateInstanceFuncs))]
        public void TypeActivatorRequiresAllArgumentsCanBeAccepted(CreateInstanceFunc createFunc)
        {
            // Arrange
            var expectedMessage = $"A suitable constructor for type '{typeof(AnotherClassAcceptingData).FullName}' could not be located. " +
                "Ensure the type is concrete and services are registered for all parameters of a public constructor.";
            var serviceCollection = new ServiceCollection()
                .AddTransient<IFakeService, FakeService>();
            var serviceProvider = CreateServiceProvider(serviceCollection);

            var ex1 = Assert.Throws<InvalidOperationException>(() =>
                CreateInstance<AnotherClassAcceptingData>(createFunc, serviceProvider, "1", "2", "3"));
            var ex2 = Assert.Throws<InvalidOperationException>(() =>
                CreateInstance<AnotherClassAcceptingData>(createFunc, serviceProvider, 1, 2));

            Assert.Equal(expectedMessage, ex1.Message);
            Assert.Equal(expectedMessage, ex2.Message);
        }

        [Theory]
        [MemberData(nameof(CreateInstanceFuncs))]
        public void TypeActivatorRethrowsOriginalExceptionFromConstructor(CreateInstanceFunc createFunc)
        {
            // Act
            var ex1 = Assert.Throws<Exception>(() =>
                CreateInstance<ClassWithThrowingEmptyCtor>(createFunc, provider: null));

            var ex2 = Assert.Throws<Exception>(() =>
                CreateInstance<ClassWithThrowingCtor>(createFunc, provider: null, args: new[] { new FakeService() }));

            // Assert
            Assert.Equal(nameof(ClassWithThrowingEmptyCtor), ex1.Message);
            Assert.Equal(nameof(ClassWithThrowingCtor), ex2.Message);
        }

        [Theory]
        [InlineData(typeof(string))]
        [InlineData(typeof(int))]
        public void TypeActivatorCreateFactoryDoesNotAllowForAmbiguousConstructorMatches(Type paramType)
        {
            // Arrange
            var type = typeof(ClassWithAmbiguousCtors);
            var expectedMessage = $"Multiple constructors accepting all given argument types have been found in type '{type}'. " +
                "There should only be one applicable constructor.";

            // Act
            var ex = Assert.Throws<InvalidOperationException>(() =>
                ActivatorUtilities.CreateFactory(type, new[] { paramType }));

            // Assert
            Assert.Equal(expectedMessage, ex.Message);
        }

        [Fact]
        public void GetServiceOrCreateInstanceRegisteredServiceTransient()
        {
            // Reset the count because test order is not guaranteed
            lock (CreationCountFakeService.InstanceLock)
            {
                CreationCountFakeService.InstanceCount = 0;

                var serviceCollection = new ServiceCollection()
                    .AddTransient<IFakeService, FakeService>()
                    .AddTransient<CreationCountFakeService>();

                var serviceProvider = CreateServiceProvider(serviceCollection);

                var service = ActivatorUtilities.GetServiceOrCreateInstance<CreationCountFakeService>(serviceProvider);
                Assert.NotNull(service);
                Assert.Equal(1, service.InstanceId);
                Assert.Equal(1, CreationCountFakeService.InstanceCount);

                service = ActivatorUtilities.GetServiceOrCreateInstance<CreationCountFakeService>(serviceProvider);
                Assert.NotNull(service);
                Assert.Equal(2, service.InstanceId);
                Assert.Equal(2, CreationCountFakeService.InstanceCount);
            }
        }

        [Fact]
        public void GetServiceOrCreateInstanceRegisteredServiceSingleton()
        {
            lock (CreationCountFakeService.InstanceLock)
            {
                // Arrange
                // Reset the count because test order is not guaranteed
                CreationCountFakeService.InstanceCount = 0;

                var serviceCollection = new ServiceCollection()
                    .AddTransient<IFakeService, FakeService>()
                    .AddSingleton<CreationCountFakeService>();
                var serviceProvider = CreateServiceProvider(serviceCollection);

                // Act and Assert
                var service = ActivatorUtilities.GetServiceOrCreateInstance<CreationCountFakeService>(serviceProvider);
                Assert.NotNull(service);
                Assert.Equal(1, service.InstanceId);
                Assert.Equal(1, CreationCountFakeService.InstanceCount);

                service = ActivatorUtilities.GetServiceOrCreateInstance<CreationCountFakeService>(serviceProvider);
                Assert.NotNull(service);
                Assert.Equal(1, service.InstanceId);
                Assert.Equal(1, CreationCountFakeService.InstanceCount);
            }
        }

        [Fact]
        public void GetServiceOrCreateInstanceUnregisteredService()
        {
            lock (CreationCountFakeService.InstanceLock)
            {
                // Arrange
                // Reset the count because test order is not guaranteed
                CreationCountFakeService.InstanceCount = 0;

                var serviceCollection = new ServiceCollection()
                    .AddTransient<IFakeService, FakeService>();
                var serviceProvider = CreateServiceProvider(serviceCollection);

                // Act and Assert
                var service = (CreationCountFakeService)ActivatorUtilities.GetServiceOrCreateInstance(
                    serviceProvider,
                    typeof(CreationCountFakeService));
                Assert.NotNull(service);
                Assert.Equal(1, service.InstanceId);
                Assert.Equal(1, CreationCountFakeService.InstanceCount);

                service = ActivatorUtilities.GetServiceOrCreateInstance<CreationCountFakeService>(serviceProvider);
                Assert.NotNull(service);
                Assert.Equal(2, service.InstanceId);
                Assert.Equal(2, CreationCountFakeService.InstanceCount);
            }
        }

        [Theory]
        [MemberData(nameof(CreateInstanceFuncs))]
        public void UnRegisteredServiceAsConstructorParameterThrowsException(CreateInstanceFunc createFunc)
        {
            var serviceCollection = new ServiceCollection()
                .AddSingleton<CreationCountFakeService>();
            var serviceProvider = CreateServiceProvider(serviceCollection);

            var ex = Assert.Throws<InvalidOperationException>(() =>
            CreateInstance<CreationCountFakeService>(createFunc, serviceProvider));
            Assert.Equal($"Unable to resolve service for type '{typeof(IFakeService)}' while attempting" +
                $" to activate '{typeof(CreationCountFakeService)}'.",
                ex.Message);
        }
    }
}