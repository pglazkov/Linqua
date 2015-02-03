using System;
using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using Linqua.DataObjects;
using Microsoft.WindowsAzure.MobileServices;

namespace Linqua.Persistance
{
	[Export(typeof(IEntryStorage))]
	public class MobileServiceEntryStorage : IEntryStorage
	{
		private static readonly IMobileServiceTable<ClientEntry> EntryTable = MobileService.Client.GetTable<ClientEntry>();

		static MobileServiceEntryStorage()
		{
			EntryTable.SystemProperties = MobileServiceSystemProperties.CreatedAt;
		}

		public async Task<IEnumerable<ClientEntry>> LoadAllEntries()
		{
			return await EntryTable.ToListAsync();
		}

		public async Task<ClientEntry> AddEntry(ClientEntry newEntry)
		{
			await EntryTable.InsertAsync(newEntry);

			await EntryTable.RefreshAsync(newEntry);

			return newEntry;
		}

		public async Task DeleteEntry(ClientEntry entry)
		{
			await EntryTable.DeleteAsync(entry);
		}

	}
}