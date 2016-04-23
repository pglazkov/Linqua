using System;
using System.Composition;
using System.Threading.Tasks;
using Windows.UI.Popups;
using JetBrains.Annotations;

namespace Framework.PlatformServices
{
    [Export(typeof(IDialogService))]
    [Shared]
    public class DialogService : IDialogService
    {
        private const string ConfirmationOkButtonTextKey = "DialogService_Confirmation_Ok";
        private const string ConfirmationCancelButtonTextKey = "DialogService_Confirmation_Cancel";

        private readonly IStringResourceManager resourceManager;

        [ImportingConstructor]
        public DialogService([NotNull] IStringResourceManager resourceManager)
        {
            Guard.NotNull(resourceManager, nameof(resourceManager));

            this.resourceManager = resourceManager;
        }

        public async Task<bool> ShowConfirmation(string title, string message, string okCommandText = null, string cancelCommandText = null)
        {
            Guard.NotNullOrEmpty(title, nameof(title));
            Guard.NotNullOrEmpty(message, nameof(message));

            var dialog = new MessageDialog(message, title);

            var okCommand = new UICommand(okCommandText ?? resourceManager.GetString(ConfirmationOkButtonTextKey, "RES_MISSING OK"));

            dialog.Commands.Add(okCommand);
            dialog.Commands.Add(new UICommand(cancelCommandText ?? resourceManager.GetString(ConfirmationCancelButtonTextKey, "RES_MISSING Cancel")));

            var resultCommand = await dialog.ShowAsync();

            return resultCommand == okCommand;
        }
    }
}