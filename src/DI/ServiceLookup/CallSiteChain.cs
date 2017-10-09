using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal class CallSiteChain// : HashSet<Type>
    {
        private enum ChainItemType
        {
            Unknown,
            InstanceUse,
            ConstructorCall,
            FactoryCall,
            EnumerableCreate
        }

        private struct ChainItemInfo
        {
            public int Order { get; }
            public Type ImplementationType { get; }
            public ChainItemType Type { get; }

            public ChainItemInfo(int order) : this(order, ChainItemType.Unknown, null)
            {
            }

            public ChainItemInfo(int order, ChainItemType type, Type implementationType) 
            {
                Order = order;
                ImplementationType = implementationType;
                Type = type;
            }
        }
        
        //private readonly HashSet<Type> chain = new HashSet<Type>();
        //private readonly System.Collections.Specialized.OrderedDictionary dictionary = new OrderedDictionary();
        private readonly Dictionary<Type,ChainItemInfo> callSiteChain;
        
        public CallSiteChain()
        {
            callSiteChain = new Dictionary<Type, ChainItemInfo>();
        }

        public void Add(Type serviceType)
        {
            // If serviceType already present in call Site Chain throw CircularDependencyException
            if (callSiteChain.ContainsKey(serviceType))
            {
                //throw new InvalidOperationException(Resources.FormatCircularDependencyException(serviceType));
                throw new InvalidOperationException(CreateCircularDependencyExceptionMessage(serviceType));
            }

            callSiteChain.Add(serviceType, new ChainItemInfo(callSiteChain.Count));
        }

        public void Remove(Type serviceType)
        {
            callSiteChain.Remove(serviceType);
        }

        private void SetImplementationType(Type serviceType, Type implementationType, ChainItemType chainItemType)
        {
            if (!callSiteChain.TryGetValue(serviceType, out var info))
                return;

            callSiteChain[serviceType] = new ChainItemInfo(info.Order, chainItemType, implementationType);
        }

        public void SetInstanceImplementationType(Type serviceType, object instance)
        {
            SetImplementationType(serviceType, instance.GetType(), ChainItemType.InstanceUse);
        }

        public void SetEnumerableImplementationType(Type serviceType, Type implementationType)
        {
            SetImplementationType(serviceType, implementationType.MakeArrayType(), ChainItemType.EnumerableCreate);
        }

        public void SetConstructorCallImplementationType(Type serviceType, Type implementationType)
        {
            SetImplementationType(serviceType, implementationType, ChainItemType.ConstructorCall);
        }

        public void SetFactoryImplementationType(Type serviceType)
        {
            SetImplementationType(serviceType, null, ChainItemType.FactoryCall);
        }

        internal string CreateCircularDependencyExceptionMessage(Type type)
        {
            var messageBuilder = new StringBuilder();
            messageBuilder.Append(Resources.FormatCircularDependencyException(type));

            void AppendResolveLine(StringBuilder builder, Type resolveType)
            {
                builder.AppendLine();
                builder.AppendFormat("Resolving '{0}'", resolveType);
            }

            foreach (var pair in callSiteChain.OrderBy(p => p.Value.Order))
            {
                var serviceType = pair.Key;
                var chainItemInfo = pair.Value;

                AppendResolveLine(messageBuilder, serviceType);
                switch (chainItemInfo.Type)
                {
                    case ChainItemType.InstanceUse:
                        messageBuilder.AppendFormat(" by using instance of type '{0}'.", chainItemInfo.ImplementationType);
                        break;
                    case ChainItemType.ConstructorCall:
                        messageBuilder.AppendFormat(" by activating '{0}'.", chainItemInfo.ImplementationType);
                        break;
                    case ChainItemType.FactoryCall:
                        messageBuilder.AppendFormat(" by running factory.");
                        break;
                    case ChainItemType.EnumerableCreate:
                        messageBuilder.AppendFormat(" by creating collection '{0}'.", chainItemInfo.ImplementationType);
                        break;
                }
            }

            AppendResolveLine(messageBuilder, type);
            messageBuilder.Append(" cause circular reference.");

            return messageBuilder.ToString();
        }
    }
}
