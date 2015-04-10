using System.Threading.Tasks;

namespace Linqua
{
    public interface IApplicationController
    {
	    void OnIsLearntChanged(EntryViewModel entry);

	    Task TranslateEntryItemAsync(EntryViewModel entryItem);
    }
}
