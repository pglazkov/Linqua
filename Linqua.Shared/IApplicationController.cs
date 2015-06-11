using System.Collections.Generic;
using System.Threading.Tasks;
using Linqua.DataObjects;

namespace Linqua
{
    public interface IApplicationController
    {
	    void OnIsLearntChanged(EntryViewModel entry);

		Task TranslateEntryItemAsync(ClientEntry entry, IEnumerable<EntryViewModel> viewModelsToUpdate);
    }
}
