using Microsoft.WindowsAzure.MobileServices;

namespace Linqua
{
	public static class MobileService
	{
		//public static readonly MobileServiceClient Client = new MobileServiceClient("http://localhost:59988");
		//public static readonly MobileServiceClient Client = new MobileServiceClient("https://localhost:44300");

		// Use this constructor instead after publishing to the cloud
		public static readonly MobileServiceClient Client = new MobileServiceClient("https://linqua.azure-mobile.net/", "veBcEvMWjGNePbAKosRSIQzJGiTrfc50"
	   );
	}
}