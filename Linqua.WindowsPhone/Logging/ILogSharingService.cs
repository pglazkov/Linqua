using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Linqua.Logging
{
	public interface ILogSharingService
	{
		[NotNull]
		Task<Uri> ShareCurrentLogAsync();
	}
}