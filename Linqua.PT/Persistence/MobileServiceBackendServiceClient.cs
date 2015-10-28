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

		//private readonly IMobileServiceTable<ClientEntry> entryTable;
		private readonly Lazy<IMobileServiceSyncTable<ClientEntry>> entrySyncTable;
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

			//entryTable = MobileService.Client.GetTable<ClientEntry>();
			entrySyncTable = new Lazy<IMobileServiceSyncTable<ClientEntry>>(CreateSyncTable);
        }

		private IMobileServiceSyncTable<ClientEntry> CreateSyncTable()
		{
			return Framework.Retry.Do(() => MobileService.Client.GetSyncTable<ClientEntry>(), TimeSpan.FromSeconds(1), 2);
		}

		private IMobileServiceSyncTable<ClientEntry> EntrySyncTable => entrySyncTable.Value;

		public async Task<IEnumerable<ClientEntry>> LoadEntries(Expression<Func<ClientEntry, bool>> filter)
		{
			return await Retry(async () =>
			{
				IMobileServiceTableQuery<ClientEntry> query = EntrySyncTable.CreateQuery();

				if (filter != null)
				{
					query = query.Where(filter);
				}

				query = query.OrderByDescending(x => x.ClientCreatedAt);

				return await query.ToListAsync();
			});
		}

		public async Task<long> GetCount(Expression<Func<ClientEntry, bool>> filter)
		{
			return await Retry(async () =>
			{
				IMobileServiceTableQuery<ClientEntry> query = EntrySyncTable.CreateQuery();

				if (filter != null)
				{
					query = query.Where(filter);
				}

				var result = (await query.ToEnumerableAsync()).Count();

				return result;
			});
		}

		public async Task<ClientEntry> LookupById(string entryId, CancellationToken? cancellationToken)
		{
			Guard.NotNull(entryId, nameof(entryId));

			return await Retry(async () =>
			{
				var result = await EntrySyncTable.LookupAsync(entryId);

				return result;
			});
		}

		public async Task<ClientEntry> LookupByExample(ClientEntry example)
		{
			Guard.Assert(!string.IsNullOrEmpty(example.Text), "!string.IsNullOrEmpty(example.Text)");

			return await Retry(async () =>
			{
				if (ConnectionHelper.IsConnectedToInternet)
				{
					var parameters = new Dictionary<string, string>();

					parameters.Add("entryText", example.Text);
					parameters.Add("excludeId", example.Id);

					var serviceResult = await MobileService.Client.InvokeApiAsync<ClientEntry>("EntryLookup", HttpMethod.Post, parameters);

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

		public async Task<IEnumerable<ClientEntry>> GetRandomEntries(int count)
		{
			return await Retry(async () =>
			{
				IEnumerable<ClientEntry> result = null;

				if (ConnectionHelper.IsConnectedToInternet)
				{
					var parameters = new Dictionary<string, string>
					{
						{"number", count.ToString()}
					};

					result = await MobileService.Client.InvokeApiAsync<IEnumerable<ClientEntry>>("RandomEntry", HttpMethod.Get, parameters);
				}
				else
				{
					var existingEntiesInLocalStorage = await EntrySyncTable.Where(x => !x.IsLearnt).ToListAsync();

					if (existingEntiesInLocalStorage.Count > 0)
					{
						var indexGenerator = new Random((int)DateTime.UtcNow.Ticks);
						var randomEntries = new List<ClientEntry>();
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

				return result ?? Enumerable.Empty<ClientEntry>();
			});
		}

		public async Task<ClientEntry> AddEntry(ClientEntry newEntry)
		{
			return await Retry(async () =>
			{
				ClientEntry resultEntry = null;

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

		public async Task DeleteEntry(ClientEntry entry)
		{
			await Retry(async () =>
			{
				await EntrySyncTable.DeleteAsync(entry);
			});

			OfflineHelper.EnqueueSync();
		}

		public async Task UpdateEntry(ClientEntry entry)
		{
			await Retry(async () =>
			{
				await EntrySyncTable.UpdateAsync(entry);
			});

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

			    await Retry(async () =>
			    {
				    await OfflineHelper.InitializeAsync(syncHandler);
			    });

				if (doInitialPoolIfNeeded)
				{
					await Retry(async () =>
					{
						await OfflineHelper.DoInitialPullIfNeededAsync();
					});
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
					var parameters = new Dictionary<string, string>();

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
			return await Framework.Retry.DoAsync(action, TimeSpan.FromSeconds(2), onExceptionAction: ex =>
			{
				Log.Warn($"Exception when executing \"{callingMemberName}\": {ex.Message}. Will retry.");
			});
		}
    }
}