using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Framework;

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

			ViewAdapter = new ViewAdapter(this);
			
			// Sample code to localize the ApplicationBar
			//BuildLocalizedApplicationBar();

			if (!Windows.ApplicationModel.DesignMode.DesignModeEnabled)
			{
				Presenter = CompositionManager.Current.GetInstance<MainPresenter>();

				Debug.Assert(Presenter != null);

				Presenter.Initialize(view: this);

				CompositionManager.Current.Compose(this);
			}
        }

		private ViewAdapter ViewAdapter { get; set; }
		private MainPresenter Presenter { get; set; }

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

		event EventHandler<EventArgs> IView.Loaded
		{
			add { ViewAdapter.Loaded += value; }
			remove { ViewAdapter.Loaded -= value; }
		}

		event EventHandler<EventArgs> IView.Unloaded
		{
			add { ViewAdapter.Unloaded += value; }
			remove { ViewAdapter.Unloaded -= value; }
		}

		T IView.FindChildName<T>(string controlName)
		{
			return ViewAdapter.FindChildName<T>(controlName);
		}

		Button IMainView.AddWordButton
		{
			get { return NewWordButton; }
		}

	    public bool Navigate(Type destination)
	    {
		    return Frame.Navigate(destination);
	    }

	    public bool Navigate(Type destination, object parameter)
	    {
		    return Frame.Navigate(destination, parameter);
	    }
    }
}
