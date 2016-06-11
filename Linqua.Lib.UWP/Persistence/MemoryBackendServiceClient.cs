using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Linqua.DataObjects;
using Linqua.Service.DataObjects;

namespace Linqua.Persistence
{
    //[Export(typeof(IBackendServiceClient))]
    public class MemoryBackendServiceClient : IBackendServiceClient
    {
        public Task<IEnumerable<Entry>> LoadEntries(Expression<Func<Entry, bool>> filter)
        {
            return Task.Factory.StartNew(() =>
            {
                var predicate = (filter ?? (x => true)).Compile();

                return FakeData.FakeWords.Where(predicate);
            });
        }

        public Task<long> GetCount(Expression<Func<Entry, bool>> filter = null)
        {
            return Task.Factory.StartNew(() =>
            {
                var predicate = (filter ?? (x => true)).Compile();

                return (long)FakeData.FakeWords.Where(predicate).Count();
            });
        }

        public Task<Entry> LookupById(string entryId, CancellationToken? cancellationToken)
        {
            return Task.Factory.StartNew(() => { return FakeData.FakeWords.SingleOrDefault(x => x.Id == entryId); });
        }

        public Task<Entry> LookupByExample(Entry example)
        {
            return Task.Factory.StartNew(() => { return example; });
        }

        public Task<IEnumerable<Entry>> GetRandomEntries(int count)
        {
            var indexGenerator = new Random((int)DateTime.UtcNow.Ticks);

            var randomIndex = indexGenerator.Next(0, FakeData.FakeWords.Count - 1);

            var randomEntry = FakeData.FakeWords[randomIndex];

            return Task.FromResult<IEnumerable<Entry>>(new[] {randomEntry});
        }

        public Task<Entry> AddEntry(Entry newEntry)
        {
            FakeData.FakeWords.Add(newEntry);

            return Task.FromResult(newEntry);
        }

        public Task DeleteEntry(Entry entry)
        {
            FakeData.FakeWords.Remove(entry);

            return Task.FromResult(true);
        }

        public Task UpdateEntry(Entry entry)
        {
            FakeData.FakeWords.Remove(entry);
            FakeData.FakeWords.Add(entry);

            return Task.FromResult(true);
        }

        public Task<LocalDbState> InitializeAsync(bool doInitialPoolIfNeeded)
        {
            return Task.FromResult(LocalDbState.Unknown);
        }

        public Task<bool> TrySyncAsync(OfflineSyncArguments args = null)
        {
            return Task.FromResult(true);
        }

        public Task<LogUploadInfo> GetLogUploadInfoAsync()
        {
            throw new NotImplementedException();
        }
    }
}