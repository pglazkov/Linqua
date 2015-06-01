using Framework;
using JetBrains.Annotations;
using Linqua.DataObjects;

namespace Linqua.Events
{
	public class EntryDefinitionChangedEvent : EventBase
    {
		public EntryDefinitionChangedEvent([NotNull] ClientEntry entry)
		{
			Guard.NotNull(entry, () => entry);

			Entry = entry;
		}

		public ClientEntry Entry { get; private set; }
    }
}
	