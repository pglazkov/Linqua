using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
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

        private const int MaxSyncRetryCount = 3;

        private static readonly AsyncLock SyncLock = new AsyncLock();
        private static readonly Queue<SyncAction> SyncQueue = new Queue<SyncAction>();
        private static readonly TimeSpan SyncQueueProcessInterval = TimeSpan.FromSeconds(30);
        private static readonly ObservableSyncEvent SyncCompletedEvent;
        private static readonly AsyncLock InitializeLock = new AsyncLock();
        private static bool isInitialized;

        static OfflineHelper()
        {
            SyncCompletedEvent = new ObservableSyncEvent("Sync Completed Event");
            SyncCompletedEvent.Publish();
        }

        #region Nested Types

        #endregion

        public static async Task InitializeAsync([NotNull] IMobileServiceSyncHandler syncHandler)
        {
            using (await InitializeLock.LockAsync())
            {
                if (isInitialized)
                {
                    return;
                }

                if (!MobileService.Client.SyncContext.IsInitialized)
                {
                    var store = new MobileServiceSQLiteStore(SqLiteDatabaseFileName);
                    store.DefineTable<ClientEntry>();
                    await MobileService.Client.SyncContext.InitializeAsync(store, syncHandler);
                }

                SetupSyncQueueMonitoring();

                isInitialized = true;
            }
        }

        private static void SetupSyncQueueMonitoring()
        {
            Observable.Timer(SyncQueueProcessInterval, SyncQueueProcessInterval)
                      .ObserveOn(TaskPoolScheduler.Default)
                      .Subscribe(b => ProcessSyncQueueAsync().FireAndForget());
        }

        private static void CheckInitialized()
        {
            if (!MobileService.Client.SyncContext.IsInitialized)
            {
                throw new NotSupportedException("Sync context hasn't been initialized yet. Please call InitializeAsync first.");
            }
        }

        private static AwaitableDisposable<IDisposable> AcquireSyncLock(CancellationToken? cancellationToken = null)
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

        public static ISyncHandle EnqueueSync(OfflineSyncArguments args = null)
        {
            CheckInitialized();

            if (!ConnectionHelper.IsConnectedToInternet)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("EnqueueSync. Not connected to the internet. Do nothing.");
                }

                return new NullSyncHandle();
            }

            var tcs = new TaskCompletionSource();

            var action = new SyncAction(args ?? OfflineSyncArguments.Default, tcs);

            lock (SyncQueue)
            {
                SyncQueue.Enqueue(action);
            }

            return action;
        }

        private static async Task ProcessSyncQueueAsync()
        {
            var actions = new List<SyncAction>();

            lock (SyncQueue)
            {
                while (SyncQueue.Count > 0)
                {
                    actions.Add(SyncQueue.Dequeue());
                }
            }

            if (actions.Count == 0)
            {
                return;
            }

            var groupsByArguments = actions.GroupBy(x => x.Arguments).ToList();

            foreach (var actionsGroup in groupsByArguments)
            {
                Exception exception = null;

                var success = false;

                try
                {
                    success = await TrySyncAsync(actionsGroup.Key);
                }
                catch (Exception ex)
                {
                    exception = ex;
                }

                foreach (var syncAction in actionsGroup)
                {
                    syncAction.CurrentTryCount += 1;

                    if (success)
                    {
                        syncAction.CompletionSource.TrySetResult();
                    }
                    else
                    {
                        if (syncAction.CurrentTryCount < MaxSyncRetryCount)
                        {
                            // Put the action back into the queue to be retried later.
                            lock (SyncQueue)
                            {
                                SyncQueue.Enqueue(syncAction);
                            }
                        }
                        else
                        {
                            if (exception != null)
                            {
                                syncAction.CompletionSource.TrySetException(exception);
                            }
                            else
                            {
                                syncAction.CompletionSource.TrySetCanceled();
                            }
                        }
                    }
                }
            }
        }

        public static async Task<bool> TrySyncAsync(OfflineSyncArguments args = null)
        {
            CheckInitialized();

            if (!ConnectionHelper.IsConnectedToInternet)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("EnqueueSync. Not connected to the internet. Do nothing.");
                }

                return false;
            }

            if (MobileService.Client.CurrentUser == null)
            {
                if (Log.IsDebugEnabled)
                    Log.Debug("Cannot sync. User is not authenticated.");

                return false;
            }

            using (await AcquireSyncLock())
            {
                var sw = new Stopwatch();

                if (!SyncCompletedEvent.CanPublish)
                {
                    SyncCompletedEvent.Reset();
                }

                args = args ?? OfflineSyncArguments.Default;

                try
                {
                    if (Log.IsDebugEnabled)
                        Log.Debug("Sync Started.");

                    Telemetry.Client.TrackTrace("Sync Started.");

                    await MobileService.Client.SyncContext.PushAsync();

                    var entryTable = MobileService.Client.GetSyncTable<ClientEntry>();

                    var mobileServiceTableQuery = entryTable.CreateQuery();

                    if (args.Query != null)
                    {
                        mobileServiceTableQuery = mobileServiceTableQuery.Where(args.Query.Expression);
                    }

                    var queryId = "entryItems" + (args.Query?.Id ?? string.Empty);

                    if (args.PurgeCache)
                    {
                        Log.Debug("Purging local store.");

                        await entryTable.PurgeAsync(queryId, mobileServiceTableQuery, CancellationToken.None);
                    }

                    await entryTable.PullAsync(queryId, mobileServiceTableQuery);

                    return true;
                }
                catch (MobileServicePushFailedException ex)
                {
                    if (Log.IsWarnEnabled)
                    {
                        Log.Warn("Push Failed. Status: {0}. Errors: {1}", ex.PushResult.Status, ex.PushResult.Errors.Count > 0 ? string.Join("; ", ex.PushResult.Errors) : "<none>");
                    }

                    ExceptionHandlingHelper.HandleNonFatalError(ex, "Push failed.");

                    return false;
                }
                catch (Exception ex)
                {
                    ExceptionHandlingHelper.HandleNonFatalError(ex, "Synchronization failed.");

                    return false;
                }
                finally
                {
                    sw.Stop();

                    if (Log.IsDebugEnabled)
                        Log.Debug("Sync completed. Elapsed: " + sw.Elapsed);

                    Telemetry.Client.TrackTrace("Sync Completed. Elapsed: " + sw.Elapsed);

                    SyncCompletedEvent.Publish();
                }
            }
        }

        public static async Task AwaitPendingSync()
        {
            await SyncCompletedEvent;

            await ProcessSyncQueueAsync();
        }
    }
}