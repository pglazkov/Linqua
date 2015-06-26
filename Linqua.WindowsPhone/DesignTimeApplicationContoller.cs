using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Framework.PlatformServices;
using Linqua.DataObjects;
using Linqua.UI;

namespace Linqua
{
    public class DesignTimeApplicationContoller : IApplicationController
    {
	    public Task DeleteEntryAsync(EntryViewModel entry)
	    {
		    return Task.FromResult(true);
	    }

	    public Task UpdateEntryAsync(ClientEntry entry)
	    {
		    return Task.FromResult(true);
	    }

	    public Task UpdateEntryIsLearnedAsync(EntryViewModel entry)
	    {
			return Task.FromResult(true);
	    }

	    public Task<string> TranslateEntryItemAsync(ClientEntry entry, IEnumerable<EntryViewModel> viewModelsToUpdate)
	    {
			return Task.FromResult(string.Empty);
	    }
    }
}
