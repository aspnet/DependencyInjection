using System;
using System.Collections.Generic;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace Microsoft.Extensions.DependencyInjection.Performance
{
    [Config(typeof(CoreConfig))]
    public class ResolvePerformance
    {
        private IServiceProvider _transientSp;
        private IServiceScope _scopedSp;
        private IServiceProvider _singletonSp;

        [Setup]
        public void Setup()
        {
            var services = new ServiceCollection();
            services.AddTransient<A>();
            services.AddTransient<B>();
            services.AddTransient<C>();
            _transientSp = services.BuildServiceProvider();


            services = new ServiceCollection();
            services.AddScoped<A>();
            services.AddScoped<B>();
            services.AddScoped<C>();
            _scopedSp = services.BuildServiceProvider().CreateScope();


            services = new ServiceCollection();
            services.AddSingleton<A>();
            services.AddSingleton<B>();
            services.AddSingleton<C>();
            _singletonSp = services.BuildServiceProvider();
        }

        [Benchmark(Baseline = true, OperationsPerInvoke = 5000)]
        public void NoDI()
        {
            new A(new B(new C()));
        }

        [Benchmark(OperationsPerInvoke = 5000)]
        public void Transient()
        {
            _transientSp.GetService<A>();
        }

        [Benchmark(OperationsPerInvoke = 5000)]
        public void Scoped()
        {
            _scopedSp.ServiceProvider.GetService<A>();
        }

        [Benchmark(OperationsPerInvoke = 5000)]
        public void Singleton()
        {
            _singletonSp.GetService<A>();
        }

        private class A
        {
            public A(B b)
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
