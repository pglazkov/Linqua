using System;
using System.Composition;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace Framework.PlatformServices
{
	[Export(typeof(IDispatcherService))]
	[Shared]
	public class DispatcherService : IDispatcherService
	{
		private CoreDispatcher Dispatcher
		{
			get { return CoreApplication.MainView.CoreWindow.Dispatcher; }
		}

		public bool CheckAccess()
		{
			return Dispatcher.HasThreadAccess;
		}

		public async void BeginInvoke(Delegate method, params Object[] args)
		{
			await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => method.DynamicInvoke(args));
		}
	}
}