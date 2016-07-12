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
    internal class CallSiteExpressionBuilder: CallSiteVisitor<ParameterExpression, Expression>
    {
        private static readonly MethodInfo CaptureDisposableMethodInfo = GetMethodInfo<Func<ServiceProvider, object, object>>((a, b) => a.CaptureDisposable(b));
        private static readonly MethodInfo TryGetValueMethodInfo = GetMethodInfo<Func<IDictionary<object, object>, object, object, bool>>((a, b, c) => a.TryGetValue(b, out c));
        private static readonly MethodInfo AddMethodInfo = GetMethodInfo<Action<IDictionary<object, object>, object, object>>((a, b, c) => a.Add(b, c));
        private static readonly MethodInfo MonitorEnterMethodInfo = GetMethodInfo<Action<object, bool>>((lockObj, lockTaken) => Monitor.Enter(lockObj, ref lockTaken));
        private static readonly MethodInfo MonitorExitMethodInfo = GetMethodInfo<Action<object>>(lockObj => Monitor.Exit(lockObj));

        private readonly ParameterExpression _providerParameter = Expression.Parameter(typeof(ServiceProvider));

        private readonly List<Expression> _initializations = new List<Expression>();
        private readonly List<ParameterExpression> _variables = new List<ParameterExpression>();
        private readonly List<Expression> _locks = new List<Expression>();

        private readonly Dictionary<Expression, ParameterExpression> _resolvedServices = new Dictionary<Expression, ParameterExpression>();
        private readonly Dictionary<Expression, LambdaExpression> _captureDisposable = new Dictionary<Expression, LambdaExpression>();

        private ParameterExpression _rootVariable;

        public Expression<Func<ServiceProvider, object>> Build(IServiceCallSite callSite)
        {
            var serviceExpression = VisitCallSite(callSite, _providerParameter);

            List<Expression> body = new List<Expression>();
            body.AddRange(_initializations);
            foreach (var expression in _locks)
            {
                serviceExpression = Lock(serviceExpression, expression);
            }
            body.Add(serviceExpression);

            return Expression.Lambda<Func<ServiceProvider, object>>(
                Expression.Block(_variables, body),
                _providerParameter);
        }

        protected override Expression VisitSingleton(SingletonCallSite singletonCallSite, ParameterExpression provider)
        {
            return VisitScoped(singletonCallSite, GetRootProvider());
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
            LambdaExpression captureDisposableLambda;
            if (!_captureDisposable.TryGetValue(provider, out captureDisposableLambda))
            {
                var parameter = Expression.Parameter(typeof(object));
                 captureDisposableLambda = Expression.Lambda(
                    Expression.Call(provider, CaptureDisposableMethodInfo, parameter), parameter);
                _captureDisposable.Add(provider, captureDisposableLambda);
            }

            return captureDisposableLambda;
        }

        public Expression GetResolvedServices(ParameterExpression provider)
        {
            ParameterExpression resolvedServicesVariable;
            if (!_resolvedServices.TryGetValue(provider, out resolvedServicesVariable))
            {
                var resolvedServicesExpression = Expression.Property(
                    provider,
                    nameof(ServiceProvider.ResolvedServices));

                resolvedServicesVariable = Expression.Variable(typeof(IDictionary<object, object>),
                    provider.Name + "resolvedServices");
                var resolvedServicesVariableAssignment = Expression.Assign(resolvedServicesVariable,
                    resolvedServicesExpression);

                _locks.Add(resolvedServicesVariable);
                _variables.Add(resolvedServicesVariable);
                _initializations.Add(resolvedServicesVariableAssignment);
                _resolvedServices.Add(provider, resolvedServicesVariable);
            }

            return resolvedServicesVariable;
        }

        public ParameterExpression GetRootProvider()
        {
            if (_rootVariable == null)
            {
                var rootExpression = Expression.Property(_providerParameter, nameof(ServiceProvider.Root));
                _rootVariable = Expression.Variable(typeof(ServiceProvider), "root");

                _variables.Add(_rootVariable);
                _initializations.Add(Expression.Assign(_rootVariable, rootExpression));
            }
            return _rootVariable;
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