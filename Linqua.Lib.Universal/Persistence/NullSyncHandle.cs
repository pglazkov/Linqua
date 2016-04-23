using System.Threading.Tasks;

namespace Linqua.Persistence
{
    public class NullSyncHandle : ISyncHandle
    {
        public Task Task
        {
            get { return Task.FromResult(true); }
        }
    }
}