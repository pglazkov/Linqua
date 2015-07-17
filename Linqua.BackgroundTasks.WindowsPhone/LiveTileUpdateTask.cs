using System;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using Linqua.Persistence;
using MetroLog;
using NotificationsExtensions.TileContent;

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

				var wideTile = TileContentFactory.CreateTileWide310x150PeekImage01();
				wideTile.Image.Src = "ms-appx:///Assets/WideLogo.png";
				wideTile.TextHeading.Text = tileHeading;
				wideTile.TextBodyWrap.Text = tileText;

				var squareTile = TileContentFactory.CreateTileSquare150x150PeekImageAndText01();
				squareTile.Image.Src = "ms-appx:///Assets/Logo.png";
				squareTile.TextHeading.Text = tileHeading;
				squareTile.TextBody1.Text = tileText;

				wideTile.Square150x150Content = squareTile;

				var notification = wideTile.CreateNotification();

				return notification;
			}

			return null;
		}
	}
}