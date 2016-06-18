using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Framework;
using Framework.PlatformServices;
using JetBrains.Annotations;
using Linqua.DataObjects;
using Linqua.Persistence.Events;
using Linqua.Persistence.Exceptions;
using Linqua.Service.DataObjects;
using MetroLog;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Nito.AsyncEx;
using AsyncLock = Nito.AsyncEx.AsyncLock;

namespace Linqua.Persistence
{
    [Export(typeof(IBackendServiceClient))]
    [Shared]
    public class MobileServiceBackendServiceClient : IBackendServiceClient
    {
        private const string SqLiteDatabaseFileName = "localstore-v2.db";
        private const string InitialPullCompleteKey = SqLiteDatabaseFileName + "_INITIAL_PULL_COMPLETE";
        private const int MaxSyncRetryCount = 3;

        #region Static

        private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger(typeof(MobileServiceBackendServiceClient).Name);
        private static readonly AsyncLock SyncLock = new AsyncLock();
        private static readonly Queue<SyncAction> SyncQueue = new Queue<SyncAction>();
        private static readonly TimeSpan SyncQueueProcessInterval = TimeSpan.FromSeconds(30);
        private static readonly ObservableSyncEvent SyncCompletedEvent;
        private static readonly AsyncLock InitializeLock = new AsyncLock();
        private static bool isInitialized;

        static MobileServiceBackendServiceClient()
        {
            SyncCompletedEvent = new ObservableSyncEvent("Sync Completed Event");
            SyncCompletedEvent.Publish();
        }

        private static async Task<bool> DoTrySyncAsync(OfflineSyncArguments args = null, [CallerMemberName] string callingMemberName = null)
        {
            try
            {
                var argsCopy = args;

                var retryPolicy = new AsyncRetryPolicy(retryInterval: TimeSpan.FromSeconds(3), retryCount: 4,
                                                       exceptionFilter: ex => !(ex is NoInternetConnectionException) && !(ex is UserIsNotLoggedInException),
                                                       onExceptionAction: ex =>
                                                       {
                                                           Log.Warn($"Exception when executing \"{callingMemberName}\": {ex.Message}. Will retry.");
                                                       });

                await RetryHelper.DoAsync(() => SyncAsync(argsCopy), retryPolicy);

                return true;
            }
            catch (Exception ex)
            {
                ExceptionHandlingHelper.HandleNonFatalError(ex, "Synchronization failed.");

                return false;
            }
        }

        private static async Task SyncAsync(OfflineSyncArguments args = null)
        {
            CheckInitialized();

            if (!ConnectionHelper.IsConnectedToInternet)
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug("EnqueueSync. Not connected to the internet. Do nothing.");
                }

                throw new NoInternetConnectionException();
            }

            if (MobileService.Client.CurrentUser == null)
            {
                if (Log.IsDebugEnabled)
                    Log.Debug("Cannot sync. User is not authenticated.");

                throw new UserIsNotLoggedInException();
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

                    var entryTable = MobileService.Client.GetSyncTable<Entry>();

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

                    
                }
                catch (MobileServicePushFailedException ex)
                {
                    if (Log.IsWarnEnabled)
                    {
                        Log.Warn("Push Failed. Status: {0}. Errors: {1}", ex.PushResult.Status, ex.PushResult.Errors.Count > 0 ? string.Join("; ", ex.PushResult.Errors) : "<none>");
                    }

                    throw;
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

        private static ISyncHandle EnqueueSync(OfflineSyncArguments args = null)
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
                    success = await DoTrySyncAsync(actionsGroup.Key);
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

        private static async Task Retry(Func<Task> action, [CallerMemberName] string callingMemberName = null)
        {
            // ReSharper disable ExplicitCallerInfoArgument
            await Retry(async () =>
            {
                await action();
                return true;
            }, callingMemberName);
            // ReSharper restore ExplicitCallerInfoArgument
        }

        private static async Task<T> Retry<T>(Func<Task<T>> action, [CallerMemberName] string callingMemberName = null)
        {
            return await RetryHelper.DoAsync(action, new AsyncRetryPolicy(TimeSpan.FromSeconds(3), retryCount: 4, onExceptionAction: ex => { Log.Warn($"Exception when executing \"{callingMemberName}\": {ex.Message}. Will retry."); }));
        }

        private static async Task InitializeAsync([NotNull] IMobileServiceSyncHandler syncHandler)
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
                    store.DefineTable<Entry>();
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
                      .Subscribe(b => AwaitableExtensions.FireAndForget(ProcessSyncQueueAsync()));
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

        private static async Task<bool> GetIsLocalStoreEmptyAsync()
        {
            var syncTable = MobileService.Client.GetSyncTable<Entry>();

            var firstEntry = (await syncTable.Take(1).ToListAsync()).SingleOrDefault();

            return firstEntry == null;
        }

        private static async Task<Optional<T>> ExecuteOptionalServiceClientOperation<T>(Func<IMobileServiceClient, Task<T>> invokeFunc)
        {
            if (ConnectionHelper.IsConnectedToInternet)
            {
                try
                {
                    return await Retry(() => invokeFunc(MobileService.Client));
                }
                catch (Exception e)
                {
                    ExceptionHandlingHelper.HandleNonFatalError(e);
                }
            }

            return new Optional<T>();
        }

        public static async Task AwaitPendingSync()
        {
            await SyncCompletedEvent;

            await ProcessSyncQueueAsync();
        }

        #endregion

        //private readonly IMobileServiceTable<Entry> entryTable;
        private readonly Lazy<IMobileServiceSyncTable<Entry>> entrySyncTable;
        private readonly IMobileServiceSyncHandler syncHandler;
        private readonly IEventAggregator eventAggregator;
        private readonly ILocalSettingsService localSettingsService;
        private bool initialized;
        private readonly AsyncLock initializationLock = new AsyncLock();

        [ImportingConstructor]
        public MobileServiceBackendServiceClient([NotNull] IMobileServiceSyncHandler syncHandler, [NotNull] IEventAggregator eventAggregator, [NotNull] ILocalSettingsService localSettingsService)
        {
            Guard.NotNull(syncHandler, nameof(syncHandler));
            Guard.NotNull(eventAggregator, nameof(eventAggregator));
            Guard.NotNull(localSettingsService, nameof(localSettingsService));

            this.syncHandler = syncHandler;
            this.eventAggregator = eventAggregator;
            this.localSettingsService = localSettingsService;

            //entryTable = MobileService.Client.GetTable<Entry>();
            entrySyncTable = new Lazy<IMobileServiceSyncTable<Entry>>(CreateSyncTable);
        }

        private bool InitialPullComplete
        {
            get { return localSettingsService.GetValue(InitialPullCompleteKey, false); }
            set { localSettingsService.SetValue(InitialPullCompleteKey, value); }
        }

        private IMobileServiceSyncTable<Entry> CreateSyncTable()
        {
            return RetryHelper.Do(() => MobileService.Client.GetSyncTable<Entry>(), TimeSpan.FromSeconds(1), 2);
        }

        private IMobileServiceSyncTable<Entry> EntrySyncTable => entrySyncTable.Value;

        public async Task<IEnumerable<Entry>> LoadEntries(Expression<Func<Entry, bool>> filter)
        {
            return await Retry(async () =>
            {
                IMobileServiceTableQuery<Entry> query = EntrySyncTable.CreateQuery();

                if (filter != null)
                {
                    query = query.Where(filter);
                }

                query = query.OrderByDescending(x => x.ClientCreatedAt);

                return await query.ToListAsync();
            });
        }

        public async Task<long> GetCount(Expression<Func<Entry, bool>> filter)
        {
            return await Retry(async () =>
            {
                IMobileServiceTableQuery<Entry> query = EntrySyncTable.CreateQuery();

                if (filter != null)
                {
                    query = query.Where(filter);
                }

                var result = (await query.ToEnumerableAsync()).Count();

                return result;
            });
        }

        public async Task<Entry> LookupById(string entryId, CancellationToken? cancellationToken)
        {
            Guard.NotNull(entryId, nameof(entryId));

            return await Retry(async () =>
            {
                var result = await EntrySyncTable.LookupAsync(entryId);

                return result;
            });
        }

        public async Task<Entry> LookupByExample(Entry example)
        {
            Guard.Assert(!string.IsNullOrEmpty(example.Text), "!string.IsNullOrEmpty(example.Text)");

            if (ConnectionHelper.IsConnectedToInternet)
            {
                var parameters = new Dictionary<string, string>
                {
                    {"entryText", example.Text},
                    {"excludeId", example.Id}
                };

                var serviceResult = await Retry(() => MobileService.Client.InvokeApiAsync<Entry>("EntryLookup", HttpMethod.Post, parameters));

                return serviceResult;
            }

            var existingEntiesInLocalStorage = await EntrySyncTable.Where(x => x.Text == example.Text && x.Id != example.Id).ToListAsync();

            if (existingEntiesInLocalStorage.Count > 0)
            {
                return existingEntiesInLocalStorage[0];
            }

            return null;
        }

        public async Task<IEnumerable<Entry>> GetRandomEntries(int count)
        {
            IEnumerable<Entry> result = null;

            if (ConnectionHelper.IsConnectedToInternet)
            {
                var parameters = new Dictionary<string, string>
                    {
                        {"number", count.ToString()}
                    };

                result = await Retry(() => MobileService.Client.InvokeApiAsync<IEnumerable<Entry>>("RandomEntry", HttpMethod.Get, parameters));
            }
            else
            {
                var existingEntiesInLocalStorage = await EntrySyncTable.Where(x => !x.IsLearnt).ToListAsync();

                if (existingEntiesInLocalStorage.Count > 0)
                {
                    var indexGenerator = new Random((int)DateTime.UtcNow.Ticks);
                    var randomEntries = new List<Entry>();
                    var excludeIndices = new HashSet<int>();

                    count = Math.Min(count, existingEntiesInLocalStorage.Count);

                    for (var i = 0; i < count; i++)
                    {
                        int randomIndex;
                        do
                        {
                            randomIndex = indexGenerator.Next(0, existingEntiesInLocalStorage.Count - 1);
                        } while (excludeIndices.Contains(randomIndex));

                        excludeIndices.Add(randomIndex);

                        var randomEntry = existingEntiesInLocalStorage[randomIndex];

                        randomEntries.Add(randomEntry);
                    }

                    result = randomEntries;
                }
            }

            return result ?? Enumerable.Empty<Entry>();
        }

        public async Task<Entry> AddEntry(Entry newEntry)
        {
            return await Retry(async () =>
            {
                Entry resultEntry = null;

                var existingEntries = await EntrySyncTable.Where(x => x.Text == newEntry.Text).ToListAsync();

                if (existingEntries.Count > 0)
                {
                    resultEntry = existingEntries[0];
                    resultEntry.IsLearnt = false;

                    await EntrySyncTable.UpdateAsync(resultEntry);
                }
                else
                {
                    resultEntry = newEntry;

                    await EntrySyncTable.InsertAsync(newEntry);
                }

                EnqueueSync();

                return resultEntry;
            });
        }

        public async Task DeleteEntry(Entry entry)
        {
            await Retry(async () => { await EntrySyncTable.DeleteAsync(entry); });

            EnqueueSync();
        }

        public async Task UpdateEntry(Entry entry)
        {
            await Retry(async () => { await EntrySyncTable.UpdateAsync(entry); });

            EnqueueSync();
        }

        public async Task InitializeAsync()
        {
            using (await initializationLock.LockAsync())
            {
                if (initialized)
                {
                    return;
                }

                await Retry(async () => { await InitializeAsync(syncHandler); });

                initialized = true;

                eventAggregator.Publish(new StorageInitializedEvent());
            }
        }

        public Task<bool> TrySyncAsync(OfflineSyncArguments args = null)
        {
            return DoTrySyncAsync(args);
        }

        public async Task<LogUploadInfo> GetLogUploadInfoAsync()
        {
            if (!ConnectionHelper.IsConnectedToInternet)
            {
                throw new NoInternetConnectionException();
            }

            var parameters = new Dictionary<string, string>
            {
                {"deviceId", DeviceInfo.DeviceId}
            };

            return await Retry(() => MobileService.Client.InvokeApiAsync<LogUploadInfo>("LogUploadInfo", HttpMethod.Get, parameters));
        }

        public async Task<LocalDbState> DoInitialPullIfNeededAsync(OfflineSyncArguments args = null)
        {
            LocalDbState result = LocalDbState.Unknown;

            if (!InitialPullComplete)
            {
                var localStoreEmpty = await GetIsLocalStoreEmptyAsync();

                if (!localStoreEmpty)
                {
                    InitialPullComplete = true;
                }
                else
                {
                    result = LocalDbState.NeedsSync;

                    Optional<int> serverEntryCount = await ExecuteOptionalServiceClientOperation(c => c.InvokeApiAsync<int>("EntryCount", HttpMethod.Get, new Dictionary<string, string>()));

                    if (!serverEntryCount.HasValue || serverEntryCount.Value > 0)
                    {
                        var success = await TrySyncAsync(args);

                        if (success)
                        {
                            InitialPullComplete = true;
                            result = LocalDbState.UpToDate;
                        }
                    }
                    else
                    {
                        InitialPullComplete = true;
                    }
                }
            }

            return result;
        }

        public async Task<bool> GetIsInitialPullRequiredAsync()
        {
            if (InitialPullComplete)
            {
                return false;
            };

            var localStoreEmpty = await GetIsLocalStoreEmptyAsync();

            return localStoreEmpty;
        }
    }
}