using Framework.PlatformServices;

namespace Framework
{
	public static class ViewLocatorPortable
	{
		public static object LocateForViewModel(IViewModel viewModel)
		{
			return PlatformService.Platform.CreateService<IViewLocationService>().LocateForViewModel(viewModel);
		}
	}
}