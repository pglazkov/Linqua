using System;

namespace Framework
{
    /// <summary>
    /// 
    /// </summary>
    public interface IPresenter
    {
		/// <summary>
		/// Raised when the presenter is fully initialized.
		/// </summary>
		event EventHandler Initialized;

        /// <summary>
        /// Gets or sets the view.
        /// </summary>
        /// <value>The view.</value>
		IView View { get; }

        /// <summary>
        /// Gets or sets the view model.
        /// </summary>
        /// <value>The view model.</value>
        IViewModel ViewModel { get; }

		/// <summary>
		/// Initialized the presenter with optional view and view model. If view and/or view model is not
		/// specified, they will be created according to the default logic.
		/// </summary>
		/// <param name="view">View to use with this presenter.</param>
		/// <param name="viewModel">View model to use with this presenter.</param>
		void Initialize(IView view = null, IViewModel viewModel = null);

		/// <summary>
		/// Does a cleanup up before deactivating this presenter.
		/// </summary>
    	void Cleanup();
    }

	public interface IPresenter<out TViewModel> : IPresenter
		where TViewModel : IViewModel
	{
		new TViewModel ViewModel { get; }
	}

	public interface IPresenter<out TViewModel, out TView> : IPresenter<TViewModel>
		where TViewModel : IViewModel
		where TView : IView
	{
		new TView View { get; }
	}
}
