// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal class CallSiteValidator: CallSiteVisitor<CallSiteValidatorState, object>
    {
        public void Validate(IServiceCallSite callSite)
        {
            VisitCallSite(callSite, default(CallSiteValidatorState));
        }

        protected override object VisitTransient(TransientCallSite transientCallSite, CallSiteValidatorState state)
        {
            VisitCallSite(transientCallSite.Service, state);
            return null;
        }

        protected override object VisitConstructor(ConstructorCallSite constructorCallSite, CallSiteValidatorState state)
        {
            foreach (var parameterCallSite in constructorCallSite.ParameterCallSites)
            {
                VisitCallSite(parameterCallSite, state);
            }
            return null;
        }

        protected override object VisitSingleton(SingletonCallSite singletonCallSite, CallSiteValidatorState state)
        {
            state.Singleton = singletonCallSite;
            VisitCallSite(singletonCallSite.ServiceCallSite, state);
            return null;
        }

        protected override object VisitScoped(ScopedCallSite scopedCallSite, CallSiteValidatorState state)
        {
            // We are fine with having ServiceScopeService requested by singletons
            if (scopedCallSite.ServiceCallSite is ServiceScopeService)
            {
                return null;
            }
            if (state.Singleton != null)
            {
                throw new InvalidOperationException(Resources.FormatScopedInSingletonException(scopedCallSite.Key.ServiceType, state.Singleton.Key.ServiceType));
            }
            VisitCallSite(scopedCallSite.ServiceCallSite, state);
            return null;
        }

        protected override object VisitConstant(ConstantCallSite constantCallSite, CallSiteValidatorState state) => null;

        protected override object VisitCreateInstance(CreateInstanceCallSite createInstanceCallSite, CallSiteValidatorState state) => null;

        protected override object VisitInstanceService(InstanceService instanceCallSite, CallSiteValidatorState state) => null;

        protected override object VisitServiceProviderService(ServiceProviderService serviceProviderService, CallSiteValidatorState state) => null;

        protected override object VisitEmptyIEnumerable(EmptyIEnumerableCallSite emptyIEnumerableCallSite, CallSiteValidatorState state) => null;

        protected override object VisitServiceScopeService(ServiceScopeService serviceScopeService, CallSiteValidatorState state) => null;

        protected override object VisitClosedIEnumerable(ClosedIEnumerableCallSite closedIEnumerableCallSite, CallSiteValidatorState state) => null;

        protected override object VisitFactoryService(FactoryService factoryService, CallSiteValidatorState state) => null;
    }
}