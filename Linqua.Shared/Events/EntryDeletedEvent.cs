using Framework;
using JetBrains.Annotations;
using Linqua.DataObjects;

namespace Linqua.Events
{
	public class EntryDeletedEvent : EventBase
	{
		public EntryDeletedEvent([NotNull] EntryViewModel deletedEntry)
		{
			Guard.NotNull(deletedEntry, () => deletedEntry);

			DeletedEntry = deletedEntry;
		}

		[NotNull]
		public EntryViewModel DeletedEntry { get; private set; }
	}
}