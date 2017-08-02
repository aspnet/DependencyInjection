// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using BenchmarkDotNet.Running;

namespace Microsoft.Extensions.DependencyInjection.Performance
{
    class Program
    {
        static void Main(string[] args)
        {
            //var getServiceBenchmark = new TimeToFirstServiceBenchmark();
            //getServiceBenchmark.Mode = ServiceProviderMode.Compiled;
            //getServiceBenchmark.SetupTransient();
            //for (int i = 0; i < 10000; i++)
            //{
            //    getServiceBenchmark.SetupTransientIteration();
            //    getServiceBenchmark.Transient();
            //}
            BenchmarkSwitcher.FromAssembly(typeof(Program).GetTypeInfo().Assembly).Run(args);
        }
    }
}