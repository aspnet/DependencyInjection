using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection
{
	/// <summary>
	/// todo
	/// </summary>
	public interface ITrackSingletonServices : IDisposable
	{
		/// <summary>
		/// todo
		/// </summary>
		/// <returns></returns>
		object GetLock();

		/// <summary>
		/// todo
		/// </summary>
		/// <returns></returns>
		IDictionary<object, object> GetResolvedSingletons();

		/// <summary>
		/// todo
		/// </summary>
		void TrackDisposable(object resolved);
	}
}