using System;
using Windows.Foundation;
using Framework;

namespace Linqua.Framework
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