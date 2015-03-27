using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;

namespace Linqua
{
    public class EntryListItemTimeGroupViewModel : TimeGroupViewModel<EntryListItemViewModel>
    {
	    public EntryListItemTimeGroupViewModel([NotNull] string groupName) : base(groupName)
	    {
	    }
    }
}
