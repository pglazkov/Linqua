using System.Composition;

namespace Linqua.Offline
{
    [Export(typeof(IOfflineSyncController))]
    [Shared]
    public class OfflineSyncController : IOfflineSyncController
    {
    }
}