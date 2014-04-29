using System.Collections.Generic;
//using Microsoft.Net.Runtime;

namespace Microsoft.AspNet.DependencyInjection
{
    //[AssemblyNeutral]
    public interface IServiceCollection : IEnumerable<IServiceDescriptor>
    {
        IServiceCollection Add(IServiceDescriptor descriptor);

        IServiceCollection Add(IEnumerable<IServiceDescriptor> descriptors);

        IServiceCollection AddTransient<TService, TImplementation>();

        IServiceCollection AddScoped<TService, TImplementation>();

        IServiceCollection AddSingleton<TService, TImplementation>();

        IServiceCollection AddSingleton<TService>();

        IServiceCollection AddTransient<TService>();

        IServiceCollection AddScoped<TService>();

    }
}
