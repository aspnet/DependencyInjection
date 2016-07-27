// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using Xunit;

using AbstractionResources = Microsoft.Extensions.DependencyInjection.Abstractions.Resources;

namespace Microsoft.Extensions.DependencyInjection
{
    public class ServiceCollectionServiceExtensionsTest
    {
        private static readonly FakeService _instance = new FakeService();

        public static TheoryData AddImplementationTypeData
        {
            get
            {
                var serviceType = typeof(IFakeService);
                var implementationType = typeof(FakeService);
                return new TheoryData<Action<IServiceCollection>, Type, Type, ServiceLifetime>
                {
                    { collection => collection.AddTransient(serviceType, implementationType), serviceType, implementationType, ServiceLifetime.Transient },
                    { collection => collection.AddTransient<IFakeService, FakeService>(), serviceType, implementationType, ServiceLifetime.Transient },
                    { collection => collection.AddTransient<IFakeService>(), serviceType, serviceType, ServiceLifetime.Transient },
                    { collection => collection.AddTransient(implementationType), implementationType, implementationType, ServiceLifetime.Transient },

                    { collection => collection.AddScoped(serviceType, implementationType), serviceType, implementationType, ServiceLifetime.Scoped },
                    { collection => collection.AddScoped<IFakeService, FakeService>(), serviceType, implementationType, ServiceLifetime.Scoped },
                    { collection => collection.AddScoped<IFakeService>(), serviceType, serviceType, ServiceLifetime.Scoped },
                    { collection => collection.AddScoped(implementationType), implementationType, implementationType, ServiceLifetime.Scoped },

                    { collection => collection.AddSingleton(serviceType, implementationType), serviceType, implementationType, ServiceLifetime.Singleton },
                    { collection => collection.AddSingleton<IFakeService, FakeService>(), serviceType, implementationType, ServiceLifetime.Singleton },
                    { collection => collection.AddSingleton<IFakeService>(), serviceType, serviceType, ServiceLifetime.Singleton },
                    { collection => collection.AddSingleton(implementationType), implementationType, implementationType, ServiceLifetime.Singleton },
                };
            }
        }

        [Theory]
        [MemberData(nameof(AddImplementationTypeData))]
        public void AddWithTypeAddsServiceWithRightLifecyle(Action<IServiceCollection> addTypeAction,
                                                            Type expectedServiceType,
                                                            Type expectedImplementationType,
                                                            ServiceLifetime lifeCycle)
        {
            // Arrange
            var collection = new ServiceCollection();

            // Act
            addTypeAction(collection);

            // Assert
            var descriptor = Assert.Single(collection);
            Assert.Equal(expectedServiceType, descriptor.ServiceType);
            Assert.Equal(lifeCycle, descriptor.Lifetime);
        }

        public static TheoryData AddImplementationFactoryData
        {
            get
            {
                var serviceType = typeof(IFakeService);
                var implementationType = typeof(FakeService);
                var objectType = typeof(object);

                return new TheoryData<Action<IServiceCollection>, Type, Type, ServiceLifetime>
                {
                    { collection => collection.AddTransient(serviceType, s => new FakeService()), serviceType, objectType, ServiceLifetime.Transient },
                    { collection => collection.AddTransient<IFakeService>(s => new FakeService()), serviceType, serviceType, ServiceLifetime.Transient },
                    { collection => collection.AddTransient<IFakeService, FakeService>(s => new FakeService()), serviceType, implementationType, ServiceLifetime.Transient },

                    { collection => collection.AddScoped(serviceType, s => new FakeService()), serviceType, objectType, ServiceLifetime.Scoped },
                    { collection => collection.AddScoped<IFakeService>(s => new FakeService()), serviceType, serviceType, ServiceLifetime.Scoped },
                    { collection => collection.AddScoped<IFakeService, FakeService>(s => new FakeService()), serviceType, implementationType, ServiceLifetime.Scoped },

                    { collection => collection.AddSingleton(serviceType, s => new FakeService()), serviceType, objectType, ServiceLifetime.Singleton },
                    { collection => collection.AddSingleton<IFakeService>(s => new FakeService()), serviceType, serviceType, ServiceLifetime.Singleton },
                    { collection => collection.AddSingleton<IFakeService, FakeService>(s => new FakeService()), serviceType, implementationType, ServiceLifetime.Singleton },
                };
            }
        }

        [Theory]
        [MemberData(nameof(AddImplementationFactoryData))]
        public void AddWithFactoryAddsServiceWithRightLifecyle(
            Action<IServiceCollection> addAction,
            Type serviceType,
            Type implementationType,
            ServiceLifetime lifeCycle)
        {
            // Arrange
            var collection = new ServiceCollection();

            // Act
            addAction(collection);

            // Assert
            var descriptor = Assert.Single(collection);
            Assert.Equal(serviceType, descriptor.ServiceType);
            Assert.Equal(implementationType, descriptor.GetImplementationType());
            Assert.Equal(lifeCycle, descriptor.Lifetime);
        }

        public static TheoryData AddSingletonData
        {
            get
            {
                return new TheoryData<Action<IServiceCollection>>
                {
                    { collection => collection.AddSingleton<IFakeService>(_instance) },
                    { collection => collection.AddSingleton(typeof(IFakeService), _instance) },
                };
            }
        }

        [Theory]
        [MemberData(nameof(AddSingletonData))]
        public void AddSingleton_AddsWithSingletonLifecycle(Action<IServiceCollection> addAction)
        {
            // Arrange
            var collection = new ServiceCollection();

            // Act
            addAction(collection);

            // Assert
            var descriptor = Assert.Single(collection);
            var instanceDescriptor = Assert.IsType<InstanceServiceDescriptor>(descriptor);

            Assert.Equal(typeof(IFakeService), instanceDescriptor.ServiceType);
            Assert.Same(_instance, instanceDescriptor.ImplementationInstance);
            Assert.Equal(ServiceLifetime.Singleton, instanceDescriptor.Lifetime);
        }

        [Theory]
        [MemberData(nameof(AddSingletonData))]
        public void TryAddNoOpFailsIfExists(Action<IServiceCollection> addAction)
        {
            // Arrange
            var collection = new ServiceCollection();
            addAction(collection);
            var d = TypeServiceDescriptor.Transient<IFakeService, FakeService>();

            // Act
            collection.TryAdd(d);

            // Assert
            var descriptor = Assert.Single(collection);
            var instanceDescriptor = Assert.IsType<InstanceServiceDescriptor>(descriptor);

            Assert.Equal(typeof(IFakeService), instanceDescriptor.ServiceType);
            Assert.Same(_instance, instanceDescriptor.ImplementationInstance);
            Assert.Equal(ServiceLifetime.Singleton, instanceDescriptor.Lifetime);
        }

        public static TheoryData TryAddImplementationTypeData
        {
            get
            {
                var serviceType = typeof(IFakeService);
                var implementationType = typeof(FakeService);
                return new TheoryData<Action<IServiceCollection>, Type, Type, ServiceLifetime>
                {
                    { collection => collection.TryAddTransient(serviceType, implementationType), serviceType, implementationType, ServiceLifetime.Transient },
                    { collection => collection.TryAddTransient<IFakeService, FakeService>(), serviceType, implementationType, ServiceLifetime.Transient },
                    { collection => collection.TryAddTransient<IFakeService>(), serviceType, serviceType, ServiceLifetime.Transient },
                    { collection => collection.TryAddTransient(implementationType), implementationType, implementationType, ServiceLifetime.Transient },

                    { collection => collection.TryAddScoped(serviceType, implementationType), serviceType, implementationType, ServiceLifetime.Scoped },
                    { collection => collection.TryAddScoped<IFakeService, FakeService>(), serviceType, implementationType, ServiceLifetime.Scoped },
                    { collection => collection.TryAddScoped<IFakeService>(), serviceType, serviceType, ServiceLifetime.Scoped },
                    { collection => collection.TryAddScoped(implementationType), implementationType, implementationType, ServiceLifetime.Scoped },

                    { collection => collection.TryAddSingleton(serviceType, implementationType), serviceType, implementationType, ServiceLifetime.Singleton },
                    { collection => collection.TryAddSingleton<IFakeService, FakeService>(), serviceType, implementationType, ServiceLifetime.Singleton },
                    { collection => collection.TryAddSingleton<IFakeService>(), serviceType, serviceType, ServiceLifetime.Singleton },
                    { collection => collection.TryAddSingleton(implementationType), implementationType, implementationType, ServiceLifetime.Singleton },
                };
            }
        }

        [Theory]
        [MemberData(nameof(TryAddImplementationTypeData))]
        public void TryAdd_WithType_AddsService(
            Action<IServiceCollection> addAction,
            Type expectedServiceType,
            Type expectedImplementationType,
            ServiceLifetime expectedLifetime)
        {
            // Arrange
            var collection = new ServiceCollection();

            // Act
            addAction(collection);

            // Assert
            var descriptor = Assert.Single(collection);
            var typeServiceDescriptor = Assert.IsType<TypeServiceDescriptor>(descriptor);

            Assert.Equal(expectedServiceType, typeServiceDescriptor.ServiceType);
            Assert.Same(expectedImplementationType, typeServiceDescriptor.ImplementationType);
            Assert.Equal(expectedLifetime, typeServiceDescriptor.Lifetime);
        }

        [Theory]
        [MemberData(nameof(TryAddImplementationTypeData))]
        public void TryAdd_WithType_DoesNotAddDuplicate(
            Action<IServiceCollection> addAction,
            Type expectedServiceType,
            Type expectedImplementationType,
            ServiceLifetime expectedLifetime)
        {
            // Arrange
            var collection = new ServiceCollection();
            collection.Add(TypeServiceDescriptor.Transient(expectedServiceType, expectedServiceType));

            // Act
            addAction(collection);

            // Assert
            var descriptor = Assert.Single(collection);
            var typeServiceDescriptor = Assert.IsType<TypeServiceDescriptor>(descriptor);

            Assert.Equal(expectedServiceType, typeServiceDescriptor.ServiceType);
            Assert.Same(expectedServiceType, typeServiceDescriptor.ImplementationType);
            Assert.Equal(ServiceLifetime.Transient, typeServiceDescriptor.Lifetime);
        }

        [Fact]
        public void TryAddIfMissingActuallyAdds()
        {
            // Arrange
            var collection = new ServiceCollection();
            var d = TypeServiceDescriptor.Transient<IFakeService, FakeService>();

            // Act
            collection.TryAdd(d);

            // Assert
            var descriptor = Assert.Single(collection);
            var typeServiceDescriptor = Assert.IsType<TypeServiceDescriptor>(descriptor);

            Assert.Equal(typeof(IFakeService), typeServiceDescriptor.ServiceType);
            Assert.Same(typeof(FakeService), typeServiceDescriptor.ImplementationType);
            Assert.Equal(ServiceLifetime.Transient, typeServiceDescriptor.Lifetime);
        }

        public static TheoryData TryAddEnumerableImplementationTypeData
        {
            get
            {
                var serviceType = typeof(IFakeService);
                var implementationType = typeof(FakeService);
                return new TheoryData<ServiceDescriptor, Type, Type, ServiceLifetime>
                {
                    { TypeServiceDescriptor.Transient<IFakeService, FakeService>(), serviceType, implementationType, ServiceLifetime.Transient },
                    { FactoryServiceDescriptor.Transient<IFakeService, FakeService>(s => new FakeService()), serviceType, implementationType, ServiceLifetime.Transient },

                    { TypeServiceDescriptor.Scoped<IFakeService, FakeService>(), serviceType, implementationType, ServiceLifetime.Scoped },
                    { FactoryServiceDescriptor.Scoped<IFakeService, FakeService>(s => new FakeService()), serviceType, implementationType, ServiceLifetime.Scoped },

                    { TypeServiceDescriptor.Singleton<IFakeService, FakeService>(), serviceType, implementationType, ServiceLifetime.Singleton },
                    { FactoryServiceDescriptor.Singleton<IFakeService, FakeService >(s => new FakeService()), serviceType, implementationType, ServiceLifetime.Singleton },

                    { InstanceServiceDescriptor.Singleton<IFakeService>(_instance), serviceType, implementationType, ServiceLifetime.Singleton },
                };
            }
        }

        [Theory]
        [MemberData(nameof(TryAddEnumerableImplementationTypeData))]
        public void TryAddEnumerable_AddsService(
            ServiceDescriptor descriptor,
            Type expectedServiceType,
            Type expectedImplementationType,
            ServiceLifetime expectedLifetime)
        {
            // Arrange
            var collection = new ServiceCollection();

            // Act
            collection.TryAddEnumerable(descriptor);

            // Assert
            descriptor = Assert.Single(collection);
            var enumerableDesctiptor = Assert.IsType<EnumerableServiceDescriptor>(descriptor);
            var d = Assert.Single(enumerableDesctiptor.Descriptors);

            Assert.Equal(expectedServiceType, d.ServiceType);
            Assert.Equal(expectedImplementationType, d.GetImplementationType());
            Assert.Equal(expectedLifetime, d.Lifetime);
        }


        [Theory]
        [MemberData(nameof(TryAddEnumerableImplementationTypeData))]
        public void TryAddEnumerable_DoesNotAddDuplicate(
            ServiceDescriptor descriptor,
            Type expectedServiceType,
            Type expectedImplementationType,
            ServiceLifetime expectedLifetime)
        {
            // Arrange
            var collection = new ServiceCollection();
            collection.TryAddEnumerable(descriptor);

            // Act
            collection.TryAddEnumerable(descriptor);

            // Assert
            descriptor = Assert.Single(collection);
            var enumerableDesctiptor = Assert.IsType<EnumerableServiceDescriptor>(descriptor);
            var d = Assert.Single(enumerableDesctiptor.Descriptors);

            Assert.Equal(expectedServiceType, d.ServiceType);
            Assert.Equal(expectedImplementationType, d.GetImplementationType());
            Assert.Equal(expectedLifetime, d.Lifetime);
        }

        public static TheoryData TryAddEnumerableInvalidImplementationTypeData
        {
            get
            {
                var serviceType = typeof(IFakeService);
                var objectType = typeof(object);

                return new TheoryData<ServiceDescriptor, Type, Type>
                {
                    { FactoryServiceDescriptor.Transient<IFakeService>(s => new FakeService()), serviceType, serviceType },
                    { FactoryServiceDescriptor.Transient(serviceType, s => new FakeService()), serviceType, objectType },

                    { FactoryServiceDescriptor.Scoped<IFakeService>(s => new FakeService()), serviceType, serviceType },
                    { FactoryServiceDescriptor.Scoped(serviceType, s => new FakeService()), serviceType, objectType },

                    { FactoryServiceDescriptor.Singleton<IFakeService>(s => new FakeService()), serviceType, serviceType },
                    { FactoryServiceDescriptor.Singleton(serviceType, s => new FakeService()), serviceType, objectType },
                };
            }
        }

        [Theory]
        [MemberData(nameof(TryAddEnumerableInvalidImplementationTypeData))]
        public void TryAddEnumerable_ThrowsWhenAddingIndistinguishableImplementationType(
            ServiceDescriptor descriptor,
            Type serviceType,
            Type implementationType)
        {
            // Arrange
            var collection = new ServiceCollection();

            // Act & Assert
            ExceptionAssert.ThrowsArgument(
                () => collection.TryAddEnumerable(descriptor),
                "descriptor",
                AbstractionResources.FormatTryAddIndistinguishableTypeToEnumerable(implementationType, serviceType));
        }

        [Fact]
        public void AddSequence_AddsServicesToCollection()
        {
            // Arrange
            var collection = new ServiceCollection();
            var descriptor1 = TypeServiceDescriptor.Transient<IFakeService, FakeService>();
            var descriptor2 = TypeServiceDescriptor.Transient<IFakeOuterService, FakeOuterService>();
            var descriptors = new[] { descriptor1, descriptor2 };

            // Act
            var result = collection.Add(descriptors);

            // Assert
            Assert.Equal(descriptors, collection);
        }

        [Fact]
        public void Replace_AddsServiceIfServiceTypeIsNotRegistered()
        {
            // Arrange
            var collection = new ServiceCollection();
            var descriptor1 = TypeServiceDescriptor.Transient<IFakeService, FakeService>();
            var descriptor2 = TypeServiceDescriptor.Transient<IFakeOuterService, FakeOuterService>();
            collection.Add(descriptor1);

            // Act
            collection.Replace(descriptor2);

            // Assert
            Assert.Equal(new[] { descriptor1, descriptor2 }, collection);
        }

        [Fact]
        public void Replace_ReplacesFirstServiceWithMatchingServiceType()
        {
            // Arrange
            var collection = new ServiceCollection();
            var descriptor1 = TypeServiceDescriptor.Transient<IFakeService, FakeService>();
            collection.Add(descriptor1);
            var descriptor2 = TypeServiceDescriptor.Singleton<IFakeService, FakeService>();

            // Act
            collection.Replace(descriptor2);

            // Assert
            Assert.Equal(new[] { descriptor2 }, collection);
        }

        [Fact]
        public void Add_ThrowsWhenAddingMultipleWithSameType()
        {
            // Arrange
            var collection = new ServiceCollection();
            var descriptor1 = TypeServiceDescriptor.Transient<IFakeService, FakeService>();
            collection.Add(descriptor1);
            var descriptor2 = TypeServiceDescriptor.Singleton<IFakeService, FakeService>();

            // Act + Assert
            var exception = Assert.Throws<InvalidOperationException>(()=> collection.Add(descriptor2));
            Assert.Equal(exception.Message,
                $"There is already descriptor with service type '{typeof(IFakeService)}' registered.");
        }


    }
}