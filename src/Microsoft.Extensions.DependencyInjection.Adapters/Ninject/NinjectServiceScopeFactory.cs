﻿using System;
using System.Collections.Generic;
using System.Linq;
using Ninject;
using Ninject.Activation;
using Ninject.Parameters;
using Ninject.Syntax;

namespace Microsoft.Extensions.DependencyInjection.Adapters.Ninject
{
    internal class NinjectServiceScopeFactory : IServiceScopeFactory
    {
        private readonly IResolutionRoot _resolver;
        private readonly IEnumerable<IParameter> _inheritedParameters;

        public NinjectServiceScopeFactory(IContext context)
        {
            _resolver = context.Kernel.Get<IResolutionRoot>();
            _inheritedParameters = context.Parameters.Where(p => p.ShouldInherit);
        }

        public IServiceScope CreateScope()
        {
            return new NinjectServiceScope(_resolver, _inheritedParameters);
        }

        private class NinjectServiceScope : IServiceScope
        {
            private readonly ScopeParameter _scope;
            private readonly IServiceProvider _serviceProvider;

            public NinjectServiceScope(
                IResolutionRoot resolver,
                IEnumerable<IParameter> inheritedParameters)
            {
                _scope = new ScopeParameter();
                inheritedParameters = inheritedParameters.AddOrReplaceScopeParameter(_scope);
                _serviceProvider = new NinjectServiceProvider(resolver, inheritedParameters.ToArray());
            }

            public IServiceProvider ServiceProvider
            {
                get { return _serviceProvider; }
            }

            public void Dispose()
            {
                _scope.Dispose();
            }
        }
    }
}