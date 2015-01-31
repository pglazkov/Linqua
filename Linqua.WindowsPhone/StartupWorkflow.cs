using System;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Framework;
using JetBrains.Annotations;

namespace Linqua
{
	public class StartupWorkflow
	{
		private readonly Frame frame;
		private readonly string arguments;

		public StartupWorkflow([NotNull] Frame frame, string arguments)
		{
			Guard.NotNull(frame, () => frame);

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

				await SecurityManager.Authenticate();
			}

			if (!frame.Navigate(typeof(MainPage), arguments))
			{
				throw new Exception("Failed to create initial page");
			}
		}
	}
}