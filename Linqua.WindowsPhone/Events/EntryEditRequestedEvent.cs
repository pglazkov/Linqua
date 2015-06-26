using Framework;
using JetBrains.Annotations;
using Linqua.UI;

namespace Linqua.Events
{
	public class EntryEditRequestedEvent : EventBase
	{
		public EntryEditRequestedEvent([NotNull] EntryViewModel entryViewModel)
		{
			Guard.NotNull(entryViewModel, () => entryViewModel);

			EntryViewModel = entryViewModel;
		}

		[NotNull]
		public EntryViewModel EntryViewModel { get; private set; }
	}
}