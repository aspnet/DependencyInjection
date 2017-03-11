using System;
using BenchmarkDotNet.Running;

namespace Microsoft.Extensions.DependencyInjection.Performance
{
    class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<ResolvePerformance>();
        }
    }
}