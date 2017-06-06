// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// A factory for creating the application's <see cref="IServiceProvider"/> at design time. Implement this interface
    /// to enable design-time services that need access to the application's services. Design-time services will
    /// automatically discover implementations of this interface.
    /// </summary>
    public interface IDesignTimeServiceProviderFactory
    {
        /// <summary>
        /// Creates a new instance of the application's <see cref="IServiceProvider"/>.
        /// </summary>
        /// <param name="args">Arguments provided by the design-time service.</param>
        /// <returns>The application's <see cref="IServiceProvider"/>.</returns>
        IServiceProvider CreateServiceProvider(string[] args);
    }
}
