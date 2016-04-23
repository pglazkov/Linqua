using Framework;
using JetBrains.Annotations;
using Linqua.DataObjects;

namespace Linqua.Events
{
    public class EntryCreatedEvent : EventBase
    {
        public EntryCreatedEvent([NotNull] ClientEntry entry)
        {
            Guard.NotNull(entry, nameof(entry));

            Entry = entry;
        }

        public ClientEntry Entry { get; private set; }
    }
}