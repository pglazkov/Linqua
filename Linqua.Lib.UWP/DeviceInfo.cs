﻿using System;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Security.ExchangeActiveSyncProvisioning;
using Windows.System.Profile;

namespace Linqua
{
    public static class DeviceInfo
    {
        private static readonly EasClientDeviceInformation Info = new EasClientDeviceInformation();
        private static readonly Lazy<string> DeviceIdLazy = new Lazy<string>(GetDeviceId);

        public static string DeviceName => IsRunningOnEmulator ? "Emulator" : Info.FriendlyName;

        public static string DeviceId => DeviceIdLazy.Value;

        public static bool IsRunningOnEmulator => Info.SystemProductName == "Virtual";

        private static string GetDeviceId()
        {
            if (!Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.System.Profile.HardwareIdentification"))
            {
                throw new NotSupportedException("Gettting Device ID is not supported on this platform (Windows.System.Profile.HardwareIdentification type is not present).");
            }

            // See: http://stackoverflow.com/questions/23321484/device-unique-id-in-windows-phone-8-1

            var token = HardwareIdentification.GetPackageSpecificToken(null);
            var hardwareId = token.Id;

            var hasher = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);
            var hashed = hasher.HashData(hardwareId);

            var hashedString = CryptographicBuffer.EncodeToHexString(hashed);

            return hashedString;
        }
    }
}