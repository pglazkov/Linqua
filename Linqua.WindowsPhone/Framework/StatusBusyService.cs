using System;
using System.Composition;
using System.Reactive.Disposables;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Framework.PlatformServices;

namespace Linqua.Framework
{
	[Export(typeof(IStatusBusyService))]
	public class StatusBusyService : IStatusBusyService
	{
		private static readonly TimeSpan DefaultDisplayAfter = TimeSpan.FromSeconds(0.5);

		private readonly DispatcherTimer displayAfterTimer = new DispatcherTimer();

		private BusyStatus currentBusyStatus;

		public StatusBusyService()
		{
			displayAfterTimer.Interval = DefaultDisplayAfter;
			displayAfterTimer.Tick += OnDisplayAfterTimerTick;
		}

		public IDisposable Busy(string statusText)
		{
			displayAfterTimer.Stop();

			currentBusyStatus = new BusyStatus(statusText ?? "Loading...");

			displayAfterTimer.Start();

			return Disposable.Create(() =>
			{
				HideStatus(currentBusyStatus);
			});
		}

		private void OnDisplayAfterTimerTick(object sender, object e)
		{
			var s = currentBusyStatus;

			if (s != null)
			{
				ShowStatus(s);
			}
		}

		private void ShowStatus(BusyStatus status)
		{
			var progressbar = StatusBar.GetForCurrentView().ProgressIndicator;
			progressbar.Text = status.StatusText;
			var _ = progressbar.ShowAsync();
		}

		private void HideStatus(BusyStatus status)
		{
			displayAfterTimer.Stop();

			currentBusyStatus = null;

			var progressbar = StatusBar.GetForCurrentView().ProgressIndicator;

			var _ = progressbar.HideAsync();
		}
	}
}