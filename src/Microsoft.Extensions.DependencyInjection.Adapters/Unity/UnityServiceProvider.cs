using System;
using Microsoft.Practices.Unity;

namespace Microsoft.Extensions.DependencyInjection.Adapters.Unity
{
    internal class UnityServiceProvider : IServiceProvider, ISupportRequiredService
    {
        private readonly IUnityContainer container;

        public UnityServiceProvider(IUnityContainer container)
        {
            this.container = container;
        }

        public object GetService(Type serviceType)
        {
            try
            {
                return container.Resolve(serviceType);
            }
            catch (ResolutionFailedException)
            {
                return null;
            }
        }

        public object GetRequiredService(Type serviceType)
        {
            return container.Resolve(serviceType);
        }
    }
}