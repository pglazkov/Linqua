using Framework;
using JetBrains.Annotations;
using Linqua.DataObjects;

namespace Linqua.Events
{
	public class EntryUpdatedEvent : EventBase
    {
		public EntryUpdatedEvent([NotNull] ClientEntry entry)
		{
			Guard.NotNull(entry, nameof(entry));

			Entry = entry;
		}

		public ClientEntry Entry { get; private set; }
    }
}
	