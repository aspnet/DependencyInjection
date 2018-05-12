using System;

namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// todo
	/// </summary>
	public interface IServiceProviderWithOptions : IServiceProvider
	{
		/// <summary>
		/// todo
		/// </summary>
		IServiceProviderOptions Options { get; }
	}
}