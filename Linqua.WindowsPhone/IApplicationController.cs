using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Linqua.DataObjects;

namespace Linqua
{
    public interface IApplicationController
    {
	    [NotNull]
	    Task DeleteEntryAsync([NotNull] EntryViewModel entry);

	    [NotNull]
	    Task UpdateEntryIsLearnedAsync([NotNull] EntryViewModel entry);

	    [NotNull]
	    Task TranslateEntryItemAsync([NotNull] ClientEntry entry, [NotNull] IEnumerable<EntryViewModel> viewModelsToUpdate);
    }
}
