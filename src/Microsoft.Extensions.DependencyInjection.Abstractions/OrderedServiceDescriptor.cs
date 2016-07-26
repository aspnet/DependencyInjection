using System;

namespace Microsoft.Extensions.DependencyInjection
{
    public class OrderedServiceDescriptor: EnumerableServiceDescriptor
    {
        public OrderedServiceDescriptor(Type serviceType) : base(serviceType)
        {
        }
    }
}