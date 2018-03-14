// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal sealed class ScopeDetector : CallSiteVisitor<object, bool>
    {
        internal static ScopeDetector Instance { get; } = new ScopeDetector();

        protected override bool VisitTransient(TransientCallSite transientCallSite, object argument) => VisitCallSite(transientCallSite.ServiceCallSite, argument);

        protected override bool VisitConstructor(ConstructorCallSite constructorCallSite, object argument)
        {
            var result = false;
            foreach (var callSite in constructorCallSite.ParameterCallSites)
            {
                result |= VisitCallSite(callSite, argument);
            }
            return result;
        }

        protected override bool VisitSingleton(SingletonCallSite singletonCallSite, object argument) => VisitCallSite(singletonCallSite.ServiceCallSite, argument);

        protected override bool VisitScoped(ScopedCallSite scopedCallSite, object argument) => true;

        protected override bool VisitConstant(ConstantCallSite constantCallSite, object argument) => false;

        protected override bool VisitCreateInstance(CreateInstanceCallSite createInstanceCallSite, object argument) => false;

        protected override bool VisitServiceProvider(ServiceProviderCallSite serviceProviderCallSite, object argument) => false;

        protected override bool VisitServiceScopeFactory(ServiceScopeFactoryCallSite serviceScopeFactoryCallSite, object argument) => false;

        protected override bool VisitIEnumerable(IEnumerableCallSite enumerableCallSite, object argument)
        {
            var result = false;
            foreach (var callSite in enumerableCallSite.ServiceCallSites)
            {
                result |= VisitCallSite(callSite, argument);
            }
            return result;
        }

        protected override bool VisitFactory(FactoryCallSite factoryCallSite, object argument) => false;

        public bool HasScopedServices(IServiceCallSite callSite) => VisitCallSite(callSite, null);
    }
}