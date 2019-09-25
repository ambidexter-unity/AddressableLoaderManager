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
			Container.Bind<IAddressableLoaderManager>().To<AddressableLoaderManager>().AsSingle();
			Container.Bind<ILocaleService>().To<LocaleService>().AsSingle();
		}
	}
}