using System;
using System.Collections.Generic;

namespace Microsoft.Extensions.DependencyInjection.ServiceLookup
{
	/// <summary>
	/// todo
	/// </summary>
	public class SingletonServiceTracker : ITrackSingletonServices
	{
		internal static readonly object Synchronise = new object();

		internal static List<object> Disposables { get; private set; } = new List<object>(); 

		internal static Dictionary<object, object> StaticResolvedServices { get; } = new Dictionary<object, object>();

		/// <summary>
		/// todo
		/// </summary>
		/// <returns></returns>
		public object GetLock()
		{
			return Synchronise;
		}

		/// <summary>
		/// todo
		/// </summary>
		/// <returns></returns>
		public IDictionary<object, object> GetResolvedSingletons()
		{
			return StaticResolvedServices;
		}

		/// <summary>
		/// todo
		/// </summary>
		/// <param name="resolved"></param>
		public void TrackDisposable(object resolved)
		{
			if (resolved is IDisposable)
			{
				Disposables.Add(resolved);
			}
		}

		/// <summary>
		/// todo
		/// </summary>
		public void Dispose()
		{
			lock (GetLock())
			{
				foreach (var disposable in Disposables)
				{
					(disposable as IDisposable)?.Dispose();
				}

				Disposables = new List<object>();
			}
		}
	}
}