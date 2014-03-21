using System;

namespace Microsoft.AspNet.DependencyInjection.Table
{
    internal interface IGenericService
    {
        LifecycleKind Lifecycle { get; }

        IService GetService(Type closedServiceType);
    }
}
