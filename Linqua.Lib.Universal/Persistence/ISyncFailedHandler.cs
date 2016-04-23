using System;
using JetBrains.Annotations;

namespace Linqua.Persistence
{
	public interface ISyncFailedHandler
	{
		void Handle([NotNull] Exception ex);
	}
}