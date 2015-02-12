using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Background;
using Linqua.BackgroundTasks;

namespace Linqua
{
	public static class BackgroundTaskHelper
	{
		private const string SyncTaskName = "LinquaOfflineSync";

		public static async Task<BackgroundTaskRegistration> RegisterSyncTask()
		{
			var accessStatus = await SetUpAccess();
			if (accessStatus == BackgroundAccessStatus.Denied) return null;

			return RegisterBackgroundTask(typeof(SynchronizationTask).FullName,
			                              SyncTaskName,
			                              new SystemTrigger(SystemTriggerType.InternetAvailable, false),
			                              new SystemCondition(SystemConditionType.InternetAvailable));
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
			String appVersion = String.Format("{0}.{1}.{2}.{3}",
					Package.Current.Id.Version.Build,
					Package.Current.Id.Version.Major,
					Package.Current.Id.Version.Minor,
					Package.Current.Id.Version.Revision);

			if (!Equals(Windows.Storage.ApplicationData.Current.LocalSettings.Values["AppVersion"], appVersion))
			{
				// Our app has been updated
				Windows.Storage.ApplicationData.Current.LocalSettings.Values["AppVersion"] = appVersion;

				// Call RemoveAccess
				BackgroundExecutionManager.RemoveAccess();
			}

			BackgroundAccessStatus status = await BackgroundExecutionManager.RequestAccessAsync();

			return status;
		}
	}
}
