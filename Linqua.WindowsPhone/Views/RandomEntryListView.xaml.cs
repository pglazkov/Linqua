using System;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media.Animation;
using Framework;
using Framework.PlatformServices;
using Linqua.Events;

namespace Linqua
{
    public partial class RandomEntryListView : UserControl
    {
	    private bool isLoaded;

	    public RandomEntryListView()
        {
            InitializeComponent();

		    ItemView.DataContextChanged += OnRandomItemDataContextChanged;
	        Loaded += OnLoaded;
		    Unloaded += OnUnloaded;

		    if (!DesignMode.DesignModeEnabled)
		    {
			    var eventAggregator = CompositionManager.Current.GetInstance<IEventAggregator>();
			    eventAggregator.GetEvent<StopFirstUseTutorialEvent>().Subscribe(OnStopFirstUseTutorialRequested);
		    }
        }

	    private EntryListViewModel ViewModel
	    {
		    get { return (EntryListViewModel)DataContext; }
	    }

	    private IRoamingSettingsService RoamingSettings
	    {
		    get { return CompositionManager.Current.GetInstance<IRoamingSettingsService>(); }
	    }

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			isLoaded = true;
			StartTutorialIfNeeded();
		}

	    private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			isLoaded = false;
		}

		private void OnRandomItemDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (isLoaded)
			{
				StartTutorialIfNeeded();
			}
		}

		private void StartTutorialIfNeeded()
		{
			if (ViewModel.RandomEntryViewModels.Count > 0)
			{
				var isComplted = RoamingSettings.GetValue<bool>(RoamingStorageKeys.IsRandomEntryUITutorialCompletedKey);

				if (!isComplted)
				{
					var firstUseStoryboard = (Storyboard)Resources["FirstUseStoryboard"];

					firstUseStoryboard.BeginTime = TimeSpan.FromMilliseconds(1000);
					firstUseStoryboard.Begin();
				}
			}
		}

	    private void OnStopFirstUseTutorialRequested(StopFirstUseTutorialEvent e)
	    {
		    StopFirstUseTutorial();
	    }

		private void StopFirstUseTutorial()
		{
			var firstUseStoryboard = (Storyboard)Resources["FirstUseStoryboard"];

			firstUseStoryboard.Stop();

			RoamingSettings.SetValue(RoamingStorageKeys.IsRandomEntryUITutorialCompletedKey, true);
		}

	    private void EntryHolding(object sender, HoldingRoutedEventArgs e)
	    {
			var senderElement = sender as FrameworkElement;
			var flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);
			
			flyoutBase.ShowAt(senderElement);
	    }

	    private void EntryPressed(object sender, PointerRoutedEventArgs e)
	    {
			
	    }

	    private void EntryLoaded(object sender, RoutedEventArgs e)
	    {
			var entryView = (Control)sender;

			var entryVm = (EntryListItemViewModel)entryView.DataContext;

			if (entryVm == null)
			{
				return;
			}

			if (entryVm.JustAdded)
			{
				entryVm.JustAdded = false;
			}
	    }

	    private void OnItemFlickedAway(object sender, EventArgs e)
	    {
		    StopFirstUseTutorial();

		    if (ViewModel.ShowNextEntriesCommand.CanExecute())
		    {
			    ViewModel.ShowNextEntriesCommand.Execute().FireAndForget();
		    }
	    }
    }
}
