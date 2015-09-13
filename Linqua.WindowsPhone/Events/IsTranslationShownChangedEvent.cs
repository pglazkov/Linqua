using Framework;
using JetBrains.Annotations;
using Linqua.UI;

namespace Linqua.Events
{
    public class IsTranslationShownChangedEvent : EventBase
    {
        public IsTranslationShownChangedEvent([NotNull] EntryListItemViewModel item)
        {
            Guard.NotNull(item, nameof(item));

            Item = item;
        }

        [NotNull]
        public EntryListItemViewModel Item { get; private set; }
    }
}