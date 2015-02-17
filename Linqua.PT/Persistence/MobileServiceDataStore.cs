using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Framework;
using JetBrains.Annotations;
using Linqua.DataObjects;
using Microsoft.WindowsAzure.MobileServices.Sync;

namespace Linqua.Persistence
{
	[Export(typeof(IDataStore))]
	public class MobileServiceDataStore : IDataStore
	{
		//private readonly IMobileServiceTable<ClientEntry> entryTable;
		private readonly IMobileServiceSyncTable<ClientEntry> entryTable;
		private readonly IMobileServiceSyncHandler syncHandler;

		[ImportingConstructor]
		public MobileServiceDataStore([NotNull] IMobileServiceSyncHandler syncHandler)
		{
			Guard.NotNull(syncHandler, () => syncHandler);

			this.syncHandler = syncHandler;

			//entryTable = MobileService.Client.GetTable<ClientEntry>();
			entryTable = MobileService.Client.GetSyncTable<ClientEntry>();
			//entryTable.SystemProperties = MobileServiceSystemProperties.CreatedAt;
		}

		public async Task<IEnumerable<ClientEntry>> LoadAllEntries()
		{
			return await entryTable.OrderByDescending(x => x.CreatedAt) .ToListAsync();
		}

		public async Task<ClientEntry> AddEntry(ClientEntry newEntry)
		{
			await entryTable.InsertAsync(newEntry);

			OfflineHelper.EnqueueSync().FireAndForget();

			return newEntry;
		}

		public async Task DeleteEntry(ClientEntry entry)
		{
			await entryTable.DeleteAsync(entry);

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