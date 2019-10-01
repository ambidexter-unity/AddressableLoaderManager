using Common.BundleManager;
using Common.GameService;
using Common.GameTask;
using Common.Locale;
using Common.WindowManager;
using UnityEngine;
using UnityEngine.U2D;
using Zenject;

namespace Sample
{
	public class SampleSceneBehaviour : MonoInstaller<SampleSceneBehaviour>
	{
		private IAddressableLoader _ldr;

#pragma warning disable 649
		[Inject] private readonly IAddressableLoaderManager _addressableLoaderManager;
		[Inject] private readonly ILocaleService _localeService;
		[Inject] private readonly IWindowManager _windowManager;
#pragma warning restore 649

		public override void InstallBindings()
		{
			SpriteAtlasManager.atlasRequested += (s, action) => Debug.LogFormat("Require atlas {0}", s);
		}

		public override void Start()
		{
			var initQueue = new GameTaskQueue();
			initQueue.Add(new GameTaskInitService(_localeService));

			initQueue.CompleteEvent += OnInitComplete;
			initQueue.Start();
		}

		private void OnInitComplete(IGameTask task)
		{
			
//			_addressableLoaderManager.LoadAddressable("WindowAtlas", "sd");
/*
				.CompletedEvent += status =>
				_ldr = _addressableLoaderManager.LoadAddressable<object>("Popup2");
*/
//			Debug.Log("Download percent: 0");
		}
/*

		private void Update()
		{
			if (_ldr == null) return;
			Debug.LogFormat("Download percent: {0}", _ldr.Progress);
			if (_ldr.IsLoaded)
			{
				OnAssetsLoaded(_ldr);
				_ldr = null;
			}
		}
*/

		private void OnAssetsLoaded(IAddressableLoader ldr)
		{
			_windowManager.ShowWindow("popup_2");
		}
	}
}