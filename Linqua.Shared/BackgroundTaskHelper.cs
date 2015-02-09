using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Linqua.BackgroundTasks;

namespace Linqua
{
    public static class BackgroundTaskHelper
    {
		private const string SyncTaskName = "OfflineSync";

		public static async Task<BackgroundTaskRegistration> RegisterSyncTask()
		{
			var hasAccess = await BackgroundExecutionManager.RequestAccessAsync();
			if (hasAccess == BackgroundAccessStatus.Denied) return null;

			var builder = new BackgroundTaskBuilder
			{
				Name = SyncTaskName,
				TaskEntryPoint = typeof(SynchronizationTask).FullName
			};

			builder.SetTrigger(new SystemTrigger(SystemTriggerType.InternetAvailable, false));
			builder.AddCondition(new SystemCondition(SystemConditionType.InternetAvailable));

			var task = builder.Register();

			return task;
		}
    }
}
