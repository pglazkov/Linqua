using System.Threading.Tasks;
using Framework;
using JetBrains.Annotations;
using Nito.AsyncEx;

namespace Linqua.Persistence
{
    internal class SyncAction : ISyncHandle
    {
        public SyncAction([NotNull] OfflineSyncArguments arguments, [NotNull] TaskCompletionSource completionSource)
        {
            Guard.NotNull(arguments, nameof(arguments));
            Guard.NotNull(completionSource, nameof(completionSource));

            CompletionSource = completionSource;
            Arguments = arguments;
        }

        public OfflineSyncArguments Arguments { get; }

        public TaskCompletionSource CompletionSource { get; }

        public int CurrentTryCount { get; set; }

        public Task Task => CompletionSource.Task;
    }
}