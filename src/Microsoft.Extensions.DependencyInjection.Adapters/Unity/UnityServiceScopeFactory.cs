using Microsoft.Practices.Unity;

namespace Microsoft.Extensions.DependencyInjection.Adapters.Unity
{
    internal class UnityServiceScopeFactory : IServiceScopeFactory
    {
        private readonly IUnityContainer container;

        public UnityServiceScopeFactory(IUnityContainer container)
        {
            this.container = container;
        }

        public IServiceScope CreateScope()
        {
            return new UnityServiceScope(CreateChildContainer());
        }

        private IUnityContainer CreateChildContainer()
        {
            var child = container.CreateChildContainer();
            return child;
        }
    }
}