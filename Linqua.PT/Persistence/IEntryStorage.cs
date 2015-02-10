using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Linqua.DataObjects;

namespace Linqua.Persistence
{
	public interface IEntryStorage
	{
		[NotNull]
		Task<IEnumerable<ClientEntry>> LoadAllEntries();

		[NotNull]
		Task<ClientEntry> AddEntry([NotNull] ClientEntry newEntry);

		[NotNull]
		Task DeleteEntry([NotNull] ClientEntry entry);

		[NotNull]
		Task InitializeAsync();

		[NotNull]
		Task EnqueueSync([CanBeNull] Expression<Func<ClientEntry, bool>> query = null);
	}
}