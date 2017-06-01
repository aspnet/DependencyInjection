// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection.ServiceLookup;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// The default IServiceProvider.
    /// </summary>
    public sealed class ServiceProvider : IServiceProvider, IDisposable
    {
        private readonly CallSiteValidator _callSiteValidator;
        internal readonly ServiceTable _table;
        private bool _disposeCalled;
        private List<IDisposable> _disposables;

        internal ServiceProvider Root { get; }
        internal Dictionary<object, object> ResolvedServices { get; } = new Dictionary<object, object>();

        internal ServiceTable Table
        {
            get { return _table; }
        }

        private static readonly Func<Type, ServiceProvider, Func<ServiceProvider, object>> _createServiceAccessor = CreateServiceAccessor;

        // For testing only
        internal Action<object> _captureDisposableCallback;

        // CallSiteRuntimeResolver is stateless so can be shared between all instances
        private static readonly CallSiteRuntimeResolver _callSiteRuntimeResolver = new CallSiteRuntimeResolver();

        internal ServiceProvider(IEnumerable<ServiceDescriptor> serviceDescriptors, ServiceProviderOptions options)
        {
            Root = this;

            if (options.ValidateScopes)
            {
                _callSiteValidator = new CallSiteValidator();
            }

            _table = new ServiceTable(serviceDescriptors);

            Table.Add(typeof(IServiceProvider), new ServiceProviderService());
            Table.Add(typeof(IServiceScopeFactory), new ServiceScopeService());
        }

        // This constructor is called exclusively to create a child scope from the parent
        internal ServiceProvider(ServiceProvider parent)
        {
            Root = parent.Root;
            _table = parent.Table;
            _callSiteValidator = parent._callSiteValidator;
        }

        /// <summary>
        /// Gets the service object of the specified type.
        /// </summary>
        /// <param name="serviceType"></param>
        /// <returns></returns>
        public object GetService(Type serviceType)
        {
            var realizedService = Table.RealizedServices.GetOrAdd(serviceType, _createServiceAccessor, this);

            _callSiteValidator?.ValidateResolution(serviceType, this);

            return realizedService.Invoke(this);
        }

        private static Func<ServiceProvider, object> CreateServiceAccessor(Type serviceType, ServiceProvider serviceProvider)
        {
            var callSite = serviceProvider.Table.GetCallSite(serviceType, new HashSet<Type>());
            if (callSite != null)
            {
                serviceProvider._callSiteValidator?.ValidateCallSite(serviceType, callSite);
                return RealizeService(serviceProvider.Table, serviceType, callSite);
            }

            return _ => null;
        }

        internal static Func<ServiceProvider, object> RealizeService(ServiceTable table, Type serviceType, IServiceCallSite callSite)
        {
            var callCount = 0;
            return provider =>
            {
                if (Interlocked.Increment(ref callCount) == 2)
                {
                    Task.Run(() =>
                    {
                        var realizedService = new CallSiteExpressionBuilder(_callSiteRuntimeResolver)
                            .Build(callSite);
                        table.RealizedServices[serviceType] = realizedService;
                    });
                }

                return _callSiteRuntimeResolver.Resolve(callSite, provider);
            };
        }

        public void Dispose()
        {
            lock (ResolvedServices)
            {
                if (_disposeCalled)
                {
                    return;
                }

                _disposeCalled = true;
                if (_disposables != null)
                {
                    for (int i = _disposables.Count - 1; i >= 0; i--)
                    {
                        var disposable = _disposables[i];
                        disposable.Dispose();
                    }

                    _disposables.Clear();
                }

                ResolvedServices.Clear();
            }
        }

        internal object CaptureDisposable(object service)
        {
            _captureDisposableCallback?.Invoke(service);

            if (!object.ReferenceEquals(this, service))
            {
                var disposable = service as IDisposable;
                if (disposable != null)
                {
                    lock (ResolvedServices)
                    {
                        if (_disposables == null)
                        {
                            _disposables = new List<IDisposable>();
                        }

                        _disposables.Add(disposable);
                    }
                }
            }
            return service;
        }

        private object GetEmptyIEnumerableOrNull(Type serviceType)
        {
            var typeInfo = serviceType.GetTypeInfo();

            if (typeInfo.IsGenericType &&
                serviceType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
            {
                var itemType = typeInfo.GenericTypeArguments[0];
                return Array.CreateInstance(itemType, 0);
            }

            return null;
        }
    }
}
