using System.Collections;
using System.Collections.Generic;

namespace Microsoft.AspNet.DependencyInjection.Table
{
    internal class ServiceScopeService : IService
    {
        public IService Next { get; set; }

        public LifecycleKind Lifecycle
        {
            get { return LifecycleKind.Scoped; }
        }

        public object Create(ServiceProvider provider)
        {
            return new ServiceScopeFactory(provider);
        }
    }
}
