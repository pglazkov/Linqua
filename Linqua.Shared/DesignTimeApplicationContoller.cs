﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Framework.PlatformServices;
using Linqua.DataObjects;

namespace Linqua
{
    public class DesignTimeApplicationContoller : IApplicationController
    {
	    public Task DeleteEntryAsync(EntryViewModel entry)
	    {
		    return Task.FromResult(true);
	    }

	    public Task UpdateEntryIsLearnedAsync(EntryViewModel entry)
	    {
			return Task.FromResult(true);
	    }

	    public Task TranslateEntryItemAsync(ClientEntry entry, IEnumerable<EntryViewModel> viewModelsToUpdate)
	    {
			return Task.FromResult(true);
	    }
    }
}