// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class ServiceDescriptorValidationTests
    {
        private static void AssertImplemntationTypeException(Type serviceType, Type implmentationType, Func<ServiceDescriptor> constructor)
        {
            var expectedMessage = $"Implementation type '{implmentationType}' can't be assigned to service type '{serviceType}'.";
            var exception = Assert.Throws<ArgumentException>(constructor);
            Assert.Equal(expectedMessage, exception.Message);
        }

        public static IEnumerable<object[]> InvalidImplementationTypes()
        {
            yield return new object[] { typeof(IFoo), typeof(Bar) };
            yield return new object[] { typeof(IFoo), typeof(IBar) };
            yield return new object[] { typeof(Foo), typeof(object) };
            yield return new object[] { typeof(Foo), typeof(IFoo) };
            yield return new object[] { typeof(IFooGeneric<int>), typeof(IFooGeneric<string>) };
            yield return new object[] { typeof(FooGeneric1<int>), typeof(FooGeneric1<string>) };
        }

        public static IEnumerable<object[]> InvalidImplementationInstances()
        {
            yield return new object[] { typeof(IFoo), new Bar() };
            yield return new object[] { typeof(IFoo), new object() };
            yield return new object[] { typeof(Foo), new object() };
            yield return new object[] { typeof(IFooGeneric<int>), new object() };
            yield return new object[] { typeof(FooGeneric1<int>), new object() };
        }

        [Theory]
        [MemberData(nameof(InvalidImplementationTypes))]
        public void ServiceDescriptor_Throws_WhenImplementationTypeNotMatch(Type serviceType, Type implmentationType)
        {
            AssertImplemntationTypeException(serviceType, implmentationType, () => new ServiceDescriptor(serviceType, implmentationType, ServiceLifetime.Scoped));
        }

        [Theory]
        [MemberData(nameof(InvalidImplementationInstances))]
        public void ServiceDescriptor_Throws_WhenImplementationInstanceTypeNotMatch(Type serviceType, object implmentationInstance)
        {
            var implmentationType = implmentationInstance.GetType();
            AssertImplemntationTypeException(serviceType, implmentationType, () => new ServiceDescriptor(serviceType, implmentationInstance));
        }

        private interface IFooGeneric<TValInterface> { }

        private class FooGeneric1<TValClass1> : IFooGeneric<TValClass1> { }

        private interface IFoo { }

        private class Foo : IFoo { }

        public interface IBar { }

        public class Bar : IBar { }
    }
}