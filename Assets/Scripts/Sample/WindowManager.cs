using Common.WindowManager;
using Zenject;

namespace Sample
{
	public class WindowManager : WindowManagerBase
	{
#pragma warning disable 649
		[Inject] private readonly DiContainer _container;
#pragma warning restore 649

		protected override int StartCanvasSortingOrder => 1000;

		protected override void InitWindow(IWindow window, object[] args)
		{
			_container.Inject(window);
			base.InitWindow(window, args);
		}
	}
}