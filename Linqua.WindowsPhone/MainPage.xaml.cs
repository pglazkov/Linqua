﻿using System;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Framework;
using Linqua.Events;
using Microsoft.WindowsAzure.MobileServices;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Linqua
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
	public sealed partial class MainPage : Page, IMainView
    {
        public MainPage()
        {
            this.InitializeComponent();
			
            this.NavigationCacheMode = NavigationCacheMode.Required;
	        
			if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
			{
				DataContext = ViewModel = CompositionManager.Current.GetInstance<ICompositionFactory>().Create<MainViewModel>();
				ViewModel.View = this;

				var eventAggregator = CompositionManager.Current.GetInstance<IEventAggregator>();

				eventAggregator.GetEvent<EntryCreatedEvent>().Subscribe(OnEntryCreated);

				Loaded += OnLoaded;
			}
        }

	    private MainViewModel ViewModel { get; set; }

		private void OnLoaded(object sender, RoutedEventArgs e)
		{
			// Schedule the initialization with the dispatcher because we need to run authentication
			// outside of the Loaded event, otherwise an exception will be thrown. 
			// For more details see: https://social.msdn.microsoft.com/Forums/vstudio/en-US/95c6569e-2fa2-43c8-af71-939e006a9b27/mobile-services-loginasync-remote-procedure-call-failed-hresult-0x800706be?forum=azuremobile
			Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
			{
				InitializeAsync().FireAndForget();
			});
		}

	    private async Task InitializeAsync()
	    {
		    await SecurityManager.Authenticate();
		    ViewModel.Initialize();
	    }

	    /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

	    public bool Navigate(Type destination)
	    {
		    return Frame.Navigate(destination);
	    }

	    public bool Navigate(Type destination, object parameter)
	    {
		    return Frame.Navigate(destination, parameter);
	    }

	    public void NavigateToNewWordPage()
	    {
		    Navigate(typeof (NewEntryPage));
	    }

		private void OnEntryCreated(EntryCreatedEvent e)
		{
			EntryListView.Focus(FocusState.Programmatic);
		}
    }
}
