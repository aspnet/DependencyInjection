using System;
using System.Runtime.ExceptionServices;
using System.Threading;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal sealed class CallSiteRuntimeResolver : CallSiteVisitor<RuntimeResolverContext, object>
    {
        public CallSiteRuntimeResolver() : base()
        {
        }

        public object Resolve(ServiceCallSite callSite, ServiceProviderEngineScope scope)
        {
            return VisitCallSite(callSite, new RuntimeResolverContext
            {
                EngineScope = scope
            });
        }

        protected override object VisitDisposeCache(ServiceCallSite transientCallSite, RuntimeResolverContext scope)
        {
            return scope.EngineScope.CaptureDisposable(VisitCallSiteMain(transientCallSite, scope));
        }

        protected override object VisitConstructor(ConstructorCallSite constructorCallSite, RuntimeResolverContext scope)
        {
            object[] parameterValues;
            if (constructorCallSite.ParameterCallSites.Length == 0)
            {
                parameterValues = Array.Empty<object>();
            }
            else
            {
                parameterValues = new object[constructorCallSite.ParameterCallSites.Length];
                for (var index = 0; index < parameterValues.Length; index++)
                {
                    parameterValues[index] = VisitCallSite(constructorCallSite.ParameterCallSites[index], scope);
                }
            }

            try
            {
                return constructorCallSite.ConstructorInfo.Invoke(parameterValues);
            }
            catch (Exception ex) when (ex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                // The above line will always throw, but the compiler requires we throw explicitly.
                throw;
            }
        }

        protected override object VisitRootCache(ServiceCallSite singletonCallSite, RuntimeResolverContext scope)
        {
            return VisitCache(singletonCallSite, scope, scope.EngineScope.Engine.Root, RuntimeResolverLock.Root);
        }

        protected override object VisitScopeCache(ServiceCallSite singletonCallSite, RuntimeResolverContext scope)
        {
            // Check if we are in the situation where scoped service was promoted to singleton
            // and we need to lock the root
            var requiredScope = scope.EngineScope == scope.EngineScope.Engine.Root ?
                RuntimeResolverLock.Root :
                RuntimeResolverLock.Scope;

            return VisitCache(singletonCallSite, scope, scope.EngineScope, requiredScope);
        }

        private object VisitCache(ServiceCallSite scopedCallSite, RuntimeResolverContext scope, ServiceProviderEngineScope serviceProviderEngine, RuntimeResolverLock lockType)
        {
            bool lockTaken = false;
            var resolvedServices = serviceProviderEngine.ResolvedServices;

            if ((scope.AcquiredLocks & lockType) == 0)
            {
                Monitor.Enter(resolvedServices, ref lockTaken);
            }

            try
            {
                if (!resolvedServices.TryGetValue(scopedCallSite.Cache.Key, out var resolved))
                {
                    resolved = VisitCallSiteMain(scopedCallSite, new RuntimeResolverContext
                    {
                        EngineScope = serviceProviderEngine,
                        AcquiredLocks = scope.AcquiredLocks | lockType
                    });

                    serviceProviderEngine.CaptureDisposable(resolved);
                    resolvedServices.Add(scopedCallSite.Cache.Key, resolved);
                }

                return resolved;
            }
            finally
            {
                if (lockTaken)
                {
                    Monitor.Exit(resolvedServices);
                }
            }
        }

        protected override object VisitConstant(ConstantCallSite constantCallSite, RuntimeResolverContext scope)
        {
            return constantCallSite.DefaultValue;
        }

        protected override object VisitServiceProvider(ServiceProviderCallSite serviceProviderCallSite, RuntimeResolverContext scope)
        {
            return scope.EngineScope;
        }

        protected override object VisitServiceScopeFactory(ServiceScopeFactoryCallSite serviceScopeFactoryCallSite, RuntimeResolverContext scope)
        {
            return scope.EngineScope.Engine;
        }

        protected override object VisitIEnumerable(IEnumerableCallSite enumerableCallSite, RuntimeResolverContext scope)
        {
            var array = Array.CreateInstance(
                enumerableCallSite.ItemType,
                enumerableCallSite.ServiceCallSites.Length);

            for (var index = 0; index < enumerableCallSite.ServiceCallSites.Length; index++)
            {
                var value = VisitCallSite(enumerableCallSite.ServiceCallSites[index], scope);
                array.SetValue(value, index);
            }
            return array;
        }

        protected override object VisitFactory(FactoryCallSite factoryCallSite, RuntimeResolverContext scope)
        {
            return factoryCallSite.Factory(scope.EngineScope);
        }
    }

    internal struct RuntimeResolverContext
    {
        public ServiceProviderEngineScope EngineScope { get; set; }

        public RuntimeResolverLock AcquiredLocks { get; set; }
    }

    [Flags]
    internal enum RuntimeResolverLock
    {
        Scope = 1,
        Root = 2
    }
}