using System;
using Microsoft.Extensions.DependencyInjection.ServiceLookup;

namespace Microsoft.Extensions.DependencyInjection
{
    internal interface IServiceProviderEngine : IDisposable
    {
        object GetService(Type serviceType);

        IServiceScope RootScope { get; }
        event Action<Type, IServiceCallSite> OnCreate;
        event Action<Type, IServiceScope> OnResolve;
    }
}