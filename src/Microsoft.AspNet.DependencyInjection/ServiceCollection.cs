using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNet.ConfigurationModel;

namespace Microsoft.AspNet.DependencyInjection
{
    public class ServiceCollection : IEnumerable<IServiceDescriptor>
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

        public ServiceCollection Add(IServiceDescriptor descriptor)
        {
            _descriptors.Add(_describe.Configure(descriptor));
            return this;
        }

        public ServiceCollection Add(
                IEnumerable<IServiceDescriptor> descriptors,
                params IEnumerable<IServiceDescriptor>[] moreDescriptors)
        {
            _descriptors.AddRange(descriptors.Select(_describe.Configure));

            foreach (var serviceDescriptors in moreDescriptors)
            {
                _descriptors.AddRange(serviceDescriptors.Select(_describe.Configure));
            }
            return this;
        }

        public ServiceCollection AddTransient<TService, TImplementation>()
        {
            Add(_describe.Transient<TService, TImplementation>());
            return this;
        }

        public ServiceCollection AddScoped<TService, TImplementation>()
        {
            Add(_describe.Scoped<TService, TImplementation>());
            return this;
        }

        public ServiceCollection AddSingleton<TService, TImplementation>()
        {
            Add(_describe.Singleton<TService, TImplementation>());
            return this;
        }

        public ServiceCollection AddInstance<TService>(object implementationInstance)
        {
            Add(_describe.Instance<TService>(implementationInstance));
            return this;
        }

        public IServiceProvider BuildServiceProvider()
        {
            return BuildServiceProvider(null);
        }

        public IServiceProvider BuildServiceProvider(IServiceProvider fallbackServiceProvider)
        {
            return new ServiceProvider(fallbackServiceProvider).Add(_descriptors);
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
