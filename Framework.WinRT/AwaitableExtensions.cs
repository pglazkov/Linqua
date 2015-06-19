using System;
using Windows.Foundation;

namespace Framework
{
	public static class AwaitableExtensionsWinRt
	{
		public static void FireAndForget(this IAsyncAction action)
		{
			if (action == null)
			{
				return;
			}

			action.AsTask().FireAndForget();
		}
	}
}