// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal class CallSiteChain
    {
        private readonly Dictionary<Type,ChainItemInfo> _callSiteChain;

        public CallSiteChain()
        {
            _callSiteChain = new Dictionary<Type, ChainItemInfo>();
        }

        public void CheckCircularDependency(Type serviceType)
        {
            if (_callSiteChain.ContainsKey(serviceType))
            {
                throw new InvalidOperationException(CreateCircularDependencyExceptionMessage(serviceType));
            }
        }

        public void Remove(Type serviceType)
        {
            _callSiteChain.Remove(serviceType);
        }

        public void AddEnumerableCreation(Type serviceType)
        {
            Add(serviceType, null, ChainItemType.EnumerableCreation);
        }

        public void AddConstructorCall(Type serviceType, Type implementationType)
        {
            Add(serviceType, implementationType, ChainItemType.ConstructorCall);
        }

        private void Add(Type serviceType, Type implementationType, ChainItemType chainItemType)
        {
            _callSiteChain[serviceType] = new ChainItemInfo(_callSiteChain.Count, chainItemType, implementationType);
        }

        private string CreateCircularDependencyExceptionMessage(Type type)
        {
            var messageBuilder = new StringBuilder();
            messageBuilder.AppendFormat(Resources.CircularDependencyException, type);
            messageBuilder.AppendLine();

            AppendResolutionPath(messageBuilder, type);

            return messageBuilder.ToString();
        }

        private void AppendResolutionPath(StringBuilder builder, Type currentlyResolving = null)
        {
            builder.AppendLine(Resources.ResolutionPathHeader);

            void AppendFormatLine(StringBuilder stringBuilder, string format, KeyValuePair<Type, ChainItemInfo> pair)
            {
                stringBuilder.AppendFormat(format, pair.Key, pair.Value.ImplementationType);
                stringBuilder.AppendLine();
            }

            foreach (var pair in _callSiteChain.OrderBy(p => p.Value.Order))
            {
                var chainItemInfo = pair.Value;

                switch (chainItemInfo.Type)
                {
                    case ChainItemType.ConstructorCall:
                        AppendFormatLine(builder, Resources.ResolutionPathItemConstructorCall, pair);
                        break;
                    case ChainItemType.EnumerableCreation:
                        AppendFormatLine(builder, Resources.ResolutionPathItemEnumerableCreate, pair);
                        break;
                }
            }

            if (currentlyResolving != null)
            {
                builder.AppendFormat(Resources.ResolutionPathItemCurrent, currentlyResolving);
            }
        }

        private enum ChainItemType
        {
            ConstructorCall,
            EnumerableCreation
        }

        private struct ChainItemInfo
        {
            public int Order { get; }
            public Type ImplementationType { get; }
            public ChainItemType Type { get; }

            public ChainItemInfo(int order, ChainItemType type, Type implementationType)
            {
                Order = order;
                ImplementationType = implementationType;
                Type = type;
            }
        }
    }
}
