using System.Collections.Generic;
using System.Threading.Tasks;
using Linqua.DataObjects;
using Linqua.UI;

namespace Linqua
{
    public class DesignTimeApplicationContoller : IEntryOperations
    {
        public Task DeleteEntryAsync(EntryViewModel entry)
        {
            return Task.FromResult(true);
        }

        public Task UpdateEntryAsync(Entry entry)
        {
            return Task.FromResult(true);
        }

        public Task UpdateEntryIsLearnedAsync(EntryViewModel entry)
        {
            return Task.FromResult(true);
        }

        public Task<string> TranslateEntryItemAsync(Entry entry, IEnumerable<EntryViewModel> viewModelsToUpdate)
        {
            return Task.FromResult(string.Empty);
        }

        public Task<string> GetEntryLanguageNameAsync(string languageCode, string locale)
        {
            return Task.FromResult(string.Empty);
        }
    }
}