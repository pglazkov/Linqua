using JetBrains.Annotations;

namespace Linqua
{
	public interface IMainView : INavigationView
	{
		void NavigateToNewWordPage();
		void FocusEntryCreationView();
		void NavigateToEntryDetails([NotNull] string entryId);
	}
}