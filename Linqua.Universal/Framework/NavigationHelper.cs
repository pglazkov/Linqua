using System;
using System.Collections.Generic;
using Windows.Foundation.Metadata;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Framework;

namespace Linqua.Framework
{
	/// <summary>
	/// NavigationHelper aids in navigation between pages.  It provides commands used to 
	/// navigate back and forward as well as registers for standard mouse and keyboard 
	/// shortcuts used to go back and forward in Windows and the hardware back button in
	/// Windows Phone.  In addition it integrates SuspensionManger to handle process lifetime
	/// management and state management when navigating between pages.
	/// </summary>
	/// <example>
	/// To make use of NavigationHelper, follow these two steps or
	/// start with a BasicPage or any other Page item template other than BlankPage.
	/// 
	/// 1) Create an instance of the NavigationHelper somewhere such as in the 
	///     constructor for the page and register a callback for the LoadState and 
	///     SaveState events.
	/// <code>
	///     public MyPage()
	///     {
	///         this.InitializeComponent();
	///         var navigationHelper = new NavigationHelper(this);
	///         this.navigationHelper.LoadState += navigationHelper_LoadState;
	///         this.navigationHelper.SaveState += navigationHelper_SaveState;
	///     }
	///     
	///     private async void navigationHelper_LoadState(object sender, LoadStateEventArgs e)
	///     { }
	///     private async void navigationHelper_SaveState(object sender, LoadStateEventArgs e)
	///     { }
	/// </code>
	/// 
	/// 2) Register the page to call into the NavigationHelper whenever the page participates 
	///     in navigation by overriding the <see cref="Windows.UI.Xaml.Controls.Page.OnNavigatedTo"/> 
	///     and <see cref="Windows.UI.Xaml.Controls.Page.OnNavigatedFrom"/> events.
	/// <code>
	///     protected override void OnNavigatedTo(NavigationEventArgs e)
	///     {
	///         navigationHelper.OnNavigatedTo(e);
	///     }
	///     
	///     protected override void OnNavigatedFrom(NavigationEventArgs e)
	///     {
	///         navigationHelper.OnNavigatedFrom(e);
	///     }
	/// </code>
	/// </example>
	[WebHostHidden]
	public class NavigationHelper : DependencyObject
	{
		private Page Page { get; }
		private Frame Frame { get { return this.Page.Frame; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="NavigationHelper"/> class.
		/// </summary>
		/// <param name="page">A reference to the current page used for navigation.  
		/// This reference allows for frame manipulation and to ensure that keyboard 
		/// navigation requests only occur when the page is occupying the entire window.</param>
		public NavigationHelper(Page page)
		{
			this.Page = page;

			// When this page is part of the visual tree make two changes:
			// 1) Map application view state to visual state for the page
			// 2) Handle hardware navigation requests
			this.Page.Loaded += (sender, e) =>
			{
				var currentView = SystemNavigationManager.GetForCurrentView();

				if (!ApiInformation.IsTypePresent("Windows.Phone.UI.Input.HardwareButtons"))
				{
					currentView.AppViewBackButtonVisibility = Frame.CanGoBack ? AppViewBackButtonVisibility.Visible : AppViewBackButtonVisibility.Collapsed;
				}

				currentView.BackRequested += OnBackRequested;
			};

			// Undo the same changes when the page is no longer visible
			this.Page.Unloaded += (sender, e) =>
			{
				var currentView = SystemNavigationManager.GetForCurrentView();

				currentView.BackRequested -= OnBackRequested;
			};
		}

		#region Navigation support

		DelegateCommand goBackCommand;
		DelegateCommand goForwardCommand;

		/// <summary>
		/// <see cref="DelegateCommand"/> used to bind to the back Button's Command property
		/// for navigating to the most recent item in back navigation history, if a Frame
		/// manages its own navigation history.
		/// 
		/// The <see cref="DelegateCommand"/> is set up to use the virtual method <see cref="GoBack"/>
		/// as the Execute Action and <see cref="CanGoBack"/> for CanExecute.
		/// </summary>
		public DelegateCommand GoBackCommand
		{
			get
			{
				if (goBackCommand == null)
				{
					goBackCommand = new DelegateCommand(
						GoBack,
						CanGoBack);
				}
				return goBackCommand;
			}
			set
			{
				goBackCommand = value;
			}
		}
		/// <summary>
		/// <see cref="DelegateCommand"/> used for navigating to the most recent item in 
		/// the forward navigation history, if a Frame manages its own navigation history.
		/// 
		/// The <see cref="DelegateCommand"/> is set up to use the virtual method <see cref="GoForward"/>
		/// as the Execute Action and <see cref="CanGoForward"/> for CanExecute.
		/// </summary>
		public DelegateCommand GoForwardCommand
		{
			get
			{
				if (goForwardCommand == null)
				{
					goForwardCommand = new DelegateCommand(
						GoForward,
						CanGoForward);
				}
				return goForwardCommand;
			}
		}

		/// <summary>
		/// Virtual method used by the <see cref="GoBackCommand"/> property
		/// to determine if the <see cref="Frame"/> can go back.
		/// </summary>
		/// <returns>
		/// true if the <see cref="Frame"/> has at least one entry 
		/// in the back navigation history.
		/// </returns>
		public virtual bool CanGoBack()
		{
			return this.Frame != null && this.Frame.CanGoBack;
		}
		/// <summary>
		/// Virtual method used by the <see cref="GoForwardCommand"/> property
		/// to determine if the <see cref="Frame"/> can go forward.
		/// </summary>
		/// <returns>
		/// true if the <see cref="Frame"/> has at least one entry 
		/// in the forward navigation history.
		/// </returns>
		public virtual bool CanGoForward()
		{
			return this.Frame != null && this.Frame.CanGoForward;
		}

		/// <summary>
		/// Virtual method used by the <see cref="GoBackCommand"/> property
		/// to invoke the <see cref="Windows.UI.Xaml.Controls.Frame.GoBack"/> method.
		/// </summary>
		public virtual void GoBack()
		{
			if (this.Frame != null && this.Frame.CanGoBack) this.Frame.GoBack();
		}
		/// <summary>
		/// Virtual method used by the <see cref="GoForwardCommand"/> property
		/// to invoke the <see cref="Windows.UI.Xaml.Controls.Frame.GoForward"/> method.
		/// </summary>
		public virtual void GoForward()
		{
			if (this.Frame != null && this.Frame.CanGoForward) this.Frame.GoForward();
		}

        /// <summary>
        /// Invoked when the hardware back button is pressed. For Windows Phone only.
        /// </summary>
        /// <param name="sender">Instance that triggered the event.</param>
        /// <param name="e">Event data describing the conditions that led to the event.</param>
        private void OnBackRequested(object sender, BackRequestedEventArgs e)
        {
            if (this.GoBackCommand.CanExecute())
            {
                e.Handled = true;
                this.GoBackCommand.Execute().FireAndForget();
            }
        }

		#endregion

		#region Process lifetime management

		private String _pageKey;

		/// <summary>
		/// Register this event on the current page to populate the page
		/// with content passed during navigation as well as any saved
		/// state provided when recreating a page from a prior session.
		/// </summary>
		public event LoadStateEventHandler LoadState;
		/// <summary>
		/// Register this event on the current page to preserve
		/// state associated with the current page in case the
		/// application is suspended or the page is discarded from
		/// the navigaqtion cache.
		/// </summary>
		public event SaveStateEventHandler SaveState;

		/// <summary>
		/// Invoked when this page is about to be displayed in a Frame.  
		/// This method calls <see cref="LoadState"/>, where all page specific
		/// navigation and process lifetime management logic should be placed.
		/// </summary>
		/// <param name="e">Event data that describes how this page was reached.  The Parameter
		/// property provides the group to be displayed.</param>
		public void OnNavigatedTo(NavigationEventArgs e)
		{
			var frameState = SuspensionManager.SessionStateForFrame(this.Frame);
			this._pageKey = "Page-" + this.Frame.BackStackDepth;

			if (e.NavigationMode == NavigationMode.New)
			{
				// Clear existing state for forward navigation when adding a new page to the
				// navigation stack
				var nextPageKey = this._pageKey;
				int nextPageIndex = this.Frame.BackStackDepth;
				while (frameState.Remove(nextPageKey))
				{
					nextPageIndex++;
					nextPageKey = "Page-" + nextPageIndex;
				}

				// Pass the navigation parameter to the new page
				if (this.LoadState != null)
				{
					this.LoadState(this, new LoadStateEventArgs(e.Parameter, null));
				}
			}
			else
			{
				// Pass the navigation parameter and preserved page state to the page, using
				// the same strategy for loading suspended state and recreating pages discarded
				// from cache
				if (this.LoadState != null)
				{
					this.LoadState(this, new LoadStateEventArgs(e.Parameter, (Dictionary<String, Object>)frameState[this._pageKey]));
				}
			}
		}

		/// <summary>
		/// Invoked when this page will no longer be displayed in a Frame.
		/// This method calls <see cref="SaveState"/>, where all page specific
		/// navigation and process lifetime management logic should be placed.
		/// </summary>
		/// <param name="e">Event data that describes how this page was reached.  The Parameter
		/// property provides the group to be displayed.</param>
		public void OnNavigatedFrom(NavigationEventArgs e)
		{
			var frameState = SuspensionManager.SessionStateForFrame(this.Frame);
			var pageState = new Dictionary<String, Object>();
			if (this.SaveState != null)
			{
				this.SaveState(this, new SaveStateEventArgs(pageState));
			}
			frameState[_pageKey] = pageState;
		}

		#endregion
	}

	/// <summary>
	/// Represents the method that will handle the <see cref="NavigationHelper.LoadState"/>event
	/// </summary>
	public delegate void LoadStateEventHandler(object sender, LoadStateEventArgs e);
	/// <summary>
	/// Represents the method that will handle the <see cref="NavigationHelper.SaveState"/>event
	/// </summary>
	public delegate void SaveStateEventHandler(object sender, SaveStateEventArgs e);

	/// <summary>
	/// Class used to hold the event data required when a page attempts to load state.
	/// </summary>
	public class LoadStateEventArgs : EventArgs
	{
		/// <summary>
		/// The parameter value passed to <see cref="Frame.Navigate(Type, Object)"/> 
		/// when this page was initially requested.
		/// </summary>
		public Object NavigationParameter { get; private set; }
		/// <summary>
		/// A dictionary of state preserved by this page during an earlier
		/// session.  This will be null the first time a page is visited.
		/// </summary>
		public Dictionary<string, Object> PageState { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="LoadStateEventArgs"/> class.
		/// </summary>
		/// <param name="navigationParameter">
		/// The parameter value passed to <see cref="Frame.Navigate(Type, Object)"/> 
		/// when this page was initially requested.
		/// </param>
		/// <param name="pageState">
		/// A dictionary of state preserved by this page during an earlier
		/// session.  This will be null the first time a page is visited.
		/// </param>
		public LoadStateEventArgs(Object navigationParameter, Dictionary<string, Object> pageState)
			: base()
		{
			this.NavigationParameter = navigationParameter;
			this.PageState = pageState;
		}
	}
	/// <summary>
	/// Class used to hold the event data required when a page attempts to save state.
	/// </summary>
	public class SaveStateEventArgs : EventArgs
	{
		/// <summary>
		/// An empty dictionary to be populated with serializable state.
		/// </summary>
		public Dictionary<string, Object> PageState { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="SaveStateEventArgs"/> class.
		/// </summary>
		/// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
		public SaveStateEventArgs(Dictionary<string, Object> pageState)
			: base()
		{
			this.PageState = pageState;
		}
	}
}
