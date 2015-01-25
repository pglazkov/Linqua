using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using Linqua.DataObjects;

namespace Linqua.Persistance
{
	//[Export(typeof(IEntryStorage))]
	public class MemoryEntryStorage : IEntryStorage
	{
		public Task<IEnumerable<ClientEntry>> LoadAllEntries()
		{
			return Task.Factory.StartNew(() => FakeData.FakeWords);
		}
	}
}