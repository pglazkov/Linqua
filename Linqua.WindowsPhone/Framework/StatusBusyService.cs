using System;
using System.Composition;
using System.Reactive.Disposables;
using Windows.UI.ViewManagement;
using Framework.PlatformServices;

namespace Linqua.Framework
{
	[Export(typeof(IStatusBusyService))]
	public class StatusBusyService : IStatusBusyService
	{
		public IDisposable Busy(string statusText)
		{
			StatusBarProgressIndicator progressbar = StatusBar.GetForCurrentView().ProgressIndicator;
			progressbar.Text = statusText ?? "Loading";
			progressbar.ShowAsync();

			return Disposable.Create(() =>
			{
				progressbar.HideAsync();
			});
		}
	}
}