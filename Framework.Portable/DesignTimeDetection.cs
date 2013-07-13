using Framework.PlatformServices;

namespace Framework
{
	public static class DesignTimeDetection
	{
		public static bool IsInDesignTool
		{
			get { return PlatformService.Platform.CreateService<IDesignModeDetectionService>().GetIsInDesignTool(); }
		}
	}
}