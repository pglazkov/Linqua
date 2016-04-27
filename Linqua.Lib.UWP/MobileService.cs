﻿using Microsoft.WindowsAzure.MobileServices;

namespace Linqua
{
    public static class MobileService
    {
        //public static readonly MobileServiceClient Client = new MobileServiceClient("http://localhost:59988");
        //public static readonly MobileServiceClient Client = new MobileServiceClient("https://localhost:44300");

        // STAGING
        //public const string MobileServiceUrl = "https://linqua-test.azure-mobile.net/";
        //public const string ApplicationKey = "";

        // PRODUCTION
        public const string MobileServiceUrl = "http://linqua-v2.azurewebsites.net";

        //public const string MobileServiceUrl = "https://linqua-v2.azure-mobile.net/";
        //public const string ApplicationKey = "XLqilnKcdfbcakoxdembcrBZYAsHGK19";

        // Use this constructor instead after publishing to the cloud
        public static readonly MobileServiceClient Client = new MobileServiceClient(MobileServiceUrl, new AuthFailureHandler());
    }
}