namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// todo
	/// </summary>
	public interface IServiceProviderOptions
	{
		/// <summary>
		/// todo
		/// </summary>
		ServiceProviderMode Mode { get; set; }

		/// <summary>
		/// todo
		/// </summary>
		ITrackSingletonServices SingletonTracker { get; set; }

		/// <summary>
		/// <c>true</c> to perform check verifying that scoped services never gets resolved from root provider; otherwise <c>false</c>.
		/// </summary>
		bool ValidateScopes { get; set; }
	}
}