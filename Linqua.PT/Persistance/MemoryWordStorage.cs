using System.Collections.Generic;
using System.Composition;
using System.Threading.Tasks;
using Linqua.Model;

namespace Linqua.Persistance
{
	[Export(typeof(IWordStorage))]
	public class MemoryWordStorage : IWordStorage
	{
		public Task<IEnumerable<Word>> LoadAllWords()
		{
			return Task.Factory.StartNew(() => FakeData.FakeWords);
		}
	}
}