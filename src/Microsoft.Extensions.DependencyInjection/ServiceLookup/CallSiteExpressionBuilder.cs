using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal class CallSiteExpressionBuilder
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
            var serviceExpression = BuildExpression(callSite, _providerParameter);

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

        private Expression BuildExpression(IServiceCallSite callSite, ParameterExpression provider)
        {
            var factoryService = callSite as FactoryService;
            if (factoryService != null)
            {
                return BuildFactoryService(factoryService, provider);
            }
            var closedIEnumerableCallSite = callSite as ClosedIEnumerableCallSite;
            if (closedIEnumerableCallSite != null)
            {
                return BuildClosedIEnumerable(closedIEnumerableCallSite, provider);
            }
            var constructorCallSite = callSite as ConstructorCallSite;
            if (constructorCallSite != null)
            {
                return BuildConstructor(constructorCallSite, provider);
            }
            var transientCallSite = callSite as TransientCallSite;
            if (transientCallSite != null)
            {
                return BuildTransient(transientCallSite, provider);
            }
            var singletonCallSite = callSite as SingletonCallSite;
            if (singletonCallSite != null)
            {
                return BuildScoped(singletonCallSite, GetRootProvider());
            }
            var scopedCallSite = callSite as ScopedCallSite;
            if (scopedCallSite != null)
            {
                return BuildScoped(scopedCallSite, provider);
            }
            var constantCallSite = callSite as ConstantCallSite;
            if (constantCallSite != null)
            {
                return Expression.Constant(constantCallSite.DefaultValue);
            }
            var createInstanceCallSite = callSite as CreateInstanceCallSite;
            if (createInstanceCallSite != null)
            {
                return Expression.New(createInstanceCallSite.Descriptor.ImplementationType);
            }
            var instanceCallSite = callSite as InstanceService;
            if (instanceCallSite != null)
            {
                return Expression.Constant(
                    instanceCallSite.Descriptor.ImplementationInstance,
                    instanceCallSite.Descriptor.ServiceType);
            }
            var serviceProviderService = callSite as ServiceProviderService;
            if (serviceProviderService != null)
            {
                return provider;
            }
            var emptyIEnumerableCallSite = callSite as EmptyIEnumerableCallSite;
            if (emptyIEnumerableCallSite != null)
            {
                return Expression.Constant(
                    emptyIEnumerableCallSite.ServiceInstance,
                    emptyIEnumerableCallSite.ServiceType);
            }
            var serviceScopeService = callSite as ServiceScopeService;
            if (serviceScopeService != null)
            {
                return Expression.New(typeof(ServiceScopeFactory).GetTypeInfo()
                        .DeclaredConstructors
                        .Single(),
                    provider);
            }
            throw new NotSupportedException("Call site type is not supported");
        }

        public Expression BuildFactoryService(FactoryService factoryService, ParameterExpression provider)
        {
            return Expression.Invoke(Expression.Constant(factoryService.Descriptor.ImplementationFactory), provider);
        }

        public Expression BuildClosedIEnumerable(ClosedIEnumerableCallSite callSite, ParameterExpression provider)
        {
            return Expression.NewArrayInit(
                callSite.ItemType,
                callSite.ServiceCallSites.Select(cs =>
                    Expression.Convert(
                        BuildExpression(cs, provider),
                        callSite.ItemType)));
        }

        public Expression BuildTransient(TransientCallSite callSite, ParameterExpression provider)
        {
            return Expression.Invoke(GetCaptureDisposable(provider),
                BuildExpression(callSite.Service, provider));
        }

        public Expression BuildConstructor(ConstructorCallSite callSite, ParameterExpression provider)
        {
            var parameters = callSite.ConstructorInfo.GetParameters();
            return Expression.New(
                callSite.ConstructorInfo,
                callSite.ParameterCallSites.Select((c, index) =>
                        Expression.Convert(BuildExpression(c, provider), parameters[index].ParameterType)));
        }

        public virtual Expression BuildScoped(ScopedCallSite callSite, ParameterExpression provider)
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
                resolvedExpression, BuildExpression(callSite.ServiceCallSite, provider));

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
                var resolvedServicesExpression = Expression.Field(
                    provider,
                    "_resolvedServices");

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
                var rootExpression = Expression.Field(_providerParameter, "_root");
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