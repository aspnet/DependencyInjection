// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Microsoft.Extensions.DependencyInjection.ServiceLookup;
using Microsoft.Extensions.DependencyInjection.Specification.Fakes;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class CallSiteTests
    {
        private static readonly CallSiteRuntimeResolver CallSiteRuntimeResolver = new CallSiteRuntimeResolver();

        [Fact]
        public void BuiltExpressionCanResolveNestedScopedService()
        {
            var descriptors = new ServiceCollection();
            descriptors.AddScoped<ServiceA>();
            descriptors.AddScoped<ServiceB>();
            descriptors.AddScoped<ServiceC>();

            var provider = new ServiceProvider(descriptors, validateScopes: true);
            var callSite = provider.GetServiceCallSite(typeof(ServiceC), new HashSet<Type>());
            var compiledCallSite = CompileCallSite(callSite);

            var serviceC = (ServiceC)compiledCallSite(provider);

            Assert.NotNull(serviceC.ServiceB.ServiceA);
            Assert.Equal(serviceC, Invoke(callSite, provider));
        }

        [Fact]
        public void BuiltExpressionRethrowsOriginalExceptionFromConstructor()
        {
            var descriptors = new ServiceCollection();
            descriptors.AddTransient<ClassWithThrowingEmptyCtor>();
            descriptors.AddTransient<ClassWithThrowingCtor>();
            descriptors.AddTransient<IFakeService, FakeService>();

            var provider = new ServiceProvider(descriptors, validateScopes: true);

            var callSite1 = provider.GetServiceCallSite(typeof(ClassWithThrowingEmptyCtor), new HashSet<Type>());
            var compiledCallSite1 = CompileCallSite(callSite1);

            var callSite2 = provider.GetServiceCallSite(typeof(ClassWithThrowingCtor), new HashSet<Type>());
            var compiledCallSite2 = CompileCallSite(callSite2);

            var ex1 = Assert.Throws<Exception>(() => compiledCallSite1(provider));
            Assert.Equal(nameof(ClassWithThrowingEmptyCtor), ex1.Message);

            var ex2 = Assert.Throws<Exception>(() => compiledCallSite2(provider));
            Assert.Equal(nameof(ClassWithThrowingCtor), ex2.Message);
        }

        private class ServiceA
        {
        }

        private class ServiceB
        {
            public ServiceB(ServiceA serviceA)
            {
                ServiceA = serviceA;
            }

            public ServiceA ServiceA { get; set; }
        }

        private class ServiceC
        {
            public ServiceC(ServiceB serviceB)
            {
                ServiceB = serviceB;
            }

            public ServiceB ServiceB { get; set; }
        }

        private static object Invoke(IServiceCallSite callSite, ServiceProvider provider)
        {
            return CallSiteRuntimeResolver.Resolve(callSite, provider);
        }

        private static Func<ServiceProvider, object> CompileCallSite(IServiceCallSite callSite)
        {
            return new CallSiteExpressionBuilder(CallSiteRuntimeResolver).Build(callSite);
        }
    }
}