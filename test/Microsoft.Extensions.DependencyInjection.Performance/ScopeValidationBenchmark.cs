using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace Microsoft.Extensions.DependencyInjection.Performance
{
    [Config(typeof(CoreConfig))]
    public class ScopeValidationBenchmark
    {
        private const int OperationsPerInvoke = 50000;

        private IServiceProvider _transientSp;
        private IServiceProvider _transientSpScopeValidation;
        private IServiceScope _scopedSpScopeValidation;
        private IServiceProvider _singletonSpScopeValidation;

        [Setup]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddTransient<A>();
            services.AddTransient<B>();
            services.AddTransient<C>();
            _transientSp = services.BuildServiceProvider();

            services = new ServiceCollection();
            services.AddTransient<A>();
            services.AddTransient<B>();
            services.AddTransient<C>();
            _transientSpScopeValidation = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });


            services = new ServiceCollection();
            services.AddScoped<A>();
            services.AddScoped<B>();
            services.AddScoped<C>();
            _scopedSpScopeValidation = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true }).CreateScope();


            services = new ServiceCollection();
            services.AddSingleton<A>();
            services.AddSingleton<B>();
            services.AddSingleton<C>();
            _singletonSpScopeValidation = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        }

        [Benchmark(Baseline = true, OperationsPerInvoke = OperationsPerInvoke)]
        public void Transient()
        {
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                var temp = _transientSp.GetService<A>();
                temp.Foo();
            }
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void TransientWithScopeValidation()
        {
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                var temp = _transientSpScopeValidation.GetService<A>();
                temp.Foo();
            }
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void ScopedWithScopeValidation()
        {
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                var temp = _scopedSpScopeValidation.ServiceProvider.GetService<A>();
                temp.Foo();
            }
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void SingletonWithScopeValidation()
        {
            for (int i = 0; i < OperationsPerInvoke; i++)
            {
                var temp = _singletonSpScopeValidation.GetService<A>();
                temp.Foo();
            }
        }

        private class A
        {
            public A(B b)
            {

            }

            [MethodImpl(MethodImplOptions.NoInlining)]
            public void Foo()
            {

            }
        }

        private class B
        {
            public B(C c)
            {

            }
        }

        private class C
        {

        }
    }
}
