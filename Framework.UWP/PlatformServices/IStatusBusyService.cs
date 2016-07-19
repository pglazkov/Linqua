using System;

namespace Framework.PlatformServices
{
    public interface IStatusBusyService
    {
        IDisposable Busy(CommonBusyType type);
        IDisposable Busy(string statusText = null);
    }
}