using Framework;
using JetBrains.Annotations;
using Linqua.DataObjects;

namespace Linqua.Events
{
    public class EntryCreatedEvent : EventBase
    {
        public EntryCreatedEvent([NotNull] Entry entry)
        {
            Guard.NotNull(entry, nameof(entry));

            Entry = entry;
        }

        public Entry Entry { get; private set; }
    }
}