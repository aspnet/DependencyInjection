using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public class DefaultServiceProviderFactory : ServiceProviderFactory<IServiceCollection>
    {
        public override IServiceCollection CreateBuilder(IServiceCollection services)
        {
            return services;
        }

        public override IServiceProvider CreateServiceProvider(IServiceCollection containerBuilder)
        {
            return containerBuilder.BuildServiceProvider();
        }
    }
}
