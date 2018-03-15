// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal sealed class ILEmitCallSiteAnalyzer : CallSiteVisitor<object, ILEmitCallSiteAnalysisResult>
    {
        internal static ILEmitCallSiteAnalyzer Instance { get; } = new ILEmitCallSiteAnalyzer();

        protected override ILEmitCallSiteAnalysisResult VisitTransient(TransientCallSite transientCallSite, object argument) => VisitCallSite(transientCallSite.ServiceCallSite, argument);

        protected override ILEmitCallSiteAnalysisResult VisitConstructor(ConstructorCallSite constructorCallSite, object argument)
        {
            var result = new ILEmitCallSiteAnalysisResult(6);
            foreach (var callSite in constructorCallSite.ParameterCallSites)
            {
                result = result.Add(VisitCallSite(callSite, argument));
            }
            return result;
        }

        protected override ILEmitCallSiteAnalysisResult VisitSingleton(SingletonCallSite singletonCallSite, object argument) => VisitCallSite(singletonCallSite.ServiceCallSite, argument);

        protected override ILEmitCallSiteAnalysisResult VisitScoped(ScopedCallSite scopedCallSite, object argument) => new ILEmitCallSiteAnalysisResult(64, true).Add(VisitCallSite(scopedCallSite.ServiceCallSite, argument));

        protected override ILEmitCallSiteAnalysisResult VisitConstant(ConstantCallSite constantCallSite, object argument) => new ILEmitCallSiteAnalysisResult(4);

        protected override ILEmitCallSiteAnalysisResult VisitCreateInstance(CreateInstanceCallSite createInstanceCallSite, object argument) => new ILEmitCallSiteAnalysisResult(4);

        protected override ILEmitCallSiteAnalysisResult VisitServiceProvider(ServiceProviderCallSite serviceProviderCallSite, object argument) => new ILEmitCallSiteAnalysisResult(1);

        protected override ILEmitCallSiteAnalysisResult VisitServiceScopeFactory(ServiceScopeFactoryCallSite serviceScopeFactoryCallSite, object argument) => new ILEmitCallSiteAnalysisResult(6);

        protected override ILEmitCallSiteAnalysisResult VisitIEnumerable(IEnumerableCallSite enumerableCallSite, object argument)
        {
            var result = new ILEmitCallSiteAnalysisResult(6);
            foreach (var callSite in enumerableCallSite.ServiceCallSites)
            {
                result = result.Add(VisitCallSite(callSite, argument));
            }
            return result;
        }

        protected override ILEmitCallSiteAnalysisResult VisitFactory(FactoryCallSite factoryCallSite, object argument) => new ILEmitCallSiteAnalysisResult(16);

        public ILEmitCallSiteAnalysisResult CollectGenerationInfo(IServiceCallSite callSite) => VisitCallSite(callSite, null);
    }
}