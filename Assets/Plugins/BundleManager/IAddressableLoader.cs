using System;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;

// ReSharper disable once CheckNamespace
namespace Common.BundleManager
{
	public delegate void LoaderCompletedHandler(AsyncOperationStatus status);

	public interface IAddressableLoader : IDisposable
	{
		event LoaderCompletedHandler CompletedEvent;
		float Progress { get; }
		bool IsLoaded { get; }
		IReadOnlyList<object> Assets { get; }
	}
}