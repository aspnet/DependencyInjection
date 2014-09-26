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
        private IDictionary<ParamTypeLookupKey, InstanceCreator> _creatorByParamsType = new Dictionary<ParamTypeLookupKey, InstanceCreator>();

        public object CreateInstance(IServiceProvider services, Type instanceType, params object[] parameters)
        {
            var paramTypes = new Type[parameters.Length];
            for (int i = 0; i < paramTypes.Length; i++)
            {
                paramTypes[i] = parameters[i].GetType();
            }
            var paramTypeKey = new ParamTypeLookupKey(instanceType, paramTypes);

            InstanceCreator creator;
            if (!_creatorByParamsType.TryGetValue(paramTypeKey, out creator))
            {
                var bestLength = -1;
                foreach (var constructor in instanceType.GetTypeInfo().DeclaredConstructors.Where(c => !c.IsStatic))
                {
                    InstanceCreator prototype;
                    int applyExactLength = InstanceCreator.TryCreate(constructor, parameters, out prototype);
                    if (applyExactLength > bestLength)
                    {
                        creator = prototype;
                        bestLength = applyExactLength;

                        if (bestLength == parameters.Length)
                        {
                            // best possible result, break
                            break;
                        }
                    }
                }

                if (creator != null)
                {
                    lock (_creatorByParamsType)
                    {
                        if (!_creatorByParamsType.ContainsKey(paramTypeKey))
                        {
                            _creatorByParamsType.Add(paramTypeKey, creator);
                        }
                    }
                }
            }

            if (creator == null)
            {
                throw new Exception(
                    string.Format(
                        "TODO: unable to locate suitable constructor for {0}. " +
                        "Ensure 'instanceType' is concrete and all parameters are accepted by a constructor.",
                        instanceType));
            }

            return creator.CreateInstance(services, parameters);
        }

        private class InstanceCreator
        {
            private readonly ConstructorInfo _constructor;
            private readonly int[] _paramPermutation;
            private readonly Type[] _serviceClosure;

            public static int TryCreate(ConstructorInfo constructor, object[] givenParameters, out InstanceCreator creator)
            {
                var constructorParams = constructor.GetParameters();
                var serviceClosure = new Type[constructorParams.Length];
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

                // for unassigned parameters, sevice closure is needed
                for (int i = 0; i < constructorParams.Length; i++)
                {
                    if (!parameterValuesSet[i])
                    {
                        serviceClosure[i] = constructorParams[i].ParameterType;
                    }
                }

                creator = new InstanceCreator(constructor, paramPermutation, serviceClosure);
                return applyExactLength;
            }

            private InstanceCreator(ConstructorInfo constructor, int[] paramPermutation, Type[] serviceClosure)
            {
                _constructor = constructor;
                _paramPermutation = paramPermutation;
                _serviceClosure = serviceClosure;
            }

            public object CreateInstance(IServiceProvider services, object[] parameters)
            {
                var rearrangedParams = new object[_serviceClosure.Length];

                for (int i = 0; i < _paramPermutation.Length; i++)
                {
                    var selectedIndex = _paramPermutation[i];
                    rearrangedParams[selectedIndex] = parameters[i];
                }

                for (int i = 0; i < _serviceClosure.Length; i++)
                {
                    var closureType = _serviceClosure[i];
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
                    for (int i = 0; i < _paramsType.Length; i++)
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
