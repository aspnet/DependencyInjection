// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.Framework.DependencyInjection
{
    public class ContextAccessor<TContext> : IContextAccessor<TContext>
    {
        private TContext _value;

        public TContext Value
        {
            get
            {
                return _value;
            }
        }

        public TContext SetValue(TContext value)
        {
            var prior = _value;
            _value = value;
            return prior;
        }
    }
}