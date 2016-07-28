using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    public class EnumerableServiceDescriptor : ServiceDescriptor
    {
        public EnumerableServiceDescriptor(Type serviceType) : base(serviceType, ServiceLifetime.Transient)
        {
        }

        public List<ServiceDescriptor> Descriptors { get; } = new List<ServiceDescriptor>();

        internal override Type GetImplementationType()
        {
            return null;
        }
    }
}