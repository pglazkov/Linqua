using System;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;
using Framework;
using Framework.PlatformServices;
using JetBrains.Annotations;
using Linqua.Offline;
using Linqua.UI;

namespace Linqua
{
	public class StartupWorkflow
	{
		private readonly Frame frame;
		private readonly string arguments;

		public StartupWorkflow([NotNull] Frame frame, string arguments)
		{
			Guard.NotNull(frame, nameof(frame));

			this.frame = frame;
			this.arguments = arguments;
		}

		public async Task RunAsync()
		{
			var authenticatedSilently = await SecurityManager.TryAuthenticateSilently();

			if (!authenticatedSilently)
			{
				if (!frame.Navigate(typeof(LoginPage), arguments))
				{
					throw new Exception("Failed to create login page");
				}

				bool authenticated = false;

				while (!authenticated)
				{
					authenticated = await SecurityManager.Authenticate();

					if (!authenticated)
					{
					    var resourceManager = CompositionManager.Current.GetInstance<IStringResourceManager>();

					    var messageTitle = resourceManager.GetString("LoginRequiredMessageTitle");
					    var messageText = resourceManager.GetString("LoginRequiredMessageText");

						var dialog = new MessageDialog(messageText, messageTitle);
						dialog.Commands.Add(new UICommand("OK"));
						await dialog.ShowAsync();
					}
				}
			}

			if (!frame.Navigate(typeof(MainPage), arguments))
			{
				throw new Exception("Failed to create initial page");
			}
		}
	}
}