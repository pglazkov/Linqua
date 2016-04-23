using Framework;
using JetBrains.Annotations;
using Linqua.UI;

namespace Linqua.Events
{
    public class EntryQuickEditRequestedEvent : EventBase
    {
        public EntryQuickEditRequestedEvent([NotNull] EntryViewModel entryViewModel)
        {
            Guard.NotNull(entryViewModel, nameof(entryViewModel));

            EntryViewModel = entryViewModel;
        }

        [NotNull]
        public EntryViewModel EntryViewModel { get; private set; }
    }
}