using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Linqua.DataObjects;
using Linqua.UI;

namespace Linqua
{
    public interface IEntryOperations
    {
        [NotNull]
        Task DeleteEntryAsync([NotNull] EntryViewModel entry);

        [NotNull]
        Task UpdateEntryAsync([NotNull] Entry entry);

        [NotNull]
        Task UpdateEntryIsLearnedAsync([NotNull] EntryViewModel entry);

        [NotNull]
        Task<string> TranslateEntryItemAsync([NotNull] Entry entry, [NotNull] IEnumerable<EntryViewModel> viewModelsToUpdate);

        [NotNull]
        Task<string> GetEntryLanguageNameAsync([NotNull] string languageCode, [NotNull] string locale);
    }
}