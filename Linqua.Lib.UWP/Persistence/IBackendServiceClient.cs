using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Linqua.DataObjects;
using Linqua.Service.DataObjects;

namespace Linqua.Persistence
{
    public interface IBackendServiceClient
    {
        [NotNull]
        Task<IEnumerable<Entry>> LoadEntries([CanBeNull] Expression<Func<Entry, bool>> filter = null);

        [NotNull]
        Task<long> GetCount([CanBeNull] Expression<Func<Entry, bool>> filter = null);

        [NotNull]
        Task<Entry> LookupById([NotNull] string entryId, CancellationToken? cancellationToken = null);

        [NotNull]
        Task<Entry> LookupByExample([NotNull] Entry example);

        [NotNull]
        Task<IEnumerable<Entry>> GetRandomEntries(int count);

        [NotNull]
        Task<Entry> AddEntry([NotNull] Entry newEntry);

        [NotNull]
        Task DeleteEntry([NotNull] Entry entry);

        [NotNull]
        Task UpdateEntry([NotNull] Entry entry);

        [NotNull]
        Task<bool> GetIsInitialPullRequiredAsync();

        [NotNull]
        Task InitializeAsync();

        [NotNull]
        Task<LocalDbState> DoInitialPullIfNeededAsync(OfflineSyncArguments args = null);

        [NotNull]
        Task<bool> TrySyncAsync(OfflineSyncArguments args = null);

        [NotNull]
        Task<LogUploadInfo> GetLogUploadInfoAsync();

        
    }
}