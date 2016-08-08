using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides an abstract base for creating a container specific builder and an <see cref="IServiceProvider"/>.
    /// </summary>
    public abstract class ServiceProviderFactory<TContainerBuilder> : IServiceProviderFactory
    {
        /// <summary>
        /// Creates a container builder from an <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The collection of services</param>
        /// <returns>A container builder that can be used to create an <see cref="IServiceProvider"/>.</returns>
        public abstract TContainerBuilder CreateBuilder(IServiceCollection services);

        /// <summary>
        /// Creates an <see cref="IServiceProvider"/> from the container builder.
        /// </summary>
        /// <param name="containerBuilder">The container builder</param>
        /// <returns>An <see cref="IServiceProvider"/></returns>
        public abstract IServiceProvider CreateServiceProvider(TContainerBuilder containerBuilder);

        object IServiceProviderFactory.CreateBuilder(IServiceCollection services)
        {
            return CreateBuilder(services);
        }

        IServiceProvider IServiceProviderFactory.CreateServiceProvider(object containerBuilder)
        {
            return CreateServiceProvider((TContainerBuilder)containerBuilder);
        }
    }
}
