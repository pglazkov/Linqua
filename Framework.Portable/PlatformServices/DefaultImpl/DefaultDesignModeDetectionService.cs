namespace Framework.PlatformServices.DefaultImpl
{
	public class DefaultDesignModeDetectionService : IDesignModeDetectionService
	{
		public bool GetIsInDesignTool()
		{
			return false;
		}
	}
}