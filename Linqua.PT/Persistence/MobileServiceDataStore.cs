﻿using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq.Expressions;
using System.Net.Http;
using System.Threading.Tasks;
using Framework;
using JetBrains.Annotations;
using Linqua.DataObjects;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.Sync;

namespace Linqua.Persistence
{
	[Export(typeof(IDataStore))]
	public class MobileServiceDataStore : IDataStore
	{
		//private readonly IMobileServiceTable<ClientEntry> entryTable;
		private readonly IMobileServiceSyncTable<ClientEntry> entrySyncTable;
		private readonly IMobileServiceSyncHandler syncHandler;

		[ImportingConstructor]
		public MobileServiceDataStore([NotNull] IMobileServiceSyncHandler syncHandler)
		{
			Guard.NotNull(syncHandler, () => syncHandler);

			this.syncHandler = syncHandler;

			//entryTable = MobileService.Client.GetTable<ClientEntry>();
			entrySyncTable = MobileService.Client.GetSyncTable<ClientEntry>();
			//entryTable.SystemProperties = MobileServiceSystemProperties.CreatedAt;
		}

		public async Task<IEnumerable<ClientEntry>> LoadEntries(Expression<Func<ClientEntry, bool>> filter)
		{
			using (await OfflineHelper.AcquireDataAccessLockAsync())
			{
				IMobileServiceTableQuery<ClientEntry> query = entrySyncTable.CreateQuery();

				if (filter != null)
				{
					query = query.Where(filter);
				}

				query = query.OrderByDescending(x => x.CreatedAt);

				return await query.ToListAsync();
			}
		}

		public async Task<ClientEntry> LookupAlreadyExisting(ClientEntry example)
		{
			using (await OfflineHelper.AcquireDataAccessLockAsync())
			{
				if (ConnectionHelper.IsConnectedToInternet)
				{
					var parameters = new Dictionary<string, string>();

					parameters.Add("entryText", example.Text);

					var serviceResult = await MobileService.Client.InvokeApiAsync<ClientEntry>("EntryLookup", HttpMethod.Post, parameters);

					return serviceResult;
				}

				var existingEntiesInLocalStorage = await entrySyncTable.Where(x => x.Text == example.Text && x.Id != example.Id).ToListAsync();

				if (existingEntiesInLocalStorage.Count > 0)
				{
					return existingEntiesInLocalStorage[0];
				}
			}

			return null;
		}

		public async Task<ClientEntry> AddEntry(ClientEntry newEntry)
		{
			using (await OfflineHelper.AcquireDataAccessLockAsync())
			{
				await entrySyncTable.InsertAsync(newEntry);
			}

			OfflineHelper.EnqueueSync().FireAndForget();

			return newEntry;
		}

		public async Task DeleteEntry(ClientEntry entry)
		{
			using (await OfflineHelper.AcquireDataAccessLockAsync())
			{
				await entrySyncTable.DeleteAsync(entry);
			}

			OfflineHelper.EnqueueSync().FireAndForget();
		}

		public async Task UpdateEntry(ClientEntry entry)
		{
			using (await OfflineHelper.AcquireDataAccessLockAsync())
			{
				await entrySyncTable.UpdateAsync(entry);
			}

			OfflineHelper.EnqueueSync().FireAndForget();
		}

		public async Task InitializeAsync()
		{
			await OfflineHelper.InitializeAsync(syncHandler);
			await OfflineHelper.DoInitialPullIfNeededAsync();
		}

		public Task EnqueueSync(OfflineSyncArguments args = null)
		{
			return OfflineHelper.EnqueueSync(args);
		}
	}
}