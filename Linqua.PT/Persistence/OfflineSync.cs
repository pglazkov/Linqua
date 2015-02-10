using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Windows.Networking.Connectivity;
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
		private const string SqLiteDatabaseFileName = "localstore.db";

		public static async Task InitializeAsync([NotNull] IMobileServiceSyncHandler syncHandler)
		{
			if (!MobileService.Client.SyncContext.IsInitialized)
			{
				var store = new MobileServiceSQLiteStore(SqLiteDatabaseFileName);
				store.DefineTable<ClientEntry>();
				await MobileService.Client.SyncContext.InitializeAsync(store, syncHandler);
			}
		}

		public static async Task DoInitialPullIfNeededAsync(Expression<Func<ClientEntry, bool>> query = null)
		{
			var firstEntry = await MobileService.Client.GetSyncTable<ClientEntry>().Take(1).ToListAsync();

			if (firstEntry == null)
			{
				await TrySyncAsync(query);
			}
		}

		public static async Task SyncAsync(Expression<Func<ClientEntry, bool>> query = null)
		{
			await MobileService.Client.SyncContext.PushAsync();

			var entryTable = MobileService.Client.GetSyncTable<ClientEntry>();

			var mobileServiceTableQuery = entryTable.CreateQuery();

			if (query != null)
			{
				mobileServiceTableQuery = mobileServiceTableQuery.Where(query);
			}

			await entryTable.PullAsync("entryItems", mobileServiceTableQuery);
		}

		public static async Task EnqueueSync(Expression<Func<ClientEntry, bool>> query = null)
		{
			if (!ConnectionHelper.IsConnectedToInternet)
			{
				return;
			}

			await Task.Run(async () =>
			{
				await TrySyncAsync(query);
			});
		}

		public static async Task<bool> TrySyncAsync(Expression<Func<ClientEntry, bool>> query = null)
		{
			try
			{
				await SyncAsync(query);
				return true;
			}
			catch (Exception)
			{
				return false;
			}
		}
	}
}