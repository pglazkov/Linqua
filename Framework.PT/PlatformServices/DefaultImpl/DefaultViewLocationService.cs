using Framework.MefExtensions;

namespace Framework.PlatformServices.DefaultImpl
{
    [DefaultExport(typeof(IViewLocationService))]
    public class DefaultViewLocationService : IViewLocationService
    {
        public object LocateForViewModel(IViewModel viewModel)
        {
            return null;
        }
    }
}