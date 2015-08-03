using System;
using StructureMap;

namespace Microsoft.Framework.DependencyInjection.Structuremap
{
    public class StructureMapServiceScope : IServiceScope
    {
        public StructureMapServiceScope(IContainer container)
        {
            Container = container;
            ServiceProvider = container.GetInstance<IServiceProvider>();
        }

        private IContainer Container { get; }

        public void Dispose()
        {
            Container.Dispose();
        }

        public IServiceProvider ServiceProvider { get; }
    }
}