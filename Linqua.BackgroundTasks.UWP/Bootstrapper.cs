using System;
using Windows.ApplicationModel;
using Linqua.Logging;
using MetroLog;

namespace Linqua
{
    internal static class Bootstrapper
    {
        private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger(typeof(Bootstrapper));

        public static void Run(Type taskType)
        {
            LoggerHelper.SetupLogger();

            Log.Info(
                $"=== Background Task Created. Name: {taskType.Name}; DeviceId: {DeviceInfo.DeviceId}; DeviceName: {DeviceInfo.DeviceName}; App Version: {$"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Revision}"}");
        }
    }
}