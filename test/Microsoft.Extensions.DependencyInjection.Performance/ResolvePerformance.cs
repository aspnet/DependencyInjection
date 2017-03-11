using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using BenchmarkDotNet.Attributes;

namespace Microsoft.Extensions.DependencyInjection.Performance
{
    [Config(typeof(CoreConfig))]
    public class ResolvePerformance
    {
        private const int OperationsPerInvoke = 50000;

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

        [Benchmark(Baseline = true, OperationsPerInvoke = OperationsPerInvoke)]
        public void NoDI()
        {
            var temp = new A(new B(new C()));
            temp.Foo();
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Transient()
        {
            var temp = _transientSp.GetService<A>();
            temp.Foo();
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Scoped()
        {
            var temp = _scopedSp.ServiceProvider.GetService<A>();
            temp.Foo();
        }

        [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
        public void Singleton()
        {
            var temp = _singletonSp.GetService<A>();
            temp.Foo();
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
