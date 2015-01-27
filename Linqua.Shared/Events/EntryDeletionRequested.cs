using Framework;
using JetBrains.Annotations;
using Linqua.DataObjects;

namespace Linqua.Events
{
	public class EntryDeletionRequested : EventBase
	{
		public EntryDeletionRequested([NotNull] ClientEntry entryToDelete)
		{
			Guard.NotNull(entryToDelete, () => entryToDelete);

			EntryToDelete = entryToDelete;
		}

		[NotNull]
		public ClientEntry EntryToDelete { get; private set; }
	}
}