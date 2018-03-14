// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Extensions.DependencyInjection.Tests
{
    public class ServiceProviderCompilationTest
    {
        [Fact]
        public async Task CompilesInLimitedStackSpace()
        {
            // Arrange
            var serviceCollection = new ServiceCollection();
            CompilationTestDataProvider.Register(serviceCollection);

            var serviceProvider = serviceCollection.BuildServiceProvider();

            // Act + Assert

            var tsc = new TaskCompletionSource<object>();
            var thread = new Thread(() =>
                {
                    try
                    {
                        object service = null;
                        for (int i = 0; i < 10; i++)
                        {
                            service = serviceProvider.GetService<I160>();
                        }
                        tsc.SetResult(service);
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
    }
}
