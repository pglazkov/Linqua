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
        public RandomEntryListView()
        {
            InitializeComponent();

            ItemView.DataContextChanged += OnRandomItemDataContextChanged;
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            if (!DesignMode.DesignModeEnabled)
            {
                var eventAggregator = CompositionManager.Current.GetInstance<IEventAggregator>();
                eventAggregator.GetEvent<IsTranslationShownChangedEvent>().Subscribe(OnItemIsTranslationShownChanged);
            }
        }

        private RandomEntryListViewModel ViewModel => (RandomEntryListViewModel)DataContext;

        private IRoamingSettingsService RoamingSettings => CompositionManager.Current.GetInstance<IRoamingSettingsService>();

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
        }

        private void OnRandomItemDataContextChanged(FrameworkElement sender, DataContextChangedEventArgs args)
        {
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
        }

        public void OnPivotItemUnloaded(IPivotHostView host)
        {
        }

        private void OnItemIsTranslationShownChanged(IsTranslationShownChangedEvent e)
        {
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