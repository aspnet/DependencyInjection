using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNet.ConfigurationModel;

namespace Microsoft.AspNet.DependencyInjection
{
    public class ServiceCollection : IServiceCollection
    {
        private readonly List<IServiceDescriptor> _descriptors;
        private readonly ServiceDescriber _describe;

        public ServiceCollection()
            : this(new Configuration())
        {
        }

        public ServiceCollection(IConfiguration configuration)
        {
            _descriptors = new List<IServiceDescriptor>();
            _describe = new ServiceDescriber(configuration);
        }

        public IServiceCollection Add(IServiceDescriptor descriptor)
        {
            _descriptors.Add(descriptor);
            return this;
        }

        public IServiceCollection Add(IEnumerable<IServiceDescriptor> descriptors)
        {
            _descriptors.AddRange(descriptors);
            return this;
        }

        public IServiceCollection AddTransient<TService, TImplementation>()
        {
            Add(_describe.Transient<TService, TImplementation>());
            return this;
        }

        public IServiceCollection AddScoped<TService, TImplementation>()
        {
            Add(_describe.Scoped<TService, TImplementation>());
            return this;
        }

        public IServiceCollection AddSingleton<TService, TImplementation>()
        {
            Add(_describe.Singleton<TService, TImplementation>());
            return this;
        }

        public IServiceCollection AddSingleton<TService>()
        {
            AddSingleton<TService, TService>();
            return this;
        }
        
        public IServiceCollection AddTransient<TService>()
        {
            AddTransient<TService, TService>();
            return this;
        }

        public IServiceCollection AddScoped<TService>()
        {
            AddScoped<TService, TService>();
            return this;
        }

        public IServiceCollection AddInstance<TService>(TService implementationInstance)
        {
            Add(_describe.Instance<TService>(implementationInstance));
            return this;
        }

        public IEnumerator<IServiceDescriptor> GetEnumerator()
        {
            return _descriptors.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
