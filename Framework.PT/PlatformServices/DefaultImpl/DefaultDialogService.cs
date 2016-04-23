using System.Threading.Tasks;
using Framework.MefExtensions;

namespace Framework.PlatformServices.DefaultImpl
{
    [DefaultExport(typeof(IDialogService))]
    public class DefaultDialogService : IDialogService
    {
        public Task<bool> ShowConfirmation(string title, string message, string okCommandText = null, string cancelCommandText = null)
        {
            throw new System.NotImplementedException();
        }
    }
}