using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Framework;
using MetroLog;

namespace Linqua
{
	public static class BackgroundTaskHelper
	{
		private const string SyncTaskName = "LinquaOfflineSync";
		private const string LiveTileUpdateTaskName = "LinquaLiveTileUpdate";

		private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger(typeof(BackgroundTaskHelper));

		private static BackgroundAccessStatus? AccessStatus { get; set; }
		private static BackgroundTaskRegistration SyncTask { get; set; }
		private static BackgroundTaskRegistration LiveTileUpdateTask { get; set; }

		public static async Task<BackgroundTaskRegistration> RegisterSyncTask()
		{
			if (SyncTask != null)
			{
				return SyncTask;
			}

			if (Log.IsDebugEnabled)
				Log.Debug("Registering SyncTask background task.");

			var accessStatus = await SetUpAccess();
			if (accessStatus == BackgroundAccessStatus.Denied) return null;

			var result = RegisterBackgroundTask(typeof(SynchronizationTask).FullName,
			                                    SyncTaskName,
			                                    new SystemTrigger(SystemTriggerType.InternetAvailable, false),
			                                    new SystemCondition(SystemConditionType.InternetAvailable));
			SyncTask = result;

			if (Log.IsDebugEnabled)
				Log.Debug("Background task registered. TaskId: {0}", result.TaskId);

			return result;
		}

		public static async Task<BackgroundTaskRegistration> RegisterLiveTileUpdateTask()
		{
			if (LiveTileUpdateTask != null)
			{
				return LiveTileUpdateTask;
			}

			if (Log.IsDebugEnabled)
				Log.Debug("Registering LiveTileUpdateTask background task.");

			var accessStatus = await SetUpAccess();
			if (accessStatus == BackgroundAccessStatus.Denied) return null;

			var result = RegisterBackgroundTask(typeof(LiveTileUpdateTask).FullName,
												LiveTileUpdateTaskName,
												new TimeTrigger(15, false), 
												new SystemCondition(SystemConditionType.SessionConnected));
			LiveTileUpdateTask = result;

			if (Log.IsDebugEnabled)
				Log.Debug("Background task registered. TaskId: {0}", result.TaskId);

			return result;
		}
		
		public static BackgroundTaskRegistration RegisterBackgroundTask(string taskEntryPoint, string taskName, IBackgroundTrigger trigger, IBackgroundCondition condition)
		{
			// Check for existing registrations of this background task.

			foreach (var cur in BackgroundTaskRegistration.AllTasks)
			{

				if (cur.Value.Name == taskName)
				{
					// The task is already registered.
			
					return (BackgroundTaskRegistration)(cur.Value);
				}
			}

			// Register the background task.

			var builder = new BackgroundTaskBuilder();

			builder.Name = taskName;
			builder.TaskEntryPoint = taskEntryPoint;
			builder.SetTrigger(trigger);

			if (condition != null)
			{
				builder.AddCondition(condition);
			}

			BackgroundTaskRegistration task = builder.Register();

			return task;
		}

		public static async Task<BackgroundAccessStatus> SetUpAccess()
		{
			if (AccessStatus == null)
			{
				string appVersion = $"{Package.Current.Id.Version.Build}.{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Revision}";

				if (!Equals(Windows.Storage.ApplicationData.Current.LocalSettings.Values["AppVersion"], appVersion))
				{
					// Our app has been updated
					Windows.Storage.ApplicationData.Current.LocalSettings.Values["AppVersion"] = appVersion;

					// Call RemoveAccess
					BackgroundExecutionManager.RemoveAccess();
				}

				AccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
			}

			Guard.Assert(AccessStatus != null, "AccessStatus != null");

			return AccessStatus.Value;
		}
	}
}
