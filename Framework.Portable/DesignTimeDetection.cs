using Framework.PlatformServices;

namespace Framework
{
	public static class DesignTimeDetection
	{
		public static bool IsInDesignTool
		{
			get
			{
			    if (CompositionManager.IsCurrentAvailable)
			    {
			        return CompositionManager.Current.GetInstance<IDesignModeDetectionService>().GetIsInDesignTool();
			    }

                // CompositionManager instance is not available at the design time
			    return true;
			}
		}
	}
}