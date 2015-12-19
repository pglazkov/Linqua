using System;
using System.Threading.Tasks;
using Windows.Foundation;
using JetBrains.Annotations;

namespace Linqua.Logging
{
	public interface ILogSharingService
	{
		[NotNull]
		IAsyncOperation<Uri> ShareCurrentLogAsync();
	}
}