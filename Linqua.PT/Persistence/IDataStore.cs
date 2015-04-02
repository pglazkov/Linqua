using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Linqua.DataObjects;

namespace Linqua.Persistence
{
	public interface IDataStore
	{
		[NotNull]
		Task<IEnumerable<ClientEntry>> LoadEntries([CanBeNull] Expression<Func<ClientEntry, bool>> filter = null);

		[NotNull]
		Task<ClientEntry> LookupById([NotNull] string entryId);

		[NotNull]
		Task<ClientEntry> LookupByExample([NotNull] ClientEntry example);

		[NotNull]
		Task<ClientEntry> AddEntry([NotNull] ClientEntry newEntry);

		[NotNull]
		Task DeleteEntry([NotNull] ClientEntry entry);

		[NotNull]
		Task UpdateEntry([NotNull] ClientEntry entry);

		[NotNull]
		Task InitializeAsync();

		[NotNull]
		Task EnqueueSync(OfflineSyncArguments args = null);
	}
}