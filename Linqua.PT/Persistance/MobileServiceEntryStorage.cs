using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using Framework;
using JetBrains.Annotations;
using Linqua.DataObjects;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;

namespace Linqua.Persistance
{
	[Export(typeof(IEntryStorage))]
	public class MobileServiceEntryStorage : IEntryStorage
	{
		//private readonly IMobileServiceTable<ClientEntry> entryTable;
		private readonly IMobileServiceSyncTable<ClientEntry> entryTable;
		private readonly IMobileServiceSyncHandler syncHandler;

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

			await entryTable.RefreshAsync(newEntry);

			SyncAsync().FireAndForget();

			return newEntry;
		}

		public async Task DeleteEntry(ClientEntry entry)
		{
			await entryTable.DeleteAsync(entry);

			SyncAsync().FireAndForget();
		}

		public async Task InitializeAsync()
		{
			if (!MobileService.Client.SyncContext.IsInitialized)
			{
				var store = new MobileServiceSQLiteStore("localstore.db");
				store.DefineTable<ClientEntry>();
				await MobileService.Client.SyncContext.InitializeAsync(store, syncHandler);
			}

			SyncAsync().FireAndForget();
		}

		private async Task SyncAsync()
		{
			await MobileService.Client.SyncContext.PushAsync();
			await entryTable.PullAsync("entryItems", entryTable.CreateQuery());
		}

	}
}