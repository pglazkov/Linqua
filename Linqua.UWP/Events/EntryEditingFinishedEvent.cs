using Framework;
using JetBrains.Annotations;
using Linqua.DataObjects;

namespace Linqua.Events
{
    public class EntryEditingFinishedEvent : EventBase
    {
        public EntryEditingFinishedEvent([NotNull] Entry data)
        {
            Guard.NotNull(data, nameof(data));

            Data = data;
        }

        [NotNull]
        public Entry Data { get; private set; }
    }
}