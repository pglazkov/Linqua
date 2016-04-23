using System.Threading.Tasks;

namespace Linqua.Persistence
{
    public interface ISyncHandle
    {
        Task Task { get; }
    }
}