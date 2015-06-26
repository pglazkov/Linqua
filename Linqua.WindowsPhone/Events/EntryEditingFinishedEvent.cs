using Framework;
using JetBrains.Annotations;
using Linqua.DataObjects;

namespace Linqua.Events
{
	public class EntryEditingFinishedEvent : EventBase
	{
		public EntryEditingFinishedEvent([NotNull] ClientEntry data)
		{
			Guard.NotNull(data, () => data);

			Data = data;
		}

		[NotNull]
		public ClientEntry Data { get; private set; }
	}
}