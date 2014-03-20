using System;

namespace Microsoft.AspNet.DependencyInjection.Table
{
    internal interface IService
    {
        IService Next { get; set; }

        LifecycleKind Lifecycle { get; }

        object Create(ServiceProvider provider);

    }
}
