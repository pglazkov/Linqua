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
using Linqua.Framework;
using Linqua.ViewModels;

namespace Linqua
{
    public partial class RandomEntryListView : UserControl, IPivotContentView
    {
	    private bool isLoaded;
	    private bool isFirstUseTutorialRunning;

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

	    private RandomEntryListViewModel ViewModel
	    {
			get { return (RandomEntryListViewModel)DataContext; }
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
			if (isFirstUseTutorialRunning)
			{
				return;
			}

			if (ViewModel != null && ViewModel.RandomEntryViewModels.Count > 0)
			{
				var isComplted = RoamingSettings.GetValue<bool>(RoamingStorageKeys.IsRandomEntryUITutorialCompletedKey);

				if (!isComplted)
				{
					isFirstUseTutorialRunning = true;

					var firstUseStoryboard = (Storyboard)Resources["FirstUseStoryboard"];
					firstUseStoryboard.Completed += OnFirstUseTutorialStoryboardCompleted;
					firstUseStoryboard.BeginTime = TimeSpan.FromSeconds(2.5);
					firstUseStoryboard.Begin();
				}
			}
		}

	    private void OnFirstUseTutorialStoryboardCompleted(object sender, object e)
	    {
		    isFirstUseTutorialRunning = false;
	    }

	    private void OnStopFirstUseTutorialRequested(StopFirstUseTutorialEvent e)
	    {
		    CompleteFirstUseTutorial();
	    }

		private void CompleteFirstUseTutorial()
		{
			StopFirstUseTutorial();

			RoamingSettings.SetValue(RoamingStorageKeys.IsRandomEntryUITutorialCompletedKey, true);
		}

	    private void StopFirstUseTutorial()
	    {
		    isFirstUseTutorialRunning = false;

		    var firstUseStoryboard = (Storyboard)Resources["FirstUseStoryboard"];

		    firstUseStoryboard.Stop();
		    firstUseStoryboard.Completed -= OnFirstUseTutorialStoryboardCompleted;
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

	    private void OnItemFlickedAway(object sender, FlickedAwayEventArgs e)
	    {
		    CompleteFirstUseTutorial();

		    if (e.Direction == FlickDirection.Left)
		    {
			    if (ViewModel.ShowNextEntriesCommand.CanExecute())
			    {
				    ViewModel.ShowNextEntriesCommand.Execute().FireAndForget();
			    }
		    }
		    else
		    {
			    if (ViewModel.ShowPreviousEntriesCommand.CanExecute())
			    {
				    ViewModel.ShowPreviousEntriesCommand.Execute().FireAndForget();
			    }
		    }
	    }

		private void OnItemFlicking(object sender, FlickingEventArgs e)
		{
			if (e.Direction == FlickDirection.Left)
			{
				e.CanContinue = ViewModel.ShowNextEntriesCommand.CanExecute();
			}
			else
			{
				e.CanContinue = ViewModel.ShowPreviousEntriesCommand.CanExecute();
			}
		}

	    public void OnPivotItemLoaded(IPivotHostView host)
	    {
		    StartTutorialIfNeeded();
	    }

	    public void OnPivotItemUnloaded(IPivotHostView host)
	    {
		    StopFirstUseTutorial();
	    }
	    
    }
}
