using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using Framework;
using JetBrains.Annotations;
using Linqua.DataObjects;
using Microsoft.WindowsAzure.MobileServices.Sync;

namespace Linqua.Persistence
{
	[Export(typeof(IEntryStorage))]
	public class MobileServiceEntryStorage : IEntryStorage
	{
		//private readonly IMobileServiceTable<ClientEntry> entryTable;
		private readonly IMobileServiceSyncTable<ClientEntry> entryTable;
		private readonly IMobileServiceSyncHandler syncHandler;

		[ImportingConstructor]
		public MobileServiceEntryStorage([NotNull] IMobileServiceSyncHandler syncHandler)
		{
			Guard.NotNull(syncHandler, () => syncHandler);

			this.syncHandler = syncHandler;

			//entryTable = MobileService.Client.GetTable<ClientEntry>();
			entryTable = MobileService.Client.GetSyncTable<ClientEntry>();
			//entryTable.SystemProperties = MobileServiceSystemProperties.CreatedAt;
		}

		public async Task<IEnumerable<ClientEntry>> LoadAllEntries()
		{
			return await entryTable.ToListAsync();
		}

		public async Task<ClientEntry> AddEntry(ClientEntry newEntry)
		{
			await entryTable.InsertAsync(newEntry);

			return newEntry;
		}

		public async Task DeleteEntry(ClientEntry entry)
		{
			await entryTable.DeleteAsync(entry);
		}

		public async Task InitializeAsync()
		{
			await OfflineSync.InitializeAsync(syncHandler);
		}

	}
}