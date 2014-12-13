using System;

namespace Framework
{
    public abstract class PresenterBase : IPresenter
    {
	    private Lazy<IView> viewLazy;
	    private bool viewLoadedFirstTime = false;

    	/// <summary>
    	/// Gets or sets the view.
    	/// </summary>
    	/// <value>The view.</value>
		public virtual IView View { get { return viewLazy.Value; } }

    	/// <summary>
    	/// Gets or sets the view model.
    	/// </summary>
    	/// <value>The view model.</value>
		public IViewModel ViewModel { get; protected set; }
		
    	public virtual void Initialize(IView view = null, IViewModel viewModel = null)
        {
			if (viewModel != null)
			{
				ViewModel = viewModel;
			}

            // Initialize view model if needed
			if (ViewModel == null)
			{
				ViewModel = CreateViewModel();
			}

			if (view != null)
			{
				InitializeView(view);

				viewLazy = new Lazy<IView>(() => view);
			}

			// Initialize view if needed
			if (viewLazy == null)
			{
				CreateLazyView();
			}

            OnInitialized();
        }

    	private void CreateLazyView()
    	{
    		viewLazy = new Lazy<IView>(() =>
    		{
    			var result = (IView)ViewLocatorPortable.LocateForViewModel(ViewModel) ?? CreateView();

    			InitializeView(result);

    			return result;
    		});
    	}

	    private void InitializeView(IView view)
	    {
		    view.Loaded += OnViewLoaded;
		    view.Unloaded += OnViewUnloaded;

		    view.DataContext = ViewModel;
	    }

	    private void OnInitialized()
	    {
			OnInitializedOverride();

		    RaiseInitialized();
	    }

	    protected virtual void OnInitializedOverride()
	    {
		    
	    }

	    #region Initialized Event

    	public event EventHandler Initialized;

	    private void RaiseInitialized()
		{
			var handler = Initialized;
			if (handler != null)
			{
				handler(this, EventArgs.Empty);
			}
		}

    	#endregion

    	protected virtual IView CreateView()
    	{
			throw new NotSupportedException("This method must be overriden in the derived class.");
    	}

    	protected abstract IViewModel CreateViewModel();

    	private void OnViewLoaded(object sender, EventArgs eventArgs)
		{
			if (!viewLoadedFirstTime)
			{
				try
				{
					OnViewLoadedFirstTimeOverride();
				}
				finally
				{
					viewLoadedFirstTime = true;
				}
			}

			OnViewLoadedOverride();
		}

	    protected virtual void OnViewLoadedFirstTimeOverride()
	    {
		    
	    }

	    private void OnViewUnloaded(object sender, EventArgs eventArgs)
    	{
    		OnViewUnloadedOverride();
    	}

    	protected virtual void OnViewUnloadedOverride()
    	{
    	}

    	protected virtual void OnViewLoadedOverride()
        {
        }

		public void Cleanup()
		{
			if (ViewModel != null)
			{
				ViewModel.Cleanup();
			}

			CleanupOverride();
		}

    	protected virtual void CleanupOverride()
    	{
    		
    	}
    }

	public abstract class PresenterBase<TViewModel> : PresenterBase, IPresenter<TViewModel> 
		where TViewModel : IViewModel
	{
		public new TViewModel ViewModel
		{
			get { return (TViewModel)base.ViewModel; }
			protected set { base.ViewModel = value; }
		}
	}

	public abstract class PresenterBase<TViewModel, TView> : PresenterBase<TViewModel>, IPresenter<TViewModel, TView> 
		where TViewModel : IViewModel
		where TView : IView
	{
		public new TView View
		{
			get { return (TView) base.View; }
		}
	}
	
}
