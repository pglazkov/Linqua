using System;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using Framework.NotificationExtensions;
using Framework.NotificationExtensions.TileContent;
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

		public void Run(IBackgroundTaskInstance taskInstance)
		{
			var tileTextContent = "Hello World! Current time is: " + DateTime.Now.ToString("T");

			ITileSquare310x310Text09 tileContent = TileContentFactory.CreateTileSquare310x310Text09();
			tileContent.TextHeadingWrap.Text = tileTextContent;

			// Create a notification for the Wide310x150 tile using one of the available templates for the size.
			ITileWide310x150Text03 wide310x150Content = TileContentFactory.CreateTileWide310x150Text03();
			wide310x150Content.TextHeadingWrap.Text = tileTextContent;

			// Create a notification for the Square150x150 tile using one of the available templates for the size.
			ITileSquare150x150Text04 square150x150Content = TileContentFactory.CreateTileSquare150x150Text04();
			square150x150Content.TextBodyWrap.Text = tileTextContent;

			// Attach the Square150x150 template to the Wide310x150 template.
			wide310x150Content.Square150x150Content = square150x150Content;

			// Attach the Wide310x150 template to the Square310x310 template.
			tileContent.Wide310x150Content = wide310x150Content;

			// Send the notification to the application? tile.
			var tileNotification = tileContent.CreateNotification();
			TileUpdateManager.CreateTileUpdaterForApplication().Update(tileNotification);
		}
	}
}