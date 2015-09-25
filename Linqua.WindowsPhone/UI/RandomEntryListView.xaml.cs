using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Linqua.UI
{
    public partial class RandomEntryListView : UserControl, IPivotContentView
    {
	    private bool isLoaded;

        private IDictionary<FirstUseTutorialType, bool> isFirstUseTutorialRunning =
            Enum.GetValues(typeof(FirstUseTutorialType)).Cast<FirstUseTutorialType>().ToDictionary(x => x, _ => false);

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
                eventAggregator.GetEvent<IsTranslationShownChangedEvent>().Subscribe(OnItemIsTranslationShownChanged);
            }
        }

        private RandomEntryListViewModel ViewModel => (RandomEntryListViewModel)DataContext;

        private IRoamingSettingsService RoamingSettings => CompositionManager.Current.GetInstance<IRoamingSettingsService>();

        private void OnLoaded(object sender, RoutedEventArgs e)
		{
            //ViewModel.SetIsFirstUseTutorialComplete(FirstUseTutorialType.TapToSeeTranslation, false);
            //ViewModel.SetIsFirstUseTutorialComplete(FirstUseTutorialType.FlickToSeeNextRandomWord, false);

            isLoaded = true;
			StartTutorialsIfNeeded();
		}

	    private void OnUnloaded(object sender, RoutedEventArgs e)
		{
			isLoaded = false;
		}

		private void OnRandomItemDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
		{
			if (isLoaded)
			{
				StartTutorialsIfNeeded();
			}
		}

        private void StartTutorialsIfNeeded()
        {
            StartTutorialIfNeeded(FirstUseTutorialType.TapToSeeTranslation, () =>
            {
                return ViewModel != null && ViewModel.RandomEntryViewModels.Count == 1;
            });

            StartTutorialIfNeeded(FirstUseTutorialType.FlickToSeeNextRandomWord, () =>
            {
                return ViewModel != null &&
                       ViewModel.ShowNextEntriesCommand.CanExecute() &&
                       ViewModel.RandomEntryViewModels.Count > 0 &&
                       !ViewModel.RandomEntryViewModels[0].IsTranslationShown;
            });
        }

		private void StartTutorialIfNeeded(FirstUseTutorialType tutorialType, Func<bool> startCondition)
		{
			if (isFirstUseTutorialRunning.Values.Any(isRunning => isRunning))
			{
				return;
			}

			if (startCondition())
			{
				var isComplted = ViewModel.GetIsFirstUseTutorialComplete(tutorialType);

				if (!isComplted)
				{
					isFirstUseTutorialRunning[tutorialType] = true;

					var firstUseStoryboard = (Storyboard)Resources["FirstUseStoryboard_" + tutorialType];
                    firstUseStoryboard.Completed += (o, e) => OnFirstUseTutorialStoryboardCompleted(this, tutorialType);
					firstUseStoryboard.BeginTime = TimeSpan.FromSeconds(2.5);
					firstUseStoryboard.Begin();
				}
			}
		}

	    private static void OnFirstUseTutorialStoryboardCompleted(RandomEntryListView this_, FirstUseTutorialType tutorialType)
	    {
            this_.isFirstUseTutorialRunning[tutorialType] = false;
	    }

	    private void OnStopFirstUseTutorialRequested(StopFirstUseTutorialEvent e)
	    {
		    if (isFirstUseTutorialRunning[e.TutorialType])
		    {
			    CompleteFirstUseTutorial(e.TutorialType);
		    }
	    }

		private void CompleteFirstUseTutorial(FirstUseTutorialType tutorialType)
		{
			StopFirstUseTutorial(tutorialType);

			ViewModel.SetIsFirstUseTutorialComplete(tutorialType, true);

            StartTutorialsIfNeeded();
		}

	    private void StopFirstUseTutorial(FirstUseTutorialType tutorialType)
	    {
		    isFirstUseTutorialRunning[tutorialType] = false;

		    var firstUseStoryboard = (Storyboard)Resources["FirstUseStoryboard_" + tutorialType];

		    firstUseStoryboard.Stop();
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
		    CompleteFirstUseTutorial(FirstUseTutorialType.FlickToSeeNextRandomWord);

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
		    StartTutorialsIfNeeded();
	    }

	    public void OnPivotItemUnloaded(IPivotHostView host)
	    {
		    StopFirstUseTutorial(FirstUseTutorialType.TapToSeeTranslation);
            StopFirstUseTutorial(FirstUseTutorialType.FlickToSeeNextRandomWord);
        }

        private void OnItemIsTranslationShownChanged(IsTranslationShownChangedEvent e)
        {
            StartTutorialsIfNeeded();
        }

        private void OnFirstUseTutorialAreaTapped(object sender, TappedRoutedEventArgs e)
        {
            if (isFirstUseTutorialRunning[FirstUseTutorialType.TapToSeeTranslation])
            {
                CompleteFirstUseTutorial(FirstUseTutorialType.TapToSeeTranslation);
            }
            else if (isFirstUseTutorialRunning[FirstUseTutorialType.FlickToSeeNextRandomWord])
            {
                CompleteFirstUseTutorial(FirstUseTutorialType.FlickToSeeNextRandomWord);
            }
        }

	    private void OnShowNextHintTapped(object sender, TappedRoutedEventArgs e)
	    {
		    var hintStoryboard = (Storyboard)Resources["NextRandomWordAnimatedHintStoryboard"];

			hintStoryboard.Begin();
	    }

	    private void OnShowPreviousHintTapped(object sender, TappedRoutedEventArgs e)
	    {
			var hintStoryboard = (Storyboard)Resources["PreviousWordAnimatedHintStoryboard"];

			hintStoryboard.Begin();
		}
    }
}
