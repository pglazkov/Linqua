using System;
using System.Threading.Tasks;

namespace Framework.PlatformServices
{
    public interface IDispatcherService
    {
        bool CheckAccess();

        Task InvokeAsync(Delegate method, params Object[] args);
    }
}