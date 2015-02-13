using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Linqua.DataObjects;

namespace Linqua.Persistence
{
	public class OfflineSyncArguments
	{
		public static readonly OfflineSyncArguments Default = new OfflineSyncArguments(null);

		public OfflineSyncArguments([CanBeNull] Expression<Func<ClientEntry, bool>> clientEntryFilter)
		{
			ClientEntryFilter = clientEntryFilter;
		}

		[CanBeNull]
		public Expression<Func<ClientEntry, bool>> ClientEntryFilter { get; private set; }
	}
}
