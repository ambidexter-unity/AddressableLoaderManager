using Common.Audio;
using Common.BundleManager;
using Common.Locale;
using Common.WindowManager;
using Zenject;

namespace Sample
{
	public class AddressableLoaderManager : AddressableLoaderManagerBase
	{
#pragma warning disable 649
		[Inject] private readonly ILocaleService _localeService;
		[Inject] private readonly IWindowManager _windowManager;
		[Inject] private readonly IAudioManager _audioManager;
#pragma warning restore 649

		protected override ILocaleService LocaleService => _localeService;
		protected override IWindowManager WindowManager => _windowManager;
		protected override IAudioManager AudioManager => _audioManager;
	}
}