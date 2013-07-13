using System;
using System.ComponentModel;
using System.Diagnostics;
using Framework;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace Linqua
{
	public partial class MainView : PhoneApplicationPage, IMainView
	{
		// Constructor
		public MainView()
		{
			InitializeComponent();

			ViewAdapter = new ViewAdapter(this);

			// Sample code to localize the ApplicationBar
			//BuildLocalizedApplicationBar();

			if (!DesignerProperties.IsInDesignTool)
			{
				Presenter = CompositionManager.Current.GetInstance<MainPresenter>();

				Debug.Assert(Presenter != null);

				Presenter.Initialize(view: this);

				CompositionManager.Current.Compose(this);
			}
		}

		private ViewAdapter ViewAdapter { get; set; }
		private MainPresenter Presenter { get; set; }

		// Sample code for building a localized ApplicationBar
		//private void BuildLocalizedApplicationBar()
		//{
		//    // Set the page's ApplicationBar to a new instance of ApplicationBar.
		//    ApplicationBar = new ApplicationBar();

		//    // Create a new button and set the text value to the localized string from AppResources.
		//    ApplicationBarIconButton appBarButton = new ApplicationBarIconButton(new Uri("/Assets/AppBar/appbar.add.rest.png", UriKind.Relative));
		//    appBarButton.Text = AppResources.AppBarButtonText;
		//    ApplicationBar.Buttons.Add(appBarButton);

		//    // Create a new menu item with the localized string from AppResources.
		//    ApplicationBarMenuItem appBarMenuItem = new ApplicationBarMenuItem(AppResources.AppBarMenuItemText);
		//    ApplicationBar.MenuItems.Add(appBarMenuItem);
		//}

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

		ApplicationBarIconButton IMainView.AddWordButton
		{
			get { return ((ApplicationBarIconButton)ApplicationBar.Buttons[0]); }
		}
	}
}