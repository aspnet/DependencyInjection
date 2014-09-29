// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Microsoft.Framework.DependencyInjection
{
    public class TypeActivator : ITypeActivator
    {
        private IDictionary<ParamTypeLookupKey, InstanceFactory> _creatorByParamsType = new Dictionary<ParamTypeLookupKey, InstanceFactory>();

        public object CreateInstance(IServiceProvider services, Type instanceType, params object[] parameters)
        {
            var paramTypes = new Type[parameters.Length];
            for (var i = 0; i < paramTypes.Length; i++)
            {
                paramTypes[i] = parameters[i].GetType();
            }
            var paramTypeKey = new ParamTypeLookupKey(instanceType, paramTypes);

            InstanceFactory factory;
            if (!_creatorByParamsType.TryGetValue(paramTypeKey, out factory))
            {
                var bestLength = -1;
                foreach (var constructor in instanceType.GetTypeInfo().DeclaredConstructors)
                {
                    if (!constructor.IsStatic)
                    {
                        var applyExactLength = InstanceFactory.CreateFactory(constructor, parameters, out var prototype);
                        if (applyExactLength > bestLength)
                        {
                            factory = prototype;
                            bestLength = applyExactLength;

                            if (bestLength == parameters.Length)
                            {
                                // best possible result, break
                                break;
                            }
                        }
                    }
                }

                if (factory != null)
                {
                    lock (_creatorByParamsType)
                    {
                        if (!_creatorByParamsType.ContainsKey(paramTypeKey))
                        {
                            _creatorByParamsType.Add(paramTypeKey, factory);
                        }
                    }
                }
            }

            if (factory == null)
            {
                throw new Exception(
                    string.Format(
                        "TODO: unable to locate suitable constructor for {0}. " +
                        "Ensure 'instanceType' is concrete and all parameters are accepted by a constructor.",
                        instanceType));
            }

            return factory.CreateInstance(services, parameters);
        }

        private class InstanceFactory
        {
            private readonly ConstructorInfo _constructor;
            private readonly int[] _paramPermutation;
            private readonly Type[] _dependencyInjectedTypes;

            public static int CreateFactory(ConstructorInfo constructor, object[] givenParameters, out InstanceFactory creator)
            {
                var constructorParams = constructor.GetParameters();
                var dependencyInjectedTypes = new Type[constructorParams.Length];
                var parameterValuesSet = new bool[constructorParams.Length];
                var paramPermutation = new int[givenParameters.Length];

                var applyIndexStart = 0;
                var applyExactLength = 0;
                for (var givenIndex = 0; givenIndex != givenParameters.Length; ++givenIndex)
                {
                    var givenType = givenParameters[givenIndex] == null ? null : givenParameters[givenIndex].GetType().GetTypeInfo();
                    var givenMatched = false;

                    for (var applyIndex = applyIndexStart; givenMatched == false && applyIndex != constructorParams.Length; ++applyIndex)
                    {
                        if (parameterValuesSet[applyIndex] == false &&
                            constructorParams[applyIndex].ParameterType.GetTypeInfo().IsAssignableFrom(givenType))
                        {
                            givenMatched = true;
                            paramPermutation[givenIndex] = applyIndex;
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

                    if (givenMatched == false)
                    {
                        creator = null;
                        return -1;
                    }
                }

                // Find parameters that remains unassigned, we need to get these parameters from service provider
                for (var i = 0; i < constructorParams.Length; i++)
                {
                    if (!parameterValuesSet[i])
                    {
                        dependencyInjectedTypes[i] = constructorParams[i].ParameterType;
                    }
                }

                creator = new InstanceFactory(constructor, paramPermutation, dependencyInjectedTypes);
                return applyExactLength;
            }

            private InstanceFactory(ConstructorInfo constructor, int[] paramPermutation, Type[] dependencyInjectedTypes)
            {
                _constructor = constructor;
                _paramPermutation = paramPermutation;
                _dependencyInjectedTypes = dependencyInjectedTypes;
            }

            public object CreateInstance(IServiceProvider services, object[] parameters)
            {
                var rearrangedParams = new object[_dependencyInjectedTypes.Length];

                for (int i = 0; i < _paramPermutation.Length; i++)
                {
                    var selectedIndex = _paramPermutation[i];
                    rearrangedParams[selectedIndex] = parameters[i];
                }

                for (int i = 0; i < _dependencyInjectedTypes.Length; i++)
                {
                    var closureType = _dependencyInjectedTypes[i];
                    if (closureType != null)
                    {
                        var paramValue = services.GetService(closureType);
                        if (paramValue == null)
                        {
                            throw new Exception(string.Format("TODO: unable to resolve service {1} to create {0}", _constructor.DeclaringType, closureType));
                        }
                        rearrangedParams[i] = paramValue;
                    }
                }

                return _constructor.Invoke(rearrangedParams);
            }
        }

        private class ParamTypeLookupKey
        {
            private readonly Type _instanceType;
            private readonly Type[] _paramsType;
            private readonly int _hashCache;

            public ParamTypeLookupKey(Type instanceType, Type[] paramsType)
            {
                if (instanceType == null || paramsType == null)
                {
                    throw new InvalidOperationException("Invalid parameters");
                }

                _instanceType = instanceType;
                _paramsType = paramsType;

                // evaluate hashcode and cache it
                int hashCode = 0;
                unchecked
                {
                    for (var i = 0; i < _paramsType.Length; i++)
                    {
                        hashCode = (hashCode + _paramsType[i].GetHashCode()) * 6793;
                    }
                    hashCode += _instanceType.GetHashCode();
                }
                _hashCache = hashCode;
            }

            public override bool Equals(object obj)
            {
                var other = obj as ParamTypeLookupKey;

                if (other == null ||
                    _instanceType != other._instanceType ||
                    _paramsType.Length != other._paramsType.Length)
                {
                    return false;
                }

                for (int i = 0; i < _paramsType.Length; i++)
                {
                    if (_paramsType[i] != other._paramsType[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            public override int GetHashCode()
            {
                return _hashCache;
            }
        }
    }
}
