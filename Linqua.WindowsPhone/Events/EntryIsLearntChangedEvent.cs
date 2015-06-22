using Framework;
using JetBrains.Annotations;
using Linqua.UI;

namespace Linqua.Events
{
    public class EntryIsLearntChangedEvent : EventBase
    {
	    public EntryIsLearntChangedEvent([NotNull] EntryViewModel entryViewModel)
	    {
		    Guard.NotNull(entryViewModel, () => entryViewModel);

		    EntryViewModel = entryViewModel;
	    }

	    public EntryViewModel EntryViewModel { get; private set; }
    }
}
