// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

using InstanceFactory = System.Func<System.IServiceProvider, object[], object>;

namespace Microsoft.Framework.DependencyInjection
{
    public class TypeActivator : ITypeActivator
    {
        private ConcurrentDictionary<ParamTypeLookupKey, InstanceFactory> _factoriesByparameterTypes
            = new ConcurrentDictionary<ParamTypeLookupKey, InstanceFactory>();

        public object CreateInstance(IServiceProvider services, Type instanceType, object[] parameters)
        {
            // We may be able to save an allocation here by modifying ParamTypeLookupKey class to know how to
            // compare (object[] parameters)
            // Note that this change would complicate the logic of ParamTypeLookupKey quite a bit and it does
            // not improve performace of no param calls. Let's look at the usage data before making the move
            Type[] paramTypes;
            if (parameters.Length == 0)
            {
                paramTypes = null;
            }
            else
            {
                paramTypes = new Type[parameters.Length];
                for (var i = 0; i < paramTypes.Length; i++)
                {
                    paramTypes[i] = parameters[i].GetType();
                }
            }

            var paramTypeKey = new ParamTypeLookupKey(instanceType, paramTypes);

            InstanceFactory factory;
            if (!_factoriesByparameterTypes.TryGetValue(paramTypeKey, out factory))
            {
                var bestLength = -1;
                InstanceFactoryBuilder bestBuilder = null;
                var parametersCount = parameters == null ? 0 : parameters.Length;
                foreach (var constructor in instanceType.GetTypeInfo().DeclaredConstructors)
                {
                    if (!constructor.IsStatic)
                    {
                        InstanceFactoryBuilder candidate;
                        var applyExactLength = InstanceFactoryBuilder.Create(constructor, parameters, out candidate);
                        if (applyExactLength > bestLength)
                        {
                            bestBuilder = candidate;
                            bestLength = applyExactLength;

                            if (bestLength == parametersCount)
                            {
                                // best possible result, break
                                break;
                            }
                        }
                    }
                }

                if (bestBuilder == null)
                {
                    throw new InvalidOperationException(string.Format(
                        "Internal error: cannot determine creation logic for type {0}, perhaps there is no accessible constructor?", instanceType.FullName));
                }

                factory = bestBuilder.Build();
                _factoriesByparameterTypes.TryAdd(paramTypeKey, factory);
            }

            if (factory == null)
            {
                throw new Exception(
                    string.Format(
                        "TODO: unable to locate suitable constructor for {0}. " +
                        "Ensure 'instanceType' is concrete and all parameters are accepted by a constructor.",
                        instanceType));
            }

            return factory(services, parameters);
        }

        private class InstanceFactoryBuilder
        {
            private readonly ConstructorInfo _constructor;
            private readonly Type[] _constructorParamTypes;
            private readonly int[] _paramMapping;

            private static readonly MethodInfo GetServiceMethod = typeof(InstanceFactoryBuilder).GetTypeInfo().GetDeclaredMethod("GetService");

            private InstanceFactoryBuilder(ConstructorInfo constructor, Type[] constructorParamTypes, int[] paramMapping)
            {
                _constructor = constructor;
                _constructorParamTypes = constructorParamTypes;
                _paramMapping = paramMapping;
            }

            /// <summary>
            /// Instance creation logic is built into an one line lambda expression such as
            /// (serviceProvider, parameters) => new MyClass((T1) parameters[0], (T2) serviceProvider.GetService(T2), (T3) parameters[1])
            /// </summary>
            public InstanceFactory Build()
            {
                var serviceProviderParam = Expression.Parameter(typeof(IServiceProvider));
                var createArgsParam = Expression.Parameter(typeof(object[]));

                var constructorExpressions = new List<Expression>();
                for (int i = 0; i < _constructorParamTypes.Length; i++)
                {
                    Expression selected;
                    var mappedIndex = _paramMapping[i];
                    if (mappedIndex >= 0)
                    {
                        // get it from args
                        // note that the method that we build is
                        // technically null safe against null value for the second parameter (of type object[])
                        // since in that case all mapped index would have stayed as -1 when the builder is created
                        // and this clause is never called
                        selected = Expression.ArrayIndex(createArgsParam, Expression.Constant(mappedIndex));
                    }
                    else
                    {
                        // get it from service provider
                        var GetServiceGenericMethod = GetServiceMethod.MakeGenericMethod(_constructorParamTypes[i]);
                        selected = Expression.Call(GetServiceGenericMethod, serviceProviderParam);
                    }

                    constructorExpressions.Add(Expression.Convert(selected, _constructorParamTypes[i]));
                }

                var createInstanceExpression = Expression.New(_constructor, constructorExpressions);
                var callback = Expression.Lambda<InstanceFactory>(createInstanceExpression,
                    new[] { serviceProviderParam, createArgsParam });

                return callback.Compile();
            }

            public static int Create(ConstructorInfo constructor, object[] givenParameters, out InstanceFactoryBuilder factory)
            {
                var constructorParams = constructor.GetParameters();
                var constructorParamTypes = new Type[constructorParams.Length];
                var parameterValuesSet = new bool[constructorParams.Length];
                var paramMapping = new int[constructorParams.Length];

                for (var i = 0; i < constructorParams.Length; i++)
                {
                    constructorParamTypes[i] = constructorParams[i].ParameterType;
                    paramMapping[i] = -1;
                }

                var applyIndexStart = 0;
                var applyExactLength = 0;
                var givenParametersCount = givenParameters == null ? 0 : givenParameters.Length;
                for (var givenIndex = 0; givenIndex != givenParametersCount; ++givenIndex)
                {
                    var givenType = givenParameters[givenIndex] == null ? null : givenParameters[givenIndex].GetType().GetTypeInfo();
                    var givenMatched = false;

                    for (var applyIndex = applyIndexStart; !givenMatched && applyIndex != constructorParams.Length; ++applyIndex)
                    {
                        if (!parameterValuesSet[applyIndex] &&
                            constructorParams[applyIndex].ParameterType.GetTypeInfo().IsAssignableFrom(givenType))
                        {
                            givenMatched = true;
                            paramMapping[applyIndex] = givenIndex;
                            parameterValuesSet[applyIndex] = true;
                            if (applyIndexStart == applyIndex)
                            {
                                applyIndexStart++;
                                if (applyIndex == givenIndex)
                                {
                                    applyExactLength = applyIndex;
                                }
                            }
                        }
                    }

                    if (!givenMatched)
                    {
                        factory = null;
                        return -1;
                    }
                }

                factory = new InstanceFactoryBuilder(constructor, constructorParamTypes, paramMapping);
                return applyExactLength;
            }

            /// <summary>
            /// This method is here to workaround a bug in CoreCLR
            /// At runtime the compiled expression is unable to call System.IServiceProvider.GetService
            /// because somehow the runtime environment would decide that the caller has no permission to do so
            /// </summary>
            private static object GetService<T>(IServiceProvider provider)
            {
                return provider.GetService(typeof(T));
            }
        }

        private struct ParamTypeLookupKey
        {
            private readonly Type _instanceType;
            private readonly Type[] _parameterTypes;

            public ParamTypeLookupKey(
                [NotNull]
                Type instanceType,
                Type[] parameterTypes)
            {
                _instanceType = instanceType;
                _parameterTypes = parameterTypes;
            }

            public override bool Equals(object obj)
            {
                var other = (ParamTypeLookupKey) obj;

                if (_instanceType != other._instanceType)
                {
                    return false;
                }

                if (_parameterTypes != other._parameterTypes)
                {
                    if (_parameterTypes ==null || other._parameterTypes == null)
                    {
                        return false;
                    }

                    if (_parameterTypes.Length != other._parameterTypes.Length)
                    {
                        return false;
                    }

                    for (int i = 0; i < _parameterTypes.Length; i++)
                    {
                        if (_parameterTypes[i] != other._parameterTypes[i])
                        {
                            return false;
                        }
                    }
                }

                return true;
            }

            public override int GetHashCode()
            {
                int hashCode = 0;
                unchecked
                {
                    if (_parameterTypes != null)
                    {
                        for (var i = 0; i < _parameterTypes.Length; i++)
                        {
                            hashCode = (hashCode + _parameterTypes[i].GetHashCode()) * 6793;
                        }
                    }
                    hashCode += _instanceType.GetHashCode();
                }
                return hashCode;
            }

            public override string ToString()
            {
                return string.Format("Instance: {0} Args: [{1}]", _instanceType.FullName, string.Join(", ", _parameterTypes.ToString()));
            }
        }
    }
}
