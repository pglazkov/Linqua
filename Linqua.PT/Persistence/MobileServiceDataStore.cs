using System;
using System.Collections.Generic;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Framework;
using JetBrains.Annotations;
using Linqua.DataObjects;
using Linqua.Persistence.Events;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;

namespace Linqua.Persistence
{
	[Export(typeof(IDataStore))]
	[Shared]
	public class MobileServiceDataStore : IDataStore
	{
		//private readonly IMobileServiceTable<ClientEntry> entryTable;
		private readonly IMobileServiceSyncTable<ClientEntry> entrySyncTable;
		private readonly IMobileServiceSyncHandler syncHandler;
		private readonly IEventAggregator eventAggregator;

		[ImportingConstructor]
		public MobileServiceDataStore([NotNull] IMobileServiceSyncHandler syncHandler, [NotNull] IEventAggregator eventAggregator)
		{
			Guard.NotNull(syncHandler, () => syncHandler);
			Guard.NotNull(eventAggregator, () => eventAggregator);

			this.syncHandler = syncHandler;
			this.eventAggregator = eventAggregator;

			//entryTable = MobileService.Client.GetTable<ClientEntry>();
			entrySyncTable = MobileService.Client.GetSyncTable<ClientEntry>();
			//entryTable.SystemProperties = MobileServiceSystemProperties.CreatedAt;
		}

		public async Task<IEnumerable<ClientEntry>> LoadEntries(Expression<Func<ClientEntry, bool>> filter)
		{
			IMobileServiceTableQuery<ClientEntry> query = entrySyncTable.CreateQuery();

			if (filter != null)
			{
				query = query.Where(filter);
			}

			query = query.OrderByDescending(x => x.CreatedAt);

			return await query.ToListAsync();
		}

		public async Task<long> GetCount(Expression<Func<ClientEntry, bool>> filter)
		{
			IMobileServiceTableQuery<ClientEntry> query = entrySyncTable.CreateQuery();

			if (filter != null)
			{
				query = query.Where(filter);
			}

			var result = (await query.ToEnumerableAsync()).Count();

			return result;
		}

		public async Task<ClientEntry> LookupById(string entryId, CancellationToken? cancellationToken)
		{
			Guard.NotNull(entryId, () => entryId);

			var result = await entrySyncTable.LookupAsync(entryId);

			return result;
		}

		public async Task<ClientEntry> LookupByExample(ClientEntry example)
		{
			Guard.Assert(!string.IsNullOrEmpty(example.Text), "!string.IsNullOrEmpty(example.Text)");

			if (ConnectionHelper.IsConnectedToInternet)
			{
				var parameters = new Dictionary<string, string>();

				parameters.Add("entryText", example.Text);
				parameters.Add("excludeId", example.Id);

				var serviceResult = await MobileService.Client.InvokeApiAsync<ClientEntry>("EntryLookup", HttpMethod.Post, parameters);

				return serviceResult;
			}

			var existingEntiesInLocalStorage = await entrySyncTable.Where(x => x.Text == example.Text && x.Id != example.Id).ToListAsync();

			if (existingEntiesInLocalStorage.Count > 0)
			{
				return existingEntiesInLocalStorage[0];
			}

			return null;
		}

		public async Task<IEnumerable<ClientEntry>> GetRandomEntries(int count)
		{
			if (ConnectionHelper.IsConnectedToInternet)
			{
				var parameters = new Dictionary<string, string>();

				parameters.Add("number", count.ToString());

				var serviceResult = await MobileService.Client.InvokeApiAsync<IEnumerable<ClientEntry>>("RandomEntry", HttpMethod.Get, parameters);

				return serviceResult;
			}

			var existingEntiesInLocalStorage = await entrySyncTable.Where(x => !x.IsLearnt).ToListAsync();

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
					}
					while (excludeIndices.Contains(randomIndex));

					excludeIndices.Add(randomIndex);

					var randomEntry = existingEntiesInLocalStorage[randomIndex];

					randomEntries.Add(randomEntry);
				}

				return randomEntries;
			}

			return null;
		}

		public async Task<ClientEntry> AddEntry(ClientEntry newEntry)
		{
			ClientEntry resultEntry = null;

			var existingEntries = await entrySyncTable.Where(x => x.Text == newEntry.Text).ToListAsync();

			if (existingEntries.Count > 0)
			{
				resultEntry = existingEntries[0];
				resultEntry.IsLearnt = false;

				await entrySyncTable.UpdateAsync(resultEntry);
			}
			else
			{
				resultEntry = newEntry;

				await entrySyncTable.InsertAsync(newEntry);
			}

			OfflineHelper.EnqueueSync().FireAndForget();

			return resultEntry;
		}

		public async Task DeleteEntry(ClientEntry entry)
		{
			await entrySyncTable.DeleteAsync(entry);

			OfflineHelper.EnqueueSync().FireAndForget();
		}

		public async Task UpdateEntry(ClientEntry entry)
		{
			await entrySyncTable.UpdateAsync(entry);

			OfflineHelper.EnqueueSync().FireAndForget();
		}

		public async Task InitializeAsync(bool doInitialPoolIfNeeded)
		{
			await OfflineHelper.InitializeAsync(syncHandler);

			if (doInitialPoolIfNeeded)
			{
				await OfflineHelper.DoInitialPullIfNeededAsync();
			}

			eventAggregator.Publish(new StorageInitializedEvent());
		}

		public Task EnqueueSync(OfflineSyncArguments args = null)
		{
			return OfflineHelper.EnqueueSync(args);
		}
	}
}