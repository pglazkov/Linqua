using Framework;
using JetBrains.Annotations;
using Linqua.DataObjects;
using Linqua.UI;

namespace Linqua.Events
{
	public class EntryDeletedEvent : EventBase
	{
		public EntryDeletedEvent([NotNull] EntryViewModel deletedEntry)
		{
			Guard.NotNull(deletedEntry, nameof(deletedEntry));

			DeletedEntry = deletedEntry;
		}

		[NotNull]
		public EntryViewModel DeletedEntry { get; private set; }
	}
}