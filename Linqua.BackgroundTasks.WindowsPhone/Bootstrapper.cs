using Linqua.Logging;

namespace Linqua
{
	internal static class Bootstrapper
	{
		public static void Run()
		{
			LoggerHelper.SetupLogger();
		}
	}
}