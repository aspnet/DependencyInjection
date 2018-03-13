// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class ServiceProviderCompilationTest
    {
        [Theory]
        [MemberData(nameof(Depth))]
        public async Task CompilesInLimitedStackSpace(int size)
        {
            Thread.Sleep(300);
            // Arrange
            var serviceCollection = new ServiceCollection();
            CompilationTestDataProvider.Register(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider(new ServiceProviderOptions { Mode = ServiceProviderMode.Compiled });

            // Act + Assert

            var tsc = new TaskCompletionSource<object>();
            var thread = new Thread(() =>
                {
                    try
                    {
                        tsc.SetResult(serviceProvider.GetService(Type.GetType(typeof(I999).FullName.Replace("999", size.ToString()))));
                    }
                    catch (Exception ex)
                    {
                        tsc.SetException(ex);
                    }
                }, 256 * 1024);
            thread.Start();
            thread.Join();
            await tsc.Task;
        }

        public static IEnumerable<object[]> Depth => Enumerable.Range(0, 999).Select(i => new object[] { i });
    }
}
