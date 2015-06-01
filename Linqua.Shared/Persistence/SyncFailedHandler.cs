using System;
using System.Composition;

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
