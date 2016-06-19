namespace Linqua
{
    public static class LegacyUserId
    {
        public const string HeaderKey = "LINQUA-LEGACY-USER-ID";

#if WINDOWS_UWP
        private const string LocalSettingsKey = "LegacyUserID";

        public static string Value
        {
            get { return (string)Windows.Storage.ApplicationData.Current.LocalSettings.Values[LocalSettingsKey]; }
            set { Windows.Storage.ApplicationData.Current.LocalSettings.Values[LocalSettingsKey] = value; }
        }
#endif
    }
}