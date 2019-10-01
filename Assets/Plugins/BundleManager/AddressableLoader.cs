using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Common.Audio;
using Common.Locale;
using Common.WindowManager;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using AudioSettings = Common.Audio.AudioSettings;

// ReSharper disable once CheckNamespace
namespace Common.BundleManager
{
	public sealed class AddressableLoader : MonoBehaviour, IAddressableLoader
	{
		private AsyncOperationHandle<IList<object>>? _handle;

		private bool _dontDestroyOnDisposed;
		private bool _isDisposed;

		private readonly HashSet<SpriteAtlas> _atlases = new HashSet<SpriteAtlas>();
		private readonly HashSet<string> _clips = new HashSet<string>();
		private readonly HashSet<string> _windows = new HashSet<string>();

		private readonly Regex _rx = new Regex(@"^\w{2}_\w{2}$");

		private Coroutine _loadRoutine;

		private void Awake()
		{
			DontDestroyOnLoad(gameObject);
		}

		private void OnDestroy()
		{
			if (_loadRoutine != null)
			{
				StopCoroutine(_loadRoutine);
				_loadRoutine = null;
			}

			_dontDestroyOnDisposed = true;
			Dispose();
		}

		public ILocaleService LocaleService { set; private get; }
		public IWindowManager WindowManager { set; private get; }
		public IAudioManager AudioManager { set; private get; }

		private void OnSpriteAtlasRequested(string atlasName, Action<SpriteAtlas> callback)
		{
			Debug.LogFormat("Require atlas {0} that is {1}", atlasName,
				_atlases.Count(atlas => atlas.name == atlasName) > 0 ? "exists" : "not exists");
			callback?.Invoke(_atlases.FirstOrDefault(asset => asset.name == atlasName));
		}

		public void Load(IEnumerable<string> keys,
			Addressables.MergeMode mergeMode = Addressables.MergeMode.Intersection)
		{
			if (_handle != null || _isDisposed) return;
			_loadRoutine = StartCoroutine(LoadAssetsRoutine(keys.Cast<object>().ToList(), mergeMode));
		}

		private IEnumerator LoadAssetsRoutine(IList<object> keys, Addressables.MergeMode mergeMode)
		{
			_handle = Addressables.LoadAssetsAsync<object>(keys, null, mergeMode);
			yield return _handle;

			if (!_isDisposed && _handle.Value.Status == AsyncOperationStatus.Succeeded)
			{
				Assets = _handle.Value.Result.ToList();
				foreach (var o in Assets)
				{
					RegisterAsset(o);
				}

				if (_atlases.Any())
				{
					SpriteAtlasManager.atlasRequested += OnSpriteAtlasRequested;
				}
			}

			_loadRoutine = null;
			CompletedEvent?.Invoke(_handle?.Status ?? AsyncOperationStatus.Failed);
		}

		private void RegisterAsset(object obj)
		{
			switch (obj)
			{
				case TextAsset textAsset:
					CheckAndRegisterLocale(textAsset);
					break;
				case SpriteAtlas spriteAtlas:
					_atlases.Add(spriteAtlas);
					break;
				case AudioSettings audioSettings:
					RegisterAudioSettings(audioSettings);
					break;
				case GameObject asset:
					var window = asset.GetComponent<Window>();
					if (window != null) RegisterWindow(window);
					break;
			}
		}

		private void CheckAndRegisterLocale(TextAsset asset)
		{
			var firstLine = new StringReader(asset.text).ReadLine();
			if (string.IsNullOrEmpty(firstLine)) return;
			var keys = firstLine.Split(',').Where(s => !string.IsNullOrEmpty(s)).ToArray();
			if (keys.All(s => _rx.IsMatch(s))) LocaleService.AddLocaleCsv(asset.text);
		}

		private void RegisterAudioSettings(AudioSettings asset)
		{
			foreach (var pair in asset.Clips)
			{
				AudioManager.RegisterClips(pair.Value, pair.Key);
				_clips.UnionWith(pair.Value.Keys);
			}
		}

		private void RegisterWindow(Window window)
		{
			WindowManager.RegisterWindow(window, true);
			_windows.Add(window.WindowId);
		}

		public event LoaderCompletedHandler CompletedEvent;

		public float Progress => _handle?.PercentComplete ?? 0f;

		public bool IsLoaded => _handle?.IsDone ?? false;

		public IReadOnlyList<object> Assets { get; private set; }

		public void Dispose()
		{
			if (_isDisposed) return;
			_isDisposed = true;

			foreach (var windowId in _windows)
			{
				WindowManager.UnregisterWindow(windowId);
			}

			AudioManager.UnregisterClips(_clips);

			if (_atlases.Any())
			{
				SpriteAtlasManager.atlasRequested -= OnSpriteAtlasRequested;
				_atlases.Clear();
			}

			_windows.Clear();
			_clips.Clear();

			if (_handle != null)
			{
				Addressables.Release(_handle.Value);
				_handle = null;
			}

			if (!_dontDestroyOnDisposed)
			{
				Destroy(gameObject);
			}
		}
	}
}