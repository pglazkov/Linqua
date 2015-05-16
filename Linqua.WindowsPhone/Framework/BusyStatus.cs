using Framework;
using JetBrains.Annotations;

namespace Linqua.Framework
{
	public class BusyStatus
	{
		public BusyStatus([NotNull] string statusText)
		{
			Guard.NotNullOrEmpty(statusText, () => statusText);

			StatusText = statusText;
		}

		[NotNull]
		public string StatusText { get; private set; }
	}
}