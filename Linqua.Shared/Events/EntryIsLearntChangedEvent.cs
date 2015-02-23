using System;
using System.Collections.Generic;
using System.Text;
using Framework;
using JetBrains.Annotations;

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
