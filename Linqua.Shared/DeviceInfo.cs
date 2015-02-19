using Windows.Security.ExchangeActiveSyncProvisioning;

namespace Linqua
{
	public static class DeviceInfo
	{
		private static readonly EasClientDeviceInformation Info = new EasClientDeviceInformation();

		public static string DeviceId
		{
			get { return IsRunningOnEmulator ? "Emulator" : Info.FriendlyName; }
		}

		public static bool IsRunningOnEmulator
		{
			get
			{
				return (Info.SystemProductName == "Virtual");
			}
		}
	}
}
