using System;
using System.Threading.Tasks;
using Framework.PlatformServices;

namespace Framework
{
    public class DispatcherProxy
    {
        private readonly IDispatcherService impl;

        private DispatcherProxy()
        {
            impl = CompositionManager.Current.GetInstance<IDispatcherService>();
        }

        public static DispatcherProxy CreateDispatcher()
        {
            return new DispatcherProxy();
        }

        public bool CheckAccess()
        {
            return impl.CheckAccess();
        }

        public Task InvokeAsync(Delegate method, params object[] args)
        {
            return impl.InvokeAsync(method, args);
        }
    }
}