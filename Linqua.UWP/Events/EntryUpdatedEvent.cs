using Framework;
using JetBrains.Annotations;
using Linqua.DataObjects;

namespace Linqua.Events
{
    public class EntryUpdatedEvent : EventBase
    {
        public EntryUpdatedEvent([NotNull] Entry entry)
        {
            Guard.NotNull(entry, nameof(entry));

            Entry = entry;
        }

        public Entry Entry { get; private set; }
    }
}