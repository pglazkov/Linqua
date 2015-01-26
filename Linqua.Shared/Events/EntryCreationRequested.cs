using Framework;

namespace Linqua.Events
{
	public class EntryCreationRequested : EventBase
	{
		public EntryCreationRequested(string entryText)
		{
			EntryText = entryText;
		}

		public string EntryText { get; private set; }
	}
}