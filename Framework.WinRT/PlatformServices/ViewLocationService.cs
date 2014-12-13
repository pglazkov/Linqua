using System.Composition;

namespace Framework.PlatformServices
{
    [Export(typeof(IViewLocationService))]
	public class ViewLocationService : IViewLocationService
	{
        public ViewLocationService()
        {
            
        }

		public object LocateForViewModel(IViewModel viewModel)
		{
			return ViewLocator.Locate(viewModel);
		}
	}
}