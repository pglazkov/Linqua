using Framework;
using JetBrains.Annotations;
using Linqua.DataObjects;

namespace Linqua.Events
{
	public class EntryDeletionRequestedEvent : EventBase
	{
		public EntryDeletionRequestedEvent([NotNull] ClientEntry entryToDelete)
		{
			Guard.NotNull(entryToDelete, () => entryToDelete);

			EntryToDelete = entryToDelete;
		}

		[NotNull]
		public ClientEntry EntryToDelete { get; private set; }
	}
}