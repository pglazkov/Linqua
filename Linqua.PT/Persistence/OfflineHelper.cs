using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Framework;
using JetBrains.Annotations;
using Linqua.DataObjects;
using MetroLog;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Nito.AsyncEx;
using AsyncLock = Nito.AsyncEx.AsyncLock;

namespace Linqua.Persistence
{
	public static class OfflineHelper
	{
		private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger(typeof(OfflineHelper).Name);

		private const string SqLiteDatabaseFileName = "localstore.db";

		private static readonly AsyncLock SyncLock = new AsyncLock();
        private static readonly Subject<SyncAction> SyncQueue = new Subject<SyncAction>();

	    #region Nested Types

	    private class SyncAction
	    {
	        public SyncAction([NotNull] OfflineSyncArguments arguments, [NotNull] TaskCompletionSource completionSource)
	        {
	            Guard.NotNull(arguments, nameof(arguments));
	            Guard.NotNull(completionSource, nameof(completionSource));

	            CompletionSource = completionSource;
	            Arguments = arguments;
	        }

            public OfflineSyncArguments Arguments { get; }

            public TaskCompletionSource CompletionSource { get; }
	    }

	    #endregion

	    static OfflineHelper()
	    {
	        SyncQueue
	            .Buffer(TimeSpan.FromMilliseconds(1000))
	            .Where(x => x.Count > 0)
                .ObserveOn(TaskPoolScheduler.Default)
	            .Subscribe(b => ProcessSyncBatchAsync(b).FireAndForget());
	    }

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

		public static AwaitableDisposable<IDisposable> AcquireSyncLock(CancellationToken? cancellationToken = null)
		{
			return SyncLock.LockAsync(cancellationToken ?? CancellationToken.None);
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

		public static Task EnqueueSync(OfflineSyncArguments args = null)
		{
			CheckInitialized();

			if (!ConnectionHelper.IsConnectedToInternet)
			{
				if (Log.IsDebugEnabled)
				{
					Log.Debug("EnqueueSync. Not connected to the internet. Do nothing.");
				}

				return Task.FromResult(true);
			}

            var tcs = new TaskCompletionSource();

            var action = new SyncAction(args ?? OfflineSyncArguments.Default, tcs);

		    SyncQueue.OnNext(action);

		    return tcs.Task;
		}

        private static async Task ProcessSyncBatchAsync(IEnumerable<SyncAction> actions)
        {
            var groupsByArguments = actions.GroupBy(x => x.Arguments).ToList();

            foreach (var actionsGroup in groupsByArguments)
            {
                Exception exception = null;

                try
                {
                    await TrySyncAsync(actionsGroup.Key);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                foreach (var syncAction in actionsGroup)
                {
                    if (exception == null)
                    {
                        syncAction.CompletionSource.TrySetResult();
                    }
                    else
                    {
                        syncAction.CompletionSource.TrySetException(exception);
                    }
                }
            }
        }

        public static async Task<bool> TrySyncAsync(OfflineSyncArguments args = null)
		{
			CheckInitialized();

			using (await AcquireSyncLock())
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
				catch (MobileServicePushFailedException ex)
				{
					if (Log.IsErrorEnabled)
					{
						Log.Error("Push Failed. Status: {0}. Errors: {1}", ex.PushResult.Status, ex.PushResult.Errors.Count > 0 ? string.Join("; ", ex.PushResult.Errors) : "<none>");
					}

					return false;
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