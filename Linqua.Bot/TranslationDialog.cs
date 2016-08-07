using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Linqua.Translation.Microsoft;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;

namespace Linqua.Bot
{
    [Serializable]
    public class TranslationDialog : IDialog<object>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
        }

        public async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> argument)
        {
            var message = await argument;

            var translationService = new MicrosoftTranslationService(new MicrosoftAccessTokenProvider(new BotDataBagSettingsService(context.PrivateConversationData)));

            var language = await translationService.DetectLanguageAsync(message.Text);

            //var languageNames = await translationService.GetLanguageNamesAsync(new[] {language}, "en");

            //var languageName = languageNames[language];

            var translation = await translationService.TranslateAsync(message.Text, language, "en");

            await context.PostAsync(translation);

            context.Wait(MessageReceivedAsync);
        }
    }
}