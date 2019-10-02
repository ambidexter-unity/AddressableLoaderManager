using Common.Audio;
using Common.BundleManager;
using Common.Locale;
using Common.WindowManager;
using Zenject;

namespace Sample
{
	public class GameInstaller : MonoInstaller<GameInstaller>
	{
		public override void InstallBindings()
		{
			Container.Bind<IWindowManager>().FromComponentInNewPrefabResource(@"WindowManager").AsSingle();
			Container.Bind<IAudioManager>().FromComponentInNewPrefabResource(@"AudioManager").AsSingle();
			Container.Bind<ILocaleService>().To<LocaleService>().AsSingle();
			Container.Bind<IAddressableLoaderManager>().To<AddressableLoaderManager>().AsSingle();
		}
	}
}