using System;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal abstract class CallSiteVisitor<TArgument, TResult>
    {
        protected virtual TResult VisitCallSite(IServiceCallSite callSite, TArgument argument)
        {
            switch (callSite.Cache.Location)
            {
                case CallSiteResultCacheLocation.Root:
                    return VisitSingleton(callSite, argument);
                case CallSiteResultCacheLocation.Scope:
                    return VisitScoped(callSite, argument);
                case CallSiteResultCacheLocation.None:
                    return VisitTransient(callSite, argument);
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected virtual TResult VisitCallSiteMain(IServiceCallSite callSite, TArgument argument)
        {
            switch (callSite.Kind)
            {
                case CallSiteKind.Factory:
                    return VisitFactory((FactoryCallSite)callSite, argument);
                case  CallSiteKind.IEnumerable:
                    return VisitIEnumerable((IEnumerableCallSite)callSite, argument);
                case CallSiteKind.Constructor:
                    return VisitConstructor((ConstructorCallSite)callSite, argument);
                case CallSiteKind.Constant:
                    return VisitConstant((ConstantCallSite)callSite, argument);
                case CallSiteKind.ServiceProvider:
                    return VisitServiceProvider((ServiceProviderCallSite)callSite, argument);
                case CallSiteKind.ServiceScopeFactory:
                    return VisitServiceScopeFactory((ServiceScopeFactoryCallSite)callSite, argument);
                default:
                    throw new NotSupportedException($"Call site type {callSite.GetType()} is not supported");
            }
        }

        protected virtual TResult VisitTransient(IServiceCallSite callSite, TArgument argument)
        {
            return VisitCallSiteMain(callSite, argument);
        }

        protected abstract TResult VisitConstructor(ConstructorCallSite constructorCallSite, TArgument argument);

        protected virtual TResult VisitSingleton(IServiceCallSite callSite, TArgument argument)
        {
            return VisitCallSiteMain(callSite, argument);
        }

        protected virtual TResult VisitScoped(IServiceCallSite callSite, TArgument argument)
        {
            return VisitCallSiteMain(callSite, argument);
        }

        protected abstract TResult VisitConstant(ConstantCallSite constantCallSite, TArgument argument);

        protected abstract TResult VisitServiceProvider(ServiceProviderCallSite serviceProviderCallSite, TArgument argument);

        protected abstract TResult VisitServiceScopeFactory(ServiceScopeFactoryCallSite serviceScopeFactoryCallSite, TArgument argument);

        protected abstract TResult VisitIEnumerable(IEnumerableCallSite enumerableCallSite, TArgument argument);

        protected abstract TResult VisitFactory(FactoryCallSite factoryCallSite, TArgument argument);
    }
}