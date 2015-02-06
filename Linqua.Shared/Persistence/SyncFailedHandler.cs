using System;
using System.Collections.Generic;
using System.Composition;
using System.Text;

namespace Linqua.Persistence
{
	[Export(typeof(ISyncFailedHandler))]
    public class SyncFailedHandler : ISyncFailedHandler
    {
	    public void Handle(Exception ex)
	    {
		    // TODO: Log the exception
	    }
    }
}
