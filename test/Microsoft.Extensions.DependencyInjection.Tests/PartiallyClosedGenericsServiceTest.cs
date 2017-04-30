using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.Extensions.DependencyInjection.Specification;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection.ServiceLookup;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class PartiallyClosedGenericsServiceTest : DependencyInjectionSpecificationTests
    {
        protected override IServiceProvider CreateServiceProvider(IServiceCollection serviceCollection) =>
               serviceCollection.BuildServiceProvider();

        public interface ITestInterface1<TI1, TI2, TI3>
        {

        }

        public class TestClass1<TC1, TC2> : ITestInterface1<TC1, TC2, TC1>
        {

        }

        public class TestClass2<TC1> : ITestInterface1<TC1, List<TC1>, Dictionary<string, HashSet<TC1>>>
        {

        }

        [Fact]
        public void TestDefault()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient(
                typeof(TestClass1<,>).GetTypeInfo().GetInterfaces().Single(),
                typeof(TestClass1<,>));
            serviceCollection.AddTransient(
                typeof(TestClass2<>).GetTypeInfo().GetInterfaces().Single(),
                typeof(TestClass2<>));

            var provider = serviceCollection.BuildServiceProvider();
            Assert.IsType(typeof(TestClass1<int, int>), 
                provider.GetRequiredService<ITestInterface1<int, int, int>>());
            Assert.IsType(typeof(TestClass1<string, int>), 
                provider.GetRequiredService<ITestInterface1<string, int, string>>());
            Assert.Null(provider.GetService<ITestInterface1<int, double, string>>());

            Assert.IsType(typeof(TestClass2<string>), 
                provider.GetRequiredService<ITestInterface1<string, List<string>, Dictionary<string, HashSet<string>>>>());
            Assert.Null(provider.GetService<ITestInterface1<string, List<string>, Dictionary<object, HashSet<string>>>>());
            Assert.Null(provider.GetService<ITestInterface1<string, List<string>, Dictionary<string, HashSet<int>>>>());
            Assert.Null(provider.GetService<ITestInterface1<string, List<int>, Dictionary<string, HashSet<string>>>>());
            Assert.Null(provider.GetService<ITestInterface1<int, List<string>, Dictionary<string, HashSet<string>>>>());
        }

        public class TestClass3<TC1> : ITestInterface1<TC1, List<TC1>, Dictionary<string, HashSet<TC1>>>
            where TC1 : class
        {

        }

        public class TestClass4<TC1> : ITestInterface1<TC1, List<TC1>, Dictionary<string, HashSet<TC1>>>
            where TC1 : struct
        {

        }

        public class TestClass5<TC1> : ITestInterface1<TC1, List<TC1>, Dictionary<string, HashSet<TC1>>>
            where TC1 : class, IDisposable
        {

        }

        public class TestClass6<TC1> : ITestInterface1<TC1, List<TC1>, Dictionary<string, HashSet<TC1>>>
            where TC1 : class, IDisposable, new()
        {

        }

        public class TestClass7<TC1> : ITestInterface1<TC1, List<TC1>, Dictionary<string, HashSet<TC1>>>
            where TC1 : struct, IDisposable
        {

        }

        public class SimpleDisposableClass : IDisposable
        {
            public SimpleDisposableClass(int _) { }

            public void Dispose()
            {
                
            }
        }

        public class SimpleDisposableClass_New : IDisposable
        {
            public void Dispose()
            {

            }
        }

        public struct SimpleDisposableValue : IDisposable
        {
            public void Dispose()
            {

            }
        }

        [Fact]
        public void TestGenericParameterConstraint()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient(
                typeof(TestClass3<>).GetTypeInfo().GetInterfaces().Single(),
                typeof(TestClass3<>));
            serviceCollection.AddTransient(
                typeof(TestClass4<>).GetTypeInfo().GetInterfaces().Single(),
                typeof(TestClass4<>));
            serviceCollection.AddTransient(
                typeof(TestClass5<>).GetTypeInfo().GetInterfaces().Single(),
                typeof(TestClass5<>));
            serviceCollection.AddTransient(
                typeof(TestClass6<>).GetTypeInfo().GetInterfaces().Single(),
                typeof(TestClass6<>));
            serviceCollection.AddTransient(
                typeof(TestClass7<>).GetTypeInfo().GetInterfaces().Single(),
                typeof(TestClass7<>));

            var provider = serviceCollection.BuildServiceProvider();

            Assert.IsType(typeof(TestClass4<int>),
                provider.GetRequiredService<ITestInterface1<int, List<int>, Dictionary<string, HashSet<int>>>>());

            Assert.IsType(typeof(TestClass3<string>),
                provider.GetRequiredService<
                    ITestInterface1<string,
                    List<string>,
                    Dictionary<string, HashSet<string>>>>());

            Assert.IsType(typeof(TestClass6<SimpleDisposableClass_New>),
                provider.GetRequiredService<
                    ITestInterface1<SimpleDisposableClass_New,
                    List<SimpleDisposableClass_New>,
                    Dictionary<string, HashSet<SimpleDisposableClass_New>>>>());

            Assert.IsType(typeof(TestClass5<SimpleDisposableClass>),
                provider.GetRequiredService<
                    ITestInterface1<SimpleDisposableClass,
                    List<SimpleDisposableClass>,
                    Dictionary<string, HashSet<SimpleDisposableClass>>>>());

            Assert.IsType(typeof(TestClass7<SimpleDisposableValue>),
                provider.GetRequiredService<
                    ITestInterface1<SimpleDisposableValue,
                    List<SimpleDisposableValue>,
                    Dictionary<string, HashSet<SimpleDisposableValue>>>>());
        }

        [Fact]
        public void TestBuilder()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddTransient(
                typeof(TestClass1<,>).GetTypeInfo().GetInterfaces().Single(),
                typeof(TestClass2<>));
            Assert.Throws<ArgumentException>(() => serviceCollection.BuildServiceProvider());
        }
    }
}
