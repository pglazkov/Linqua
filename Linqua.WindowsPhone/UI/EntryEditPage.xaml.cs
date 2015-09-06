using System;
using Windows.ApplicationModel;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Framework;
using Linqua.Framework;

// The Basic Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace Linqua.UI
{
	/// <summary>
	/// An empty page that can be used on its own or navigated to within a Frame.
	/// </summary>
	public sealed partial class EntryEditPage : Page, IEntryEditView
	{
		private readonly NavigationHelper navigationHelper;

		public EntryEditPage()
		{
			InitializeComponent();

			if (!DesignMode.DesignModeEnabled)
			{
				ViewModel = CompositionManager.Current.GetInstance<ICompositionFactory>().Create<EntryEditViewModel>();
				ViewModel.View = this;
			}

			navigationHelper = new NavigationHelper(this);
			navigationHelper.LoadState += NavigationHelper_LoadState;
			navigationHelper.SaveState += NavigationHelper_SaveState;
		}

		/// <summary>
		/// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
		/// </summary>
		public NavigationHelper NavigationHelper => navigationHelper;

	    /// <summary>
		/// Gets the view model for this <see cref="Page"/>.
		/// This can be changed to a strongly typed view model.
		/// </summary>
		public EntryEditViewModel ViewModel
		{
			get { return (EntryEditViewModel)DataContext; }
			set { DataContext = value; }
		}

		/// <summary>
		/// Populates the page with content passed during navigation.  Any saved state is also
		/// provided when recreating a page from a prior session.
		/// </summary>
		/// <param name="sender">
		/// The source of the event; typically <see cref="NavigationHelper"/>
		/// </param>
		/// <param name="e">Event data that provides both the navigation parameter passed to
		/// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested and
		/// a dictionary of state preserved by this page during an earlier
		/// session.  The state will be null the first time a page is visited.</param>
		private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
		{
			Guard.Assert(e.NavigationParameter != null, "e.NavigationParameter != null");
			Guard.Assert(e.NavigationParameter is string, "e.NavigationParameter is string");

			ViewModel.InitializeAsync((string)e.NavigationParameter).FireAndForget();
		}

		/// <summary>
		/// Preserves state associated with this page in case the application is suspended or the
		/// page is discarded from the navigation cache.  Values must conform to the serialization
		/// requirements of <see cref="SuspensionManager.SessionState"/>.
		/// </summary>
		/// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
		/// <param name="e">Event data that provides an empty dictionary to be populated with
		/// serializable state.</param>
		private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
		{
		}

		#region NavigationHelper registration

		/// <summary>
		/// The methods provided in this section are simply used to allow
		/// NavigationHelper to respond to the page's navigation methods.
		/// <para>
		/// Page specific logic should be placed in event handlers for the  
		/// <see cref="Linqua.Framework.NavigationHelper.LoadState"/>
		/// and <see cref="Linqua.Framework.NavigationHelper.SaveState"/>.
		/// The navigation parameter is available in the LoadState method 
		/// in addition to page state preserved during an earlier session.
		/// </para>
		/// </summary>
		/// <param name="e">Provides data for navigation methods and event
		/// handlers that cannot cancel the navigation request.</param>
		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			ApplicationView.GetForCurrentView().SetDesiredBoundsMode(ApplicationViewBoundsMode.UseCoreWindow);

			navigationHelper.OnNavigatedTo(e);
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			navigationHelper.OnNavigatedFrom(e);

			ViewModel.Cleanup();
		}

		#endregion

		public void NavigateBack()
		{
		    Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
		    {
		        if (Frame.CanGoBack)
		        {
		            Frame.GoBack();
		        }
		        else
		        {
		            Frame.Navigate(typeof(MainPage));
		        }
		    }).FireAndForget();

		}
	}
}
