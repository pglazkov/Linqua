using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Threading.Tasks;
using Framework;
using JetBrains.Annotations;
using Linqua.DataObjects;
using Microsoft.WindowsAzure.MobileServices;
using Microsoft.WindowsAzure.MobileServices.SQLiteStore;
using Microsoft.WindowsAzure.MobileServices.Sync;

namespace Linqua.Persistence
{
	[Export(typeof(IEntryStorage))]
	public class MobileServiceEntryStorage : IEntryStorage
	{
		//private readonly IMobileServiceTable<ClientEntry> entryTable;
		private readonly IMobileServiceSyncTable<ClientEntry> entryTable;
		private readonly IMobileServiceSyncHandler syncHandler;
		private readonly ISyncFailedHandler syncFailedHandler;

		[ImportingConstructor]
		public MobileServiceEntryStorage([NotNull] IMobileServiceSyncHandler syncHandler, [NotNull] ISyncFailedHandler syncFailedHandler)
		{
			Guard.NotNull(syncHandler, () => syncHandler);
			Guard.NotNull(syncFailedHandler, () => syncFailedHandler);

			this.syncHandler = syncHandler;
			this.syncFailedHandler = syncFailedHandler;

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

			await SyncAsync();
		}

		private Task SyncAsync()
		{
			return Task.Run(async () =>
			{
				try
				{
					await MobileService.Client.SyncContext.PushAsync();
					await entryTable.PullAsync("entryItems", entryTable.CreateQuery());
				}
				catch (Exception ex)
				{
					syncFailedHandler.Handle(ex);
				}
			});
		}

	}
}