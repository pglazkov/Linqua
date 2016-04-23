using Framework;
using JetBrains.Annotations;

namespace Linqua.Framework
{
	public class BusyStatus
	{
		public BusyStatus([NotNull] string statusText)
		{
			Guard.NotNullOrEmpty(statusText, nameof(statusText));

			StatusText = statusText;
		}

		[NotNull]
		public string StatusText { get; private set; }
	}
}