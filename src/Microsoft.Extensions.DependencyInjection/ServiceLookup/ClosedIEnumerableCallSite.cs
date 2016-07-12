using System;
using System.Linq.Expressions;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    class ClosedIEnumerableCallSite : IServiceCallSite
    {
        internal Type ItemType { get; }
        internal IServiceCallSite[] ServiceCallSites { get; }

        public ClosedIEnumerableCallSite(Type itemType, IServiceCallSite[] serviceCallSites)
        {
            ItemType = itemType;
            ServiceCallSites = serviceCallSites;
        }

        public object Invoke(ServiceProvider provider)
        {
            var array = Array.CreateInstance(ItemType, ServiceCallSites.Length);
            for (var index = 0; index < ServiceCallSites.Length; index++)
            {
                array.SetValue(ServiceCallSites[index].Invoke(provider), index);
            }
            return array;
        }
    }
}