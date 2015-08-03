using StructureMap;

namespace Microsoft.Framework.DependencyInjection.Structuremap
{
    public class StructureMapServiceScopeFactory : IServiceScopeFactory
    {
        private readonly IContainer _container;

        public StructureMapServiceScopeFactory(IContainer container)
        {
            _container = container;
        }

        public IServiceScope CreateScope()
        {
            return new StructureMapServiceScope(_container.GetNestedContainer());
        }
    }
}