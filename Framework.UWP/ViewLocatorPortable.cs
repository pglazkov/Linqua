using Framework.PlatformServices;

namespace Framework
{
    public static class ViewLocatorPortable
    {
        public static object LocateForViewModel(IViewModel viewModel)
        {
            return CompositionManager.Current.GetInstance<IViewLocationService>().LocateForViewModel(viewModel);
        }
    }
}