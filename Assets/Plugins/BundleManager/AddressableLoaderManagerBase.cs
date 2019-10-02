using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Common.Audio;
using Common.Locale;
using Common.WindowManager;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.U2D;
using AudioSettings = Common.Audio.AudioSettings;

// ReSharper disable once CheckNamespace
namespace Common.BundleManager
{
	/// <summary>
	/// Сервис управления загрузчиками бандлов.
	/// </summary>
	public abstract class AddressableLoaderManagerBase : IAddressableLoaderManager
	{
		private readonly List<AddressableLoader> _loaders = new List<AddressableLoader>();

		private readonly Dictionary<string, Action<SpriteAtlas>> _delayedAtlasRequests =
			new Dictionary<string, Action<SpriteAtlas>>();

		private readonly Regex _rx = new Regex(@"^\w{2}_\w{2}$");

		protected abstract ILocaleService LocaleService { get; }
		protected abstract IWindowManager WindowManager { get; }
		protected abstract IAudioManager AudioManager { get; }

		protected AddressableLoaderManagerBase()
		{
			SpriteAtlasManager.atlasRequested += OnAtlasRequested;
		}

		private void OnAtlasRequested(string atlasName, Action<SpriteAtlas> callback)
		{
			var atlas = _loaders.SelectMany(loader => loader.Assets).Where(o => o is SpriteAtlas).Cast<SpriteAtlas>()
				.FirstOrDefault(spriteAtlas => spriteAtlas.name == atlasName);
			if (atlas) callback?.Invoke(atlas);
			else _delayedAtlasRequests.Add(atlasName, callback);
			Debug.LogFormat("Require atlas {0} that is {1}", atlasName, atlas ? "exists" : "not exists");
		}

		// IBundleService

		public IAddressableLoader LoadAddressable(params string[] args)
		{
			var ldr = new AddressableLoader();

			_loaders.Add(ldr);
			ldr.CompletedEvent += LdrOnCompleted;
			ldr.DisposeEvent += LdrOnDispose;

			ldr.Load(args);
			return ldr;
		}

		private void LdrOnDispose(AddressableLoader loader)
		{
			loader.DisposeEvent -= LdrOnDispose;
			_loaders.Remove(loader);

			var sounds = loader.Assets.Where(o => o is AudioSettings).Cast<AudioSettings>()
				.SelectMany(settings => settings.Clips.Values).SelectMany(clips => clips.Keys)
				.Distinct().ToArray();
			var windows = loader.Assets.Where(o => (o as GameObject)?.GetComponent<Window>())
				.Cast<GameObject>().Select(o => o.GetComponent<Window>().WindowId).ToArray();

			if (sounds.Length > 0) AudioManager.UnregisterClips(sounds);

			foreach (var windowId in windows)
			{
				WindowManager.UnregisterWindow(windowId);
			}

			Resources.UnloadUnusedAssets();
		}

		private void LdrOnCompleted(IAddressableLoader loader, AsyncOperationStatus status)
		{
			loader.CompletedEvent -= LdrOnCompleted;

			var atlases = loader.Assets.Where(o => o is SpriteAtlas).Cast<SpriteAtlas>().ToArray();
			var sounds = loader.Assets.Where(o => o is AudioSettings).Cast<AudioSettings>().ToArray();
			var locales = loader.Assets.Where(IsLocale).Cast<TextAsset>().ToArray();
			var windows = loader.Assets.Where(o => (o as GameObject)?.GetComponent<Window>())
				.Cast<GameObject>().Select(o => o.GetComponent<Window>()).ToArray();

			if (atlases.Length > 0)
			{
				foreach (var pair in _delayedAtlasRequests.ToList())
				{
					if (atlases.Any(atlas =>
					{
						if (atlas.name != pair.Key) return false;
						pair.Value?.Invoke(atlas);
						_delayedAtlasRequests.Remove(pair.Key);
						return true;
					}))
					{
						Debug.LogFormat("Request for atlas {0} was found and satisfied.", pair.Key);
						break;
					}
				}
			}

			foreach (var audioSettings in sounds)
			{
				foreach (var pair in audioSettings.Clips)
				{
					AudioManager.RegisterClips(pair.Value, pair.Key);
				}
			}

			foreach (var locale in locales)
			{
				LocaleService.AddLocaleCsv(locale.text);
			}

			foreach (var window in windows)
			{
				WindowManager.RegisterWindow(window, true);
			}
		}

		// \IBundleService

		private bool IsLocale(object asset)
		{
			var textAsset = asset as TextAsset;
			if (!textAsset) return false;

			var firstLine = new StringReader(textAsset.text).ReadLine();
			if (string.IsNullOrEmpty(firstLine)) return false;
			var keys = firstLine.Split(',').Where(s => !string.IsNullOrEmpty(s)).ToArray();
			return keys.All(s => _rx.IsMatch(s));
		}
	}
}