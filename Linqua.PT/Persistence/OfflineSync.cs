using System;
using System.Threading.Tasks;
using Framework;
using JetBrains.Annotations;
using Linqua.DataObjects;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;

namespace Linqua.Persistence
{
	public static class OfflineSync
	{
		public static async Task InitializeAsync([NotNull] IMobileServiceSyncHandler syncHandler)
		{
			if (!MobileService.Client.SyncContext.IsInitialized)
			{
				var store = new MobileServiceSQLiteStore("localstore.db");
				store.DefineTable<ClientEntry>();
				await MobileService.Client.SyncContext.InitializeAsync(store, syncHandler);
			}
		}

		public static async Task SyncAsync()
		{
			await MobileService.Client.SyncContext.PushAsync();

			var entryTable = MobileService.Client.GetSyncTable<ClientEntry>();

			await entryTable.PullAsync("entryItems", entryTable.CreateQuery());
		}
	}
}