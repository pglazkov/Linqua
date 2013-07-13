namespace Framework.PlatformServices
{
	public class ViewLocationService : IViewLocationService
	{
		public object LocateForViewModel(IViewModel viewModel)
		{
			return ViewLocator.Locate(viewModel);
		}
	}
}