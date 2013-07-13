using System.ComponentModel;

namespace Framework.PlatformServices
{
	public class DesignModeDetectionService : IDesignModeDetectionService
	{
		public bool GetIsInDesignTool()
		{
			return DesignerProperties.IsInDesignTool;
		}
	}
}