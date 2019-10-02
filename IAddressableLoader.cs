using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;

// ReSharper disable once CheckNamespace
namespace Common.BundleManager
{
	public delegate void LoaderCompletedHandler(IAddressableLoader loader, AsyncOperationStatus status);

	public interface IAddressableLoader : IDisposable, IEnumerator
	{
		event LoaderCompletedHandler CompletedEvent;
		float Progress { get; }
		bool IsLoaded { get; }
		IReadOnlyList<object> Assets { get; }
		AsyncOperationStatus Status { get; }
	}
}