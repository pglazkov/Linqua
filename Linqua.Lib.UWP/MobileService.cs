using System;
using Microsoft.WindowsAzure.MobileServices;

namespace Linqua
{
    public static class MobileService
    {
        public const string MobileServiceUrl = "https://linqua-v2.azurewebsites.net";
        public static readonly MobileServiceClient Client;

        static MobileService()
        {
#if LOCAL_SERVICE
            Client = new MobileServiceClient("http://localhost:59988/", new LegacyUserIdHandler(), new AuthFailureHandler());
            Client.AlternateLoginHost = new Uri(MobileServiceUrl);
#else
            Client = new MobileServiceClient(MobileServiceUrl, new AuthFailureHandler());
#endif
        }
    }
}