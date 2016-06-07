using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Framework;
using JetBrains.Annotations;
using Linqua.DataObjects;
using Linqua.Persistence.Events;
using Linqua.Persistence.Exceptions;
using Linqua.Service.DataObjects;
using MetroLog;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;
using Nito.AsyncEx;

namespace Linqua.Persistence
{
    [Export(typeof(IBackendServiceClient))]
    [Shared]
    public class MobileServiceBackendServiceClient : IBackendServiceClient
    {
        private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger(typeof(MobileServiceBackendServiceClient).Name);

        //private readonly IMobileServiceTable<Entry> entryTable;
        private readonly Lazy<IMobileServiceSyncTable<Entry>> entrySyncTable;
        private readonly IMobileServiceSyncHandler syncHandler;
        private readonly IEventAggregator eventAggregator;
        private bool initialized;
        private readonly AsyncLock initializationLock = new AsyncLock();

        [ImportingConstructor]
        public MobileServiceBackendServiceClient([NotNull] IMobileServiceSyncHandler syncHandler, [NotNull] IEventAggregator eventAggregator)
        {
            Guard.NotNull(syncHandler, nameof(syncHandler));
            Guard.NotNull(eventAggregator, nameof(eventAggregator));

            this.syncHandler = syncHandler;
            this.eventAggregator = eventAggregator;

            //entryTable = MobileService.Client.GetTable<Entry>();
            entrySyncTable = new Lazy<IMobileServiceSyncTable<Entry>>(CreateSyncTable);
        }

        private IMobileServiceSyncTable<Entry> CreateSyncTable()
        {
            return Framework.Retry.Do(() => MobileService.Client.GetSyncTable<Entry>(), TimeSpan.FromSeconds(1), 2);
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

            return await Retry(async () =>
            {
                if (ConnectionHelper.IsConnectedToInternet)
                {
                    var parameters = new Dictionary<string, string>();

                    parameters.Add("entryText", example.Text);
                    parameters.Add("excludeId", example.Id);

                    var serviceResult = await MobileService.Client.InvokeApiAsync<Entry>("EntryLookup", HttpMethod.Post, parameters);

                    return serviceResult;
                }

                var existingEntiesInLocalStorage = await EntrySyncTable.Where(x => x.Text == example.Text && x.Id != example.Id).ToListAsync();

                if (existingEntiesInLocalStorage.Count > 0)
                {
                    return existingEntiesInLocalStorage[0];
                }

                return null;
            });
        }

        public async Task<IEnumerable<Entry>> GetRandomEntries(int count)
        {
            return await Retry(async () =>
            {
                IEnumerable<Entry> result = null;

                if (ConnectionHelper.IsConnectedToInternet)
                {
                    var parameters = new Dictionary<string, string>
                    {
                        {"number", count.ToString()}
                    };

                    result = await MobileService.Client.InvokeApiAsync<IEnumerable<Entry>>("RandomEntry", HttpMethod.Get, parameters);
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
            });
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

                OfflineHelper.EnqueueSync();

                return resultEntry;
            });
        }

        public async Task DeleteEntry(Entry entry)
        {
            await Retry(async () => { await EntrySyncTable.DeleteAsync(entry); });

            OfflineHelper.EnqueueSync();
        }

        public async Task UpdateEntry(Entry entry)
        {
            await Retry(async () => { await EntrySyncTable.UpdateAsync(entry); });

            OfflineHelper.EnqueueSync();
        }

        public async Task InitializeAsync(bool doInitialPoolIfNeeded)
        {
            using (await initializationLock.LockAsync())
            {
                if (initialized)
                {
                    return;
                }

                await Retry(async () => { await OfflineHelper.InitializeAsync(syncHandler); });

                if (doInitialPoolIfNeeded)
                {
                    await Retry(async () => { await OfflineHelper.DoInitialPullIfNeededAsync(); });
                }

                initialized = true;

                eventAggregator.Publish(new StorageInitializedEvent());
            }
        }

        public Task<bool> TrySyncAsync(OfflineSyncArguments args = null)
        {
            return OfflineHelper.TrySyncAsync(args);
        }

        public async Task<LogUploadInfo> GetLogUploadInfoAsync()
        {
            if (ConnectionHelper.IsConnectedToInternet)
            {
                return await Retry(async () =>
                {
                    var parameters = new Dictionary<string, string>
                    {
                        {"deviceId", DeviceInfo.DeviceId}
                    };

                    var serviceResult = await MobileService.Client.InvokeApiAsync<LogUploadInfo>("LogUploadInfo", HttpMethod.Get, parameters);

                    return serviceResult;
                });
            }

            throw new NoInternetConnectionException();
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
            return await Framework.Retry.DoAsync(action, TimeSpan.FromSeconds(3), retryCount: 4, onExceptionAction: ex => { Log.Warn($"Exception when executing \"{callingMemberName}\": {ex.Message}. Will retry."); });
        }
    }
}