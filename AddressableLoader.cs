using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

// ReSharper disable once CheckNamespace
namespace Common.BundleManager
{
	internal sealed class AddressableLoader : IAddressableLoader
	{
		private readonly object[] _emptyAssets = new object[0];

		private AsyncOperationHandle<IList<object>>? _handle;
		private bool _isDisposed;

		public AddressableLoader()
		{
			Assets = _emptyAssets;
		}

		public bool MoveNext()
		{
			return ((IEnumerator) _handle)?.MoveNext() ?? true;
		}

		public void Reset()
		{
		}

		public object Current => ((IEnumerator) _handle)?.Current;

		public void Load(IEnumerable<string> keys,
			Addressables.MergeMode mergeMode = Addressables.MergeMode.Intersection)
		{
			if (_handle != null || _isDisposed) return;

			_handle = Addressables.LoadAssetsAsync<object>(keys.Cast<object>().ToList(), null, mergeMode);
			if (_handle.Value.IsDone) OnComplete(_handle.Value);
			else _handle.Value.Completed += OnComplete;
		}

		private void OnComplete(AsyncOperationHandle<IList<object>> handle)
		{
			handle.Completed -= OnComplete;
			if (!_isDisposed && handle.Status == AsyncOperationStatus.Succeeded)
			{
				Assets = handle.Result.ToList();
			}

			CompletedEvent?.Invoke(this, handle.Status);
		}

		public event LoaderCompletedHandler CompletedEvent;

		public float Progress => _handle?.PercentComplete ?? 0f;

		public bool IsLoaded => _handle?.IsDone ?? false;

		public IReadOnlyList<object> Assets { get; private set; }

		internal event Action<AddressableLoader> DisposeEvent;

		public void Dispose()
		{
			if (_isDisposed) return;
			_isDisposed = true;

			DisposeEvent?.Invoke(this);

			if (_handle != null)
			{
				Addressables.Release(_handle.Value);
				_handle = null;
			}

			Assets = _emptyAssets;
		}
	}
}