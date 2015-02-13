using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Linqua.DataObjects;

namespace Linqua.Persistence
{
	//[Export(typeof(IDataStore))]
	public class MemoryDataStore : IDataStore
	{
		public Task<IEnumerable<ClientEntry>> LoadAllEntries()
		{
			return Task.Factory.StartNew(() => (IEnumerable<ClientEntry>)FakeData.FakeWords);
		}

		public Task<ClientEntry> AddEntry(ClientEntry newEntry)
		{
			FakeData.FakeWords.Add(newEntry);

			return Task.FromResult(newEntry);
		}

		public Task DeleteEntry(ClientEntry entry)
		{
			FakeData.FakeWords.Remove(entry);

			return Task.FromResult(true);
		}

		public Task InitializeAsync()
		{
			return Task.FromResult(true);
		}

		public Task EnqueueSync(OfflineSyncArguments args)
		{
			return Task.FromResult(true);
		}
	}
}