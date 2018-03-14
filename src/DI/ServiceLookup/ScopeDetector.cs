// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal struct GenerationInfo
    {
        public GenerationInfo(int size) : this()
        {
            Size = size;
        }

        public GenerationInfo(int size, bool hasScope)
        {
            Size = size;
            HasScope = hasScope;
        }

        public int Size;

        public bool HasScope;

        public GenerationInfo Add(GenerationInfo other)
        {
            return new GenerationInfo()
            {
                Size = Size + other.Size,
                HasScope = HasScope | other.HasScope
            };
        }

        public GenerationInfo Add(byte size)
        {
            return new GenerationInfo()
            {
                Size = Size + size,
                HasScope = HasScope
            };
        }
    }

    internal sealed class ScopeDetector : CallSiteVisitor<object, GenerationInfo>
    {
        internal static ScopeDetector Instance { get; } = new ScopeDetector();

        protected override GenerationInfo VisitTransient(TransientCallSite transientCallSite, object argument) => VisitCallSite(transientCallSite.ServiceCallSite, argument);

        protected override GenerationInfo VisitConstructor(ConstructorCallSite constructorCallSite, object argument)
        {
            var result = new GenerationInfo();
            foreach (var callSite in constructorCallSite.ParameterCallSites)
            {
                result = result.Add(VisitCallSite(callSite, argument));
            }
            return result;
        }

        protected override GenerationInfo VisitSingleton(SingletonCallSite singletonCallSite, object argument) => VisitCallSite(singletonCallSite.ServiceCallSite, argument);

        protected override GenerationInfo VisitScoped(ScopedCallSite scopedCallSite, object argument) => new GenerationInfo(64, true).Add(VisitCallSite(scopedCallSite.ServiceCallSite, argument));

        protected override GenerationInfo VisitConstant(ConstantCallSite constantCallSite, object argument) => new GenerationInfo(4);

        protected override GenerationInfo VisitCreateInstance(CreateInstanceCallSite createInstanceCallSite, object argument) => new GenerationInfo(4);

        protected override GenerationInfo VisitServiceProvider(ServiceProviderCallSite serviceProviderCallSite, object argument) => new GenerationInfo(4);

        protected override GenerationInfo VisitServiceScopeFactory(ServiceScopeFactoryCallSite serviceScopeFactoryCallSite, object argument) => new GenerationInfo(4);

        protected override GenerationInfo VisitIEnumerable(IEnumerableCallSite enumerableCallSite, object argument)
        {
            var result = new GenerationInfo();
            foreach (var callSite in enumerableCallSite.ServiceCallSites)
            {
                result = result.Add(VisitCallSite(callSite, argument));
            }
            return result;
        }

        protected override GenerationInfo VisitFactory(FactoryCallSite factoryCallSite, object argument) => new GenerationInfo(4);

        public GenerationInfo CollectGenerationInfo(IServiceCallSite callSite) => VisitCallSite(callSite, null);
    }
}