using JetBrains.Annotations;

namespace Linqua.UI
{
    public class EntryListItemTimeGroupViewModel : TimeGroupViewModel<EntryListItemViewModel>
    {
	    public EntryListItemTimeGroupViewModel([NotNull] string groupName) : base(groupName)
	    {
	    }
    }
}
