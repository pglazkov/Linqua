using System;
using System.Composition;
using System.Reactive.Disposables;
#if WINDOWS_UWP
using Windows.Foundation.Metadata;
#endif
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Framework;
using Framework.PlatformServices;
using JetBrains.Annotations;

namespace Linqua.Framework
{
    [Export(typeof(IStatusBusyService))]
    [Shared]
    public class StatusBusyService : IStatusBusyService
    {
        private static readonly TimeSpan DefaultDisplayAfter = TimeSpan.FromSeconds(0.5);

        private readonly DispatcherTimer displayAfterTimer = new DispatcherTimer();

        private readonly IStringResourceManager stringResourceManager;

        private BusyStatus currentBusyStatus;

        [ImportingConstructor]
        public StatusBusyService([NotNull] IStringResourceManager stringResourceManager)
        {
            Guard.NotNull(stringResourceManager, nameof(stringResourceManager));

            this.stringResourceManager = stringResourceManager;

            displayAfterTimer.Interval = DefaultDisplayAfter;
            displayAfterTimer.Tick += OnDisplayAfterTimerTick;
        }

        public IDisposable Busy(CommonBusyType type)
        {
            var statusText = CreateStatusTextFromType(type);

            return Busy(statusText);
        }

        private string CreateStatusTextFromType(CommonBusyType type)
        {
            switch (type)
            {
                case CommonBusyType.Loading:
                    return stringResourceManager.GetString("StatusBusyService_StatusText_Loading");
                case CommonBusyType.Saving:
                    return stringResourceManager.GetString("StatusBusyService_StatusText_Saving");
                case CommonBusyType.Deleting:
                    return stringResourceManager.GetString("StatusBusyService_StatusText_Deleting");
                case CommonBusyType.Syncing:
                    return stringResourceManager.GetString("StatusBusyService_StatusText_Syncing");
                case CommonBusyType.GenericLongRunningTask:
                    return stringResourceManager.GetString("StatusBusyService_StatusText_GenericLongRunningTask");
                default:
                    throw new NotImplementedException($"Handling of \"{type}\" is not implemented.");
            }
        }

        public IDisposable Busy(string statusText)
        {
            displayAfterTimer.Stop();

            currentBusyStatus = new BusyStatus(statusText ?? CreateStatusTextFromType(CommonBusyType.Loading));

            displayAfterTimer.Start();

            return Disposable.Create(() => { HideStatus(currentBusyStatus); });
        }

        private void OnDisplayAfterTimerTick(object sender, object e)
        {
            displayAfterTimer.Stop();

            var s = currentBusyStatus;

            if (s != null)
            {
                ShowStatus(s);
            }
        }

        private void ShowStatus(BusyStatus status)
        {
            if (IsStatusBarFeaturePresent())
            {
                var progressbar = StatusBar.GetForCurrentView().ProgressIndicator;
                progressbar.Text = status.StatusText;
                var _ = progressbar.ShowAsync();
            }
        }

        private void HideStatus(BusyStatus status)
        {
            displayAfterTimer.Stop();

            currentBusyStatus = null;

            if (IsStatusBarFeaturePresent())
            {
                var progressbar = StatusBar.GetForCurrentView().ProgressIndicator;

                var _ = progressbar.HideAsync();
            }
        }

        private static bool IsStatusBarFeaturePresent()
        {
            bool isStatusBarPresent = true;

#if WINDOWS_UWP
            isStatusBarPresent = ApiInformation.IsTypePresent("Windows.UI.ViewManagement.StatusBar");
#endif
            return isStatusBarPresent;
        }
    }
}