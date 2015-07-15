using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
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
		Task<long> GetCount([CanBeNull] Expression<Func<ClientEntry, bool>> filter = null);

		[NotNull]
		Task<ClientEntry> LookupById([NotNull] string entryId, CancellationToken? cancellationToken = null);

		[NotNull]
		Task<ClientEntry> LookupByExample([NotNull] ClientEntry example);

		[NotNull]
		Task<ClientEntry> GetRandomEntry(string excludeId = null);

		[NotNull]
		Task<ClientEntry> AddEntry([NotNull] ClientEntry newEntry);

		[NotNull]
		Task DeleteEntry([NotNull] ClientEntry entry);

		[NotNull]
		Task UpdateEntry([NotNull] ClientEntry entry);

		[NotNull]
		Task InitializeAsync(bool doInitialPoolIfNeeded = true);

		[NotNull]
		Task EnqueueSync(OfflineSyncArguments args = null);
	}
}