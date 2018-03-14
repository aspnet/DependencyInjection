// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
    internal sealed class ILEmitResolverBuilder : CallSiteVisitor<ILEmitResolverBuilderContext, Expression>, IResolverBuilder
    {
        private static readonly MethodInfo ResolvedServicesGetter = typeof(ServiceProviderEngineScope).GetProperty(
            nameof(ServiceProviderEngineScope.ResolvedServices), BindingFlags.Instance | BindingFlags.NonPublic).GetMethod;

        private static readonly FieldInfo RuntimeResolverField = typeof(ILEmitResolverBuilderRuntimeContext).GetField(nameof(ILEmitResolverBuilderRuntimeContext.RuntimeResolver));
        private static readonly FieldInfo RootField = typeof(ILEmitResolverBuilderRuntimeContext).GetField(nameof(ILEmitResolverBuilderRuntimeContext.Root));
        private static readonly FieldInfo FactoriesField = typeof(ILEmitResolverBuilderRuntimeContext).GetField(nameof(ILEmitResolverBuilderRuntimeContext.Factories));
        private static readonly FieldInfo ConstantsField = typeof(ILEmitResolverBuilderRuntimeContext).GetField(nameof(ILEmitResolverBuilderRuntimeContext.Constants));

        private class ILEmitResolverBuilderRuntimeContext
        {
            public CallSiteRuntimeResolver RuntimeResolver;
            public IServiceScopeFactory ScopeFactory;
            public ServiceProviderEngineScope Root;
            public object[] Constants;
            public Func<IServiceProvider, object>[] Factories;
        }

        private readonly CallSiteRuntimeResolver _runtimeResolver;

        private readonly IServiceScopeFactory _serviceScopeFactory;

        private readonly ServiceProviderEngineScope _rootScope;

        public ILEmitResolverBuilder(CallSiteRuntimeResolver runtimeResolver, IServiceScopeFactory serviceScopeFactory, ServiceProviderEngineScope rootScope)
        {
            if (runtimeResolver == null)
            {
                throw new ArgumentNullException(nameof(runtimeResolver));
            }
            _runtimeResolver = runtimeResolver;
            _serviceScopeFactory = serviceScopeFactory;
            _rootScope = rootScope;
        }

        public Func<ServiceProviderEngineScope, object> Build(IServiceCallSite callSite)
        {
            if (callSite is SingletonCallSite singletonCallSite)
            {
                // If root call site is singleton we can return Func calling
                // _runtimeResolver.Resolve directly and avoid Expression generation
                if (TryResolveSingletonValue(singletonCallSite, out var value))
                {
                    return scope => value;
                }

                return scope => _runtimeResolver.Resolve(callSite, scope);
            }

            return BuildType(callSite);
        }

        protected override Expression VisitTransient(TransientCallSite transientCallSite, ILEmitResolverBuilderContext argument)
        {
            var shouldCapture = BeginCaptureDisposable(transientCallSite.ServiceCallSite.ImplementationType, argument);

            VisitCallSite(transientCallSite.ServiceCallSite, argument);

            if (shouldCapture)
            {
                EndCaptureDisposable(argument);
            }
            return null;
        }

        protected override Expression VisitConstructor(ConstructorCallSite constructorCallSite, ILEmitResolverBuilderContext argument)
        {
            foreach (var parameterCallSite in constructorCallSite.ParameterCallSites)
            {
                VisitCallSite(parameterCallSite, argument);
            }
            argument.Generator.Emit(OpCodes.Newobj, constructorCallSite.ConstructorInfo);
            return null;
        }

        protected override Expression VisitSingleton(SingletonCallSite singletonCallSite, ILEmitResolverBuilderContext argument)
        {
            if (TryResolveSingletonValue(singletonCallSite, out var value))
            {
                AddConstant(argument, value);
                return null;
            }

            argument.Generator.Emit(OpCodes.Ldarg_0);
            argument.Generator.Emit(OpCodes.Ldfld, RuntimeResolverField);

            AddConstant(argument, singletonCallSite);

            argument.Generator.Emit(OpCodes.Ldarg_0);
            argument.Generator.Emit(OpCodes.Ldfld, RootField);

            argument.Generator.Emit(OpCodes.Callvirt, ExpressionResolverBuilder.CallSiteRuntimeResolverResolve);
            return null;
        }

        protected override Expression VisitScoped(ScopedCallSite scopedCallSite, ILEmitResolverBuilderContext argument)
        {
            var resultLocal = argument.Generator.DeclareLocal(scopedCallSite.ServiceType);
            var cacheKeyLocal = argument.Generator.DeclareLocal(typeof(object));
            var endLabel = argument.Generator.DefineLabel();

            // Resolved services would be 0 local
            argument.Generator.Emit(OpCodes.Ldloc_0);

            AddConstant(argument, scopedCallSite.CacheKey);
            // Duplicate cache key
            argument.Generator.Emit(OpCodes.Dup);
            // and store to local
            StLoc(argument.Generator, cacheKeyLocal.LocalIndex);

            // Load address of local
            argument.Generator.Emit(OpCodes.Ldloca, resultLocal.LocalIndex);
            // .TryGetValue
            argument.Generator.Emit(OpCodes.Callvirt, ExpressionResolverBuilder.TryGetValueMethodInfo);

            // Jump to create new if nothing in cache
            argument.Generator.Emit(OpCodes.Brtrue, endLabel);

            var shouldCapture = BeginCaptureDisposable(scopedCallSite.ServiceCallSite.ImplementationType, argument);

            VisitCallSite(scopedCallSite.ServiceCallSite, argument);

            if (shouldCapture)
            {
                EndCaptureDisposable(argument);
            }

            // Store return value into var
            argument.Generator.Emit(OpCodes.Stloc, resultLocal.LocalIndex);

            argument.Generator.Emit(OpCodes.Ldloc_0);
            // Load cache key
            LdLoc(argument.Generator, cacheKeyLocal.LocalIndex);
            // Load value
            LdLoc(argument.Generator, resultLocal.LocalIndex);

            argument.Generator.Emit(OpCodes.Callvirt, ExpressionResolverBuilder.AddMethodInfo);

            // Load result and return it
            argument.Generator.MarkLabel(endLabel);
            LdLoc(argument.Generator, resultLocal.LocalIndex);

            return null;
        }

        protected override Expression VisitConstant(ConstantCallSite constantCallSite, ILEmitResolverBuilderContext argument)
        {
            AddConstant(argument, constantCallSite.DefaultValue);
            return null;
        }

        protected override Expression VisitCreateInstance(CreateInstanceCallSite createInstanceCallSite, ILEmitResolverBuilderContext argument)
        {
            argument.Generator.Emit(OpCodes.Newobj, createInstanceCallSite.ImplementationType.GetConstructor(Type.EmptyTypes));
            return null;
        }

        protected override Expression VisitServiceProvider(ServiceProviderCallSite serviceProviderCallSite, ILEmitResolverBuilderContext argument)
        {
            // provider
            argument.Generator.Emit(OpCodes.Ldarg_1);
            return null;
        }

        protected override Expression VisitServiceScopeFactory(ServiceScopeFactoryCallSite serviceScopeFactoryCallSite, ILEmitResolverBuilderContext argument)
        {
            // this
            argument.Generator.Emit(OpCodes.Ldarg_0);
            //      .ScopeFactory
            argument.Generator.Emit(OpCodes.Ldfld, typeof(ILEmitResolverBuilderRuntimeContext).GetField(nameof(ILEmitResolverBuilderRuntimeContext.ScopeFactory)));
            return null;
        }

        protected override Expression VisitIEnumerable(IEnumerableCallSite enumerableCallSite, ILEmitResolverBuilderContext argument)
        {
            if (enumerableCallSite.ServiceCallSites.Length == 0)
            {
                argument.Generator.Emit(OpCodes.Call, ExpressionResolverBuilder.ArrayEmptyMethodInfo.MakeGenericMethod(enumerableCallSite.ItemType));
            }
            else
            {
                // push length
                argument.Generator.Emit(OpCodes.Ldc_I4, enumerableCallSite.ServiceCallSites.Length);
                // new ItemType[length]
                argument.Generator.Emit(OpCodes.Newarr, enumerableCallSite.ItemType);
                for (int i = 0; i < enumerableCallSite.ServiceCallSites.Length; i++)
                {
                    // array
                    argument.Generator.Emit(OpCodes.Dup);
                    // push index
                    argument.Generator.Emit(OpCodes.Ldc_I4, i);
                    // create parameter
                    VisitCallSite(enumerableCallSite.ServiceCallSites[i], argument);
                    argument.Generator.Emit(OpCodes.Stelem, enumerableCallSite.ItemType);
                }
            }

            return null;
        }

        protected override Expression VisitFactory(FactoryCallSite factoryCallSite, ILEmitResolverBuilderContext argument)
        {
            if (argument.Factories == null)
            {
                argument.Factories = new List<Func<IServiceProvider, object>>();
            }

            // this
            argument.Generator.Emit(OpCodes.Ldarg_0);
            //      .Factories
            argument.Generator.Emit(OpCodes.Ldfld, FactoriesField);

            //                 i
            argument.Generator.Emit(OpCodes.Ldc_I4, argument.Factories.Count);
            //                [ ]
            argument.Generator.Emit(OpCodes.Ldelem, typeof(Func<IServiceProvider, object>));

            //                       provider
            argument.Generator.Emit(OpCodes.Ldarg_1);
            //                     (         )
            argument.Generator.Emit(OpCodes.Call, ExpressionResolverBuilder.InvokeFactoryMethodInfo);

            argument.Factories.Add(factoryCallSite.Factory);
            return null;
        }


        private void AddConstant(ILEmitResolverBuilderContext argument, object value)
        {
            if (argument.Constants == null)
            {
                argument.Constants = new List<object>();
            }

            argument.Generator.Emit(OpCodes.Ldarg_0);
            argument.Generator.Emit(OpCodes.Ldfld, ConstantsField);

            argument.Generator.Emit(OpCodes.Ldc_I4, argument.Constants.Count);
            argument.Generator.Emit(OpCodes.Ldelem, typeof(object));
            argument.Constants.Add(value);
        }


        private Func<ServiceProviderEngineScope, object> BuildType(IServiceCallSite callSite)
        {
            var dynamicMethod = new DynamicMethod("ResolveService", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, typeof(object), new [] {typeof(ILEmitResolverBuilderRuntimeContext), typeof(ServiceProviderEngineScope) }, GetType(), true);

            var info = ScopeDetector.Instance.CollectGenerationInfo(callSite);
            var context2 = GenerateMethodBody(callSite, dynamicMethod.GetILGenerator(info.Size), info);

#if SAVE_ASSEMBLY || NET461
            var assemblyName = "Test" + DateTime.Now.Ticks;

            var fileName = "Test" + DateTime.Now.Ticks;
            var assembly = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.RunAndSave);
            var module = assembly.DefineDynamicModule(assemblyName, assemblyName+".dll");
            var type = module.DefineType("Resolver");

            var method = type.DefineMethod(
                "ResolveService", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, typeof(object),
                new[] { typeof(ILEmitResolverBuilderRuntimeContext), typeof(ServiceProviderEngineScope) });

            GenerateMethodBody(callSite, method.GetILGenerator(), info);
            type.CreateTypeInfo();
            assembly.Save(assemblyName+".dll");
#endif

            return (Func<ServiceProviderEngineScope, object>)dynamicMethod.CreateDelegate(typeof(Func<ServiceProviderEngineScope, object>), context2);
        }

        private ILEmitResolverBuilderRuntimeContext GenerateMethodBody(IServiceCallSite callSite, ILGenerator generator, GenerationInfo info)
        {
            var context = new ILEmitResolverBuilderContext()
            {
                Generator = generator,
                Constants = null,
                Factories = null
            };
            var hasScopes = info.HasScope;
            if (hasScopes)
            {
                // Has to be first local defined
                var resolvedServicesLocal = context.Generator.DeclareLocal(typeof(IDictionary<object, object>));
                Debug.Assert(resolvedServicesLocal.LocalIndex == 0);
                var lockTakenLocal = context.Generator.DeclareLocal(typeof(bool));
                Debug.Assert(lockTakenLocal.LocalIndex == 1);

                context.Generator.BeginExceptionBlock();

                // scope
                context.Generator.Emit(OpCodes.Ldarg_1);
                // .ResolvedServices
                context.Generator.Emit(OpCodes.Callvirt, ResolvedServicesGetter);

                context.Generator.Emit(OpCodes.Dup);
                // Store resolved services
                context.Generator.Emit(OpCodes.Stloc_0);
                context.Generator.Emit(OpCodes.Ldloca_S, 1);

                // Monitor.Enter
                context.Generator.Emit(OpCodes.Call, ExpressionResolverBuilder.MonitorEnterMethodInfo);
            }

            VisitCallSite(callSite, context);

            if (hasScopes)
            {
                var resultLocal = context.Generator.DeclareLocal(typeof(object));

                StLoc(context.Generator, resultLocal.LocalIndex);
                context.Generator.BeginFinallyBlock();

                var postExitLabel = context.Generator.DefineLabel();
                context.Generator.Emit(OpCodes.Ldloc_1);
                context.Generator.Emit(OpCodes.Brfalse, postExitLabel);

                context.Generator.Emit(OpCodes.Ldloc, 0);

                // Monitor.Exit
                context.Generator.Emit(OpCodes.Call, ExpressionResolverBuilder.MonitorExitMethodInfo);
                context.Generator.MarkLabel(postExitLabel);

                context.Generator.EndExceptionBlock();

                LdLoc(context.Generator, resultLocal.LocalIndex);
            }

            context.Generator.Emit(OpCodes.Ret);
            return new ILEmitResolverBuilderRuntimeContext
            {
                Constants = context.Constants?.ToArray(),
                Factories = context.Factories?.ToArray(),
                Root = _rootScope,
                RuntimeResolver = _runtimeResolver,
                ScopeFactory = _serviceScopeFactory
            };
        }

        private bool TryResolveSingletonValue(SingletonCallSite singletonCallSite, out object value)
        {
            lock (_rootScope.ResolvedServices)
            {
                return _rootScope.ResolvedServices.TryGetValue(singletonCallSite.CacheKey, out value);
            }
        }

        private static bool BeginCaptureDisposable(Type implType, ILEmitResolverBuilderContext argument)
        {
            var shouldCapture = !(implType != null && !typeof(IDisposable).GetTypeInfo().IsAssignableFrom(implType.GetTypeInfo()));

            if (shouldCapture)
            {
                // context
                argument.Generator.Emit(OpCodes.Ldarg_1);
            }

            return shouldCapture;
        }
        private static void EndCaptureDisposable(ILEmitResolverBuilderContext argument)
        {
            argument.Generator.Emit(OpCodes.Callvirt, ExpressionResolverBuilder.CaptureDisposableMethodInfo);
        }

        private void LdLoc(ILGenerator generator, int index)
        {
            switch (index)
            {
                case 0: generator.Emit(OpCodes.Ldloc_0);
                    return;
                case 1: generator.Emit(OpCodes.Ldloc_1);
                    return;
                case 2: generator.Emit(OpCodes.Ldloc_2);
                    return;
                case 3: generator.Emit(OpCodes.Ldloc_3);
                    return;
            }

            if (index < byte.MaxValue)
            {
                generator.Emit(OpCodes.Ldloc_S, (byte)index);
                return;
            }

            generator.Emit(OpCodes.Ldloc, index);
        }

        private void StLoc(ILGenerator generator, int index)
        {
            switch (index)
            {
                case 0: generator.Emit(OpCodes.Stloc_0);
                    return;
                case 1: generator.Emit(OpCodes.Stloc_1);
                    return;
                case 2: generator.Emit(OpCodes.Stloc_2);
                    return;
                case 3: generator.Emit(OpCodes.Stloc_3);
                    return;
            }

            if (index < byte.MaxValue)
            {
                generator.Emit(OpCodes.Stloc_S, (byte)index);
                return;
            }

            generator.Emit(OpCodes.Stloc, index);
        }
    }
}