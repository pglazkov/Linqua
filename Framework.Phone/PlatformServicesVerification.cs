using Framework.PlatformServices;

namespace Framework
{
	public class PlatformServicesVerification
	{
		public static void EnsureLoaded()
		{
			if (!(PlatformService.Platform is PlatformServiceProvider))
			{
				PlatformService.Platform = new PlatformServiceProvider();
			}
		} 
	}
}