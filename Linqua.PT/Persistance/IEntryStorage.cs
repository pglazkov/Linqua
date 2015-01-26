using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Linqua.DataObjects;

namespace Linqua.Persistance
{
	public interface IEntryStorage
	{
		[NotNull]
		Task<IEnumerable<ClientEntry>> LoadAllEntries();

		[NotNull]
		Task AddEntry([NotNull] ClientEntry newEntry);
	}
}