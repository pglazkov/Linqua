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
		private static readonly MobileServiceClient MobileService = new MobileServiceClient("http://localhost:59988");

		// Use this constructor instead after publishing to the cloud
		// public static MobileServiceClient MobileService = new MobileServiceClient(
		//      "https://linqua.azure-mobile.net/",
		//      "veBcEvMWjGNePbAKosRSIQzJGiTrfc50"
		//);

		private static readonly IMobileServiceTable<ClientEntry> EntryTable = MobileService.GetTable<ClientEntry>();


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