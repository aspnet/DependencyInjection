using System;
using System.Runtime.ExceptionServices;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal class CallSiteRuntimeResolver : CallSiteVisitor<ServiceProvider, object>
    {
        public object Resolve(IServiceCallSite callSite, ServiceProvider provider)
        {
            return VisitCallSite(callSite, provider);
        }

        protected override object VisitTransient(TransientCallSite transientCallSite, ServiceProvider provider)
        {
            return provider.CaptureDisposable(
                VisitCallSite(transientCallSite.ServiceCallSite, provider));
        }

        protected override object VisitConstructor(ConstructorCallSite constructorCallSite, ServiceProvider provider)
        {
            object[] parameterValues = new object[constructorCallSite.ParameterCallSites.Length];
            for (var index = 0; index < parameterValues.Length; index++)
            {
                parameterValues[index] = VisitCallSite(constructorCallSite.ParameterCallSites[index], provider);
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

        protected override object VisitSingleton(SingletonCallSite singletonCallSite, ServiceProvider provider)
        {
            return VisitScoped(singletonCallSite, provider.Root);
        }

        protected override object VisitScoped(ScopedCallSite scopedCallSite, ServiceProvider provider)
        {
            lock (provider.ResolvedServices)
            {
                if (!provider.ResolvedServices.TryGetValue(scopedCallSite, out var resolved))
                {
                    resolved = VisitCallSite(scopedCallSite.ServiceCallSite, provider);
                    provider.CaptureDisposable(resolved);
                    provider.ResolvedServices.Add(scopedCallSite, resolved);
                }
                return resolved;
            }
        }

        protected override object VisitConstant(ConstantCallSite constantCallSite, ServiceProvider provider)
        {
            return constantCallSite.DefaultValue;
        }

        protected override object VisitCreateInstance(CreateInstanceCallSite createInstanceCallSite, ServiceProvider provider)
        {
            try
            {
                return Activator.CreateInstance(createInstanceCallSite.ImplementationType);
            }
            catch (Exception ex) when (ex.InnerException != null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                // The above line will always throw, but the compiler requires we throw explicitly.
                throw;
            }
        }

        protected override object VisitServiceProviderService(ServiceProviderService serviceProviderService, ServiceProvider provider)
        {
            return provider;
        }

        protected override object VisitServiceScopeService(ServiceScopeService serviceScopeService, ServiceProvider provider)
        {
            return new ServiceScopeFactory(provider);
        }

        protected override object VisitClosedIEnumerable(ClosedIEnumerableCallSite closedIEnumerableCallSite, ServiceProvider provider)
        {
            var array = Array.CreateInstance(
                closedIEnumerableCallSite.ItemType,
                closedIEnumerableCallSite.ServiceCallSites.Length);

            for (var index = 0; index < closedIEnumerableCallSite.ServiceCallSites.Length; index++)
            {
                var value = VisitCallSite(closedIEnumerableCallSite.ServiceCallSites[index], provider);
                array.SetValue(value, index);
            }
            return array;
        }

        protected override object VisitFactoryService(FactoryService factoryService, ServiceProvider provider)
        {
            return factoryService.Factory(provider);
        }
    }
}