using Framework;

namespace Linqua.Events
{
	public class EntryCreationRequestedEvent : EventBase
	{
		public EntryCreationRequestedEvent(string entryText)
		{
			EntryText = entryText;
		}

		public string EntryText { get; private set; }
	}
}