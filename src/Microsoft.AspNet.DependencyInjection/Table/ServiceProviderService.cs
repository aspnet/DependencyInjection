using System;

namespace Microsoft.AspNet.DependencyInjection.Table
{
    internal class ServiceProviderService : IService
    {
        public IService Next { get; set; }

        public LifecycleKind Lifecycle
        {
            get { return LifecycleKind.Transient;  }
        }

        public object Create(ServiceProvider provider)
        {
            return provider;
        }
    }
}
