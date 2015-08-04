﻿using System;
using Windows.ApplicationModel.Background;
using Framework;
using Linqua.Logging;
using Linqua.Notifications;
using Linqua.Persistence;
using MetroLog;

namespace Linqua
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
					IDataStore storage = new MobileServiceDataStore(new SyncHandler(), new EventManager());
					await storage.InitializeAsync(doInitialPoolIfNeeded: false);

					var liveTileManager = new LiveTileManager(storage);

					await liveTileManager.UpdateTileAsync();
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
	}
}