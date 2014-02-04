
using System;

namespace Microsoft.AspNet.DependencyInjection
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class DefaultServiceAttribute : Attribute
    {
        public DefaultServiceAttribute(Type serviceType, Type implementationType)
        {
            ServiceType = serviceType;
            ImplementationType = implementationType;
        }

        public Type ImplementationType { get; private set; }

        public Type ServiceType { get; private set; }
    }
}
