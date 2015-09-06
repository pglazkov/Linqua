using System;
using Framework;
using JetBrains.Annotations;

namespace Linqua.Events
{
	public class EntryEditRequestedEvent : EventBase
	{
		public EntryEditRequestedEvent([NotNull] string entryId)
		{
			Guard.NotNullOrEmpty(entryId, nameof(entryId));

			EntryId = entryId;
		}

		[NotNull]
		public string EntryId { get; private set; }
	}
}