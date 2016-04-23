using Framework.MefExtensions;

namespace Framework.PlatformServices.DefaultImpl
{
    [DefaultExport(typeof(IDesignModeDetectionService))]
    public class DefaultDesignModeDetectionService : IDesignModeDetectionService
    {
        public bool GetIsInDesignTool()
        {
            return false;
        }
    }
}