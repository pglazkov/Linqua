using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using JetBrains.Annotations;
using Linqua.DataObjects;
using MetroLog;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Nito.AsyncEx;

namespace Linqua.Persistence
{
	public static class OfflineHelper
	{
		private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger(typeof(OfflineHelper).Name);

		private const string SqLiteDatabaseFileName = "localstore.db";

		private static readonly AsyncLock SyncLock = new AsyncLock();

		public static async Task InitializeAsync([NotNull] IMobileServiceSyncHandler syncHandler)
		{
			if (!MobileService.Client.SyncContext.IsInitialized)
			{
				var store = new MobileServiceSQLiteStore(SqLiteDatabaseFileName);
				store.DefineTable<ClientEntry>();
				await MobileService.Client.SyncContext.InitializeAsync(store, syncHandler);
			}
		}

		private static void CheckInitialized()
		{
			if (!MobileService.Client.SyncContext.IsInitialized)
			{
				throw new NotSupportedException("Sync context hasn't been initialized yet. Please call InitializeAsync first.");
			}
		}

		public static AwaitableDisposable<IDisposable> AcquireDataAccessLockAsync()
		{
			return SyncLock.LockAsync();
		}

		public static async Task DoInitialPullIfNeededAsync(OfflineSyncArguments args = null)
		{
			CheckInitialized();

			var firstEntry = (await MobileService.Client.GetSyncTable<ClientEntry>().Take(1).ToListAsync()).SingleOrDefault();

			if (firstEntry == null)
			{
				await TrySyncAsync(args);
			}
		}

		public static async Task EnqueueSync(OfflineSyncArguments args = null)
		{
			CheckInitialized();

			if (!ConnectionHelper.IsConnectedToInternet)
			{
				if (Log.IsDebugEnabled)
				{
					Log.Debug("EnqueueSync. Not connected to the internet. Do nothing.");
				}

				return;
			}

			await Task.Run(async () =>
			{
				await TrySyncAsync(args);
			});
		}

		public static async Task<bool> TrySyncAsync(OfflineSyncArguments args = null)
		{
			CheckInitialized();

			using (await AcquireDataAccessLockAsync())
			{
				args = args ?? OfflineSyncArguments.Default;

				try
				{
					if (Log.IsDebugEnabled)
						Log.Debug("Sync Started.");
					
					await MobileService.Client.SyncContext.PushAsync();

					IMobileServiceSyncTable<ClientEntry> entryTable = MobileService.Client.GetSyncTable<ClientEntry>();
					
					IMobileServiceTableQuery<ClientEntry> mobileServiceTableQuery = entryTable.CreateQuery();

					if (args.Query != null)
					{
						mobileServiceTableQuery = mobileServiceTableQuery.Where(args.Query.Expression);
					}

					string queryId = "entryItems" + (args.Query != null ? args.Query.Id : string.Empty);

					if (args.PurgeCache)
					{
						Log.Debug("Purging local store.");

						await entryTable.PurgeAsync(queryId, mobileServiceTableQuery, CancellationToken.None);
					}

					await entryTable.PullAsync(queryId, mobileServiceTableQuery);

					if (Log.IsDebugEnabled)
						Log.Debug("Sync completed.");

					return true;
				}
				catch (Exception ex)
				{
					if (Log.IsErrorEnabled)
					{
						Log.Error("Synchronization failed.", ex);
					}

					return false;
				}
			}
		}
	}
}