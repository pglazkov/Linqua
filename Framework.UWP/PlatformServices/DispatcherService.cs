using System;
using System.Composition;
using System.Threading.Tasks;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;

namespace Framework.PlatformServices
{
    [Export(typeof(IDispatcherService))]
    [Shared]
    public class DispatcherService : IDispatcherService
    {
        private CoreDispatcher Dispatcher
        {
            get { return CoreApplication.MainView.CoreWindow.Dispatcher; }
        }

        public bool CheckAccess()
        {
            return Dispatcher.HasThreadAccess;
        }

        public async Task InvokeAsync(Delegate method, params object[] args)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => method.DynamicInvoke(args));
        }
    }
}