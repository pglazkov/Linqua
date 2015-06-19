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
