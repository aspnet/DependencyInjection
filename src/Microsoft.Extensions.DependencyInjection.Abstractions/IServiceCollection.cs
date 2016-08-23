// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Specifies the contract for a collection of service descriptors.
    /// </summary>
    public interface IServiceCollection : IList<ServiceDescriptor>
    {
    }

    public interface IServicesEditor: IList<ServiceDescriptor>
    {
        Type ServiceType { get; }
    }

    public interface IServicesEditor<in T> : IServicesEditor
    {
    }

}
