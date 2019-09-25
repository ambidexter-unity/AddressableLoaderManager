using Common.BundleManager;
using Common.GameService;
using Common.GameTask;
using Common.Locale;
using Zenject;

namespace Sample
{
	public class SampleSceneBehaviour : MonoInstaller<SampleSceneBehaviour>
	{
#pragma warning disable 649
		[Inject] private readonly IAddressableLoaderManager _addressableLoaderManager;
		[Inject] private readonly ILocaleService _localeService;
#pragma warning restore 649

		public override void InstallBindings()
		{
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
			throw new System.NotImplementedException();
		}
	}
}