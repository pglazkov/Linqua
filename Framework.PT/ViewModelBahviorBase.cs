using JetBrains.Annotations;

namespace Framework
{
	public abstract class ViewModelBahviorBase<TViewModel> : NotificationObject, IViewModelBahavior
		where TViewModel : IViewModelWithBehaviors
	{
		public TViewModel AssociatedViewModel { get; private set; }

		public void Attach([NotNull] IViewModelWithBehaviors viewModel)
		{
			Guard.NotNull(viewModel, () => viewModel);

			AssociatedViewModel = (TViewModel)viewModel;
		}

		protected virtual void AttachOverride([NotNull] TViewModel viewModel)
		{
		}
	}
}