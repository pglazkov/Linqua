using System;
using System.Composition;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Framework;
using Framework.PlatformServices;
using JetBrains.Annotations;
using Linqua.Events;
using Linqua.Notifications;
using Linqua.Persistence.Events;
using Linqua.UI;
using MetroLog;

namespace Linqua
{
	[Export]
	[Shared]
	[UsedImplicitly]
	public class ApplicationController
	{
        private static readonly ILogger Log = LogManagerFactory.DefaultLogManager.GetLogger<ApplicationController>();

        private readonly IEventAggregator eventAggregator;
		private readonly ILiveTileManager liveTileManager;
	    private readonly IDispatcherService dispatcherService;
	    private Frame navigationFrame;

		[ImportingConstructor]
		public ApplicationController([NotNull] IEventAggregator eventAggregator, [NotNull] ILiveTileManager liveTileManager, [NotNull] IDispatcherService dispatcherService)
		{
			Guard.NotNull(eventAggregator, nameof(eventAggregator));
			Guard.NotNull(liveTileManager, nameof(liveTileManager));
		    Guard.NotNull(dispatcherService, nameof(dispatcherService));

			this.eventAggregator = eventAggregator;
			this.liveTileManager = liveTileManager;
		    this.dispatcherService = dispatcherService;
		}

		public void Initialize()
		{
			eventAggregator.GetEvent<EntryEditRequestedEvent>().Subscribe(OnEntryEditRequested);
			eventAggregator.GetEvent<StorageInitializedEvent>().Subscribe(OnStorageInitialized);
		}

		public void RegisterFrame([NotNull] Frame frame)
		{
			Guard.NotNull(frame, nameof(frame));

			navigationFrame = frame;
		}

		private void OnEntryEditRequested(EntryEditRequestedEvent e)
		{
			Guard.Assert(navigationFrame != null, "Please initialize the main navigation frame first (call the RegisterFrame method)");

		    dispatcherService.InvokeAsync(() =>
		    {
		        navigationFrame.Navigate(typeof(EntryEditPage), e.EntryId);
		    });
		}

		private void OnStorageInitialized(StorageInitializedEvent e)
		{
		    UpdateLiveTileAsync().FireAndForget();
		}

	    private async Task UpdateLiveTileAsync()
	    {
	        try
	        {
	            await Task.Run(() => liveTileManager.UpdateTileAsync());
	        }
	        catch (Exception e)
	        {
	            Log.Warn("Could not update live tile.", e);
	        }
	    }
	}
}