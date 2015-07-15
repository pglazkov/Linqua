using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using Framework.NotificationExtensions;
using Framework.NotificationExtensions.TileContent;
using Linqua.Persistence;
using MetroLog;

namespace Linqua.BackgroundTasks
{
	public sealed class LiveTileUpdateTask : IBackgroundTask
	{
		private static readonly ILogger Log;
		  
		static LiveTileUpdateTask()
		{
			LoggerHelper.SetupLogger();
			Log = LogManagerFactory.DefaultLogManager.GetLogger<LiveTileUpdateTask>();
		}

		public async void Run(IBackgroundTaskInstance taskInstance)
		{
			if (Log.IsDebugEnabled)
				Log.Debug("Live tile update background task starting");

			var deferral = taskInstance.GetDeferral();

			try
			{
				var authenticatedSilently = await SecurityManager.TryAuthenticateSilently();

				if (authenticatedSilently)
				{
					var dataTile = await CreateDataTileAsync();

					if (dataTile != null)
					{
						var appLogoTile = CreateAppLogoTile();

						TileUpdateManager.CreateTileUpdaterForApplication().EnableNotificationQueue(true);
						TileUpdateManager.CreateTileUpdaterForApplication().Clear();
						TileUpdateManager.CreateTileUpdaterForApplication().Update(appLogoTile);
						TileUpdateManager.CreateTileUpdaterForApplication().Update(dataTile);
					}
					else
					{
						if (Log.IsDebugEnabled)
							Log.Debug("There is no data for the tile update.");
					}
				}
				else
				{
					if (Log.IsWarnEnabled)
						Log.Warn("Authentication failed.");
				}

				if (Log.IsDebugEnabled)
					Log.Debug("Live tile update background task completed");
			}
			catch (Exception ex)
			{
				if (Log.IsErrorEnabled)
					Log.Error("An error occured while trying to update the live tile.", ex);

				throw;
			}
			finally
			{
				deferral.Complete();
			}
		}

		private static async Task<TileNotification> CreateDataTileAsync()
		{
			IDataStore storage = new MobileServiceDataStore(new SyncHandler());
			await storage.InitializeAsync(doInitialPoolIfNeeded: false);

			var randomEntry = await storage.GetRandomEntry();

			if (randomEntry != null)
			{
				var tileHeading = randomEntry.Text;
				var tileText = randomEntry.Definition;

				ITileWide310x150Text01 textWide310X150 = TileContentFactory.CreateTileWide310x150Text01();
				textWide310X150.TextHeading.Text = tileHeading;
				textWide310X150.TextBody1.Text = tileText;

				ITileSquare150x150Text01 textSquare150X150 = TileContentFactory.CreateTileSquare150x150Text01();
				textSquare150X150.TextHeading.Text = tileHeading;
				textSquare150X150.TextBody1.Text = tileText;

				textWide310X150.Square150x150Content = textSquare150X150;

				var textTileNotification = textWide310X150.CreateNotification();

				return textTileNotification;
			}

			return null;
		}

		private static TileNotification CreateAppLogoTile()
		{
			var imageSquare150X150 = TileContentFactory.CreateTileSquare150x150Image();
			imageSquare150X150.Image.Src = "ms-appx:///Assets/Logo.png";
			imageSquare150X150.Branding = TileBranding.None;

			var imageWide310X150 = TileContentFactory.CreateTileWide310x150Image();
			imageWide310X150.Image.Src = "ms-appx:///Assets/WideLogo.png";
			imageWide310X150.Branding = TileBranding.None;

			imageWide310X150.Square150x150Content = imageSquare150X150;
			

			return imageWide310X150.CreateNotification();
		}
	}
}