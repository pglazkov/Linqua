using System.ComponentModel;
using System.Composition;

namespace Framework.PlatformServices
{
    [Export(typeof(IDesignModeDetectionService))]
    public class DesignModeDetectionService : IDesignModeDetectionService
    {
        public bool GetIsInDesignTool()
        {
            return Windows.ApplicationModel.DesignMode.DesignModeEnabled;
        }
    }
}