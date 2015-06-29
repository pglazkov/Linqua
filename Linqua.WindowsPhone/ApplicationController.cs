using System;
using System.Composition;
using System.Reactive.Linq;
using Windows.UI.Xaml.Controls;
using Framework;
using JetBrains.Annotations;
using Linqua.Events;
using Linqua.UI;

namespace Linqua
{
	[Export]
	[Shared]
	[UsedImplicitly]
	public class ApplicationController
	{
		private readonly IEventAggregator eventAggregator;
		private Frame navigationFrame;

		[ImportingConstructor]
		public ApplicationController([NotNull] IEventAggregator eventAggregator)
		{
			Guard.NotNull(eventAggregator, () => eventAggregator);

			this.eventAggregator = eventAggregator;
		}

		public void Initialize()
		{
			eventAggregator.GetEvent<EntryEditRequestedEvent>().Subscribe(OnEntryEditRequested);
		}

		public void RegisterFrame([NotNull] Frame frame)
		{
			Guard.NotNull(frame, () => frame);

			navigationFrame = frame;
		}

		private void OnEntryEditRequested(EntryEditRequestedEvent e)
		{
			Guard.Assert(navigationFrame != null, "Please initialize the main navigation frame first (call the RegisterFrame method)");

			navigationFrame.Navigate(typeof(EntryEditPage), e.EntryId);
		}
	}
}