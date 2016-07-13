// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal class CallSiteExpressionBuilder : CallSiteVisitor<ParameterExpression, Expression>
    {
        private static readonly MethodInfo CaptureDisposableMethodInfo = GetMethodInfo<Func<ServiceProvider, object, object>>((a, b) => a.CaptureDisposable(b));
        private static readonly MethodInfo TryGetValueMethodInfo = GetMethodInfo<Func<IDictionary<object, object>, object, object, bool>>((a, b, c) => a.TryGetValue(b, out c));
        private static readonly MethodInfo AddMethodInfo = GetMethodInfo<Action<IDictionary<object, object>, object, object>>((a, b, c) => a.Add(b, c));
        private static readonly MethodInfo MonitorEnterMethodInfo = GetMethodInfo<Action<object, bool>>((lockObj, lockTaken) => Monitor.Enter(lockObj, ref lockTaken));
        private static readonly MethodInfo MonitorExitMethodInfo = GetMethodInfo<Action<object>>(lockObj => Monitor.Exit(lockObj));
        private static readonly MethodInfo CallSiteRuntimeResolverResolve =
            GetMethodInfo<Func<CallSiteRuntimeResolver, IServiceCallSite, ServiceProvider, object>>((r, c, p) => r.Resolve(c, p));

        private readonly ParameterExpression _providerParameter = Expression.Parameter(typeof(ServiceProvider));
        private readonly CallSiteRuntimeResolver _runtimeResolver;

        private ParameterExpression _resolvedServices;
        private LambdaExpression _captureDisposable;
        private BinaryExpression _resolvedServicesVariableAssignment;

        public CallSiteExpressionBuilder(CallSiteRuntimeResolver runtimeResolver)
        {
            if (runtimeResolver == null)
            {
                throw new ArgumentNullException(nameof(runtimeResolver));
            }
            _runtimeResolver = runtimeResolver;
        }

        public Expression<Func<ServiceProvider, object>> Build(IServiceCallSite callSite)
        {
            var serviceExpression = VisitCallSite(callSite, _providerParameter);

            List<Expression> body = new List<Expression>();
            if (_resolvedServicesVariableAssignment != null)
            {
                body.Add(_resolvedServicesVariableAssignment);
            }
            if (_resolvedServices != null)
            {
                serviceExpression = Lock(serviceExpression, _resolvedServices);
            }
            body.Add(serviceExpression);

            var variables = _resolvedServices != null
                ? new[] { _resolvedServices }
                : Enumerable.Empty<ParameterExpression>();

            return Expression.Lambda<Func<ServiceProvider, object>>(
                Expression.Block(variables, body),
                _providerParameter);
        }

        protected override Expression VisitSingleton(SingletonCallSite singletonCallSite, ParameterExpression provider)
        {
            // Call to CallSiteRuntimeResolver.Resolve is being returned here
            // because in the current use case singleton service was already resolved and cached
            // to dictionary so there is no need to generate full tree at this point.

            return Expression.Call(
                Expression.Constant(_runtimeResolver),
                CallSiteRuntimeResolverResolve,
                Expression.Constant(singletonCallSite, typeof(IServiceCallSite)),
                provider);
        }

        protected override Expression VisitConstant(ConstantCallSite constantCallSite, ParameterExpression provider)
        {
            return Expression.Constant(constantCallSite.DefaultValue);
        }

        protected override Expression VisitCreateInstance(CreateInstanceCallSite createInstanceCallSite, ParameterExpression provider)
        {
            return Expression.New(createInstanceCallSite.Descriptor.ImplementationType);
        }

        protected override Expression VisitInstanceService(InstanceService instanceCallSite, ParameterExpression provider)
        {
            return Expression.Constant(
                instanceCallSite.Descriptor.ImplementationInstance,
                instanceCallSite.Descriptor.ServiceType);
        }

        protected override Expression VisitServiceProviderService(ServiceProviderService serviceProviderService, ParameterExpression provider)
        {
            return provider;
        }

        protected override Expression VisitEmptyIEnumerable(EmptyIEnumerableCallSite emptyIEnumerableCallSite, ParameterExpression provider)
        {
            return Expression.Constant(
                emptyIEnumerableCallSite.ServiceInstance,
                emptyIEnumerableCallSite.ServiceType);
        }

        protected override Expression VisitServiceScopeService(ServiceScopeService serviceScopeService, ParameterExpression provider)
        {
            return Expression.New(typeof(ServiceScopeFactory).GetTypeInfo()
                    .DeclaredConstructors
                    .Single(),
                provider);
        }

        protected override Expression VisitFactoryService(FactoryService factoryService, ParameterExpression provider)
        {
            return Expression.Invoke(Expression.Constant(factoryService.Descriptor.ImplementationFactory), provider);
        }

        protected override Expression VisitClosedIEnumerable(ClosedIEnumerableCallSite callSite, ParameterExpression provider)
        {
            return Expression.NewArrayInit(
                callSite.ItemType,
                callSite.ServiceCallSites.Select(cs =>
                    Expression.Convert(
                        VisitCallSite(cs, provider),
                        callSite.ItemType)));
        }

        protected override Expression VisitTransient(TransientCallSite callSite, ParameterExpression provider)
        {
            return Expression.Invoke(GetCaptureDisposable(provider),
                VisitCallSite(callSite.Service, provider));
        }

        protected override Expression VisitConstructor(ConstructorCallSite callSite, ParameterExpression provider)
        {
            var parameters = callSite.ConstructorInfo.GetParameters();
            return Expression.New(
                callSite.ConstructorInfo,
                callSite.ParameterCallSites.Select((c, index) =>
                        Expression.Convert(VisitCallSite(c, provider), parameters[index].ParameterType)));
        }

        protected override Expression VisitScoped(ScopedCallSite callSite, ParameterExpression provider)
        {
            var keyExpression = Expression.Constant(
                callSite.Key,
                typeof(object));

            var resolvedExpression = Expression.Variable(typeof(object), "resolved");

            var resolvedServices = GetResolvedServices(provider);

            var tryGetValueExpression = Expression.Call(
                resolvedServices,
                TryGetValueMethodInfo,
                keyExpression,
                resolvedExpression);

            var assignExpression = Expression.Assign(
                resolvedExpression, VisitCallSite(callSite.ServiceCallSite, provider));

            var addValueExpression = Expression.Call(
                resolvedServices,
                AddMethodInfo,
                keyExpression,
                resolvedExpression);

            var blockExpression = Expression.Block(
                typeof(object),
                new[] {
                    resolvedExpression
                },
                Expression.IfThen(
                    Expression.Not(tryGetValueExpression),
                    Expression.Block(assignExpression, addValueExpression)),
                resolvedExpression);

            return blockExpression;
        }

        private static MethodInfo GetMethodInfo<T>(Expression<T> expr)
        {
            var mc = (MethodCallExpression)expr.Body;
            return mc.Method;
        }

        public Expression GetCaptureDisposable(ParameterExpression provider)
        {
            if (_captureDisposable == null)
            {
                var parameter = Expression.Parameter(typeof(object));
                _captureDisposable = Expression.Lambda(
                    Expression.Call(provider, CaptureDisposableMethodInfo, parameter), parameter);
            }

            return _captureDisposable;
        }

        public Expression GetResolvedServices(ParameterExpression provider)
        {
            if (provider != _providerParameter)
            {
                throw new NotSupportedException("Resolved services call is supported only for main provider");
            }
            if (_resolvedServices == null)
            {
                var resolvedServicesExpression = Expression.Property(
                    provider,
                    nameof(ServiceProvider.ResolvedServices));

                _resolvedServices = Expression.Variable(typeof(IDictionary<object, object>),
                    provider.Name + "resolvedServices");
                _resolvedServicesVariableAssignment = Expression.Assign(_resolvedServices,
                    resolvedServicesExpression);
            }

            return _resolvedServices;
        }

        private static Expression Lock(Expression body, Expression syncVariable)
        {
            // The C# compiler would copy the lock object to guard against mutation.
            // We don't, since we know the lock object is readonly.
            var lockWasTaken = Expression.Variable(typeof(bool), "lockWasTaken");

            var monitorEnter = Expression.Call(MonitorEnterMethodInfo, syncVariable, lockWasTaken);
            var monitorExit = Expression.Call(MonitorExitMethodInfo, syncVariable);

            var tryBody = Expression.Block(monitorEnter, body);
            var finallyBody = Expression.IfThen(lockWasTaken, monitorExit);

            return Expression.Block(
                typeof(object),
                new[] { lockWasTaken },
                Expression.TryFinally(tryBody, finallyBody));
        }
    }
}