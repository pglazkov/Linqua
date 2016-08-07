using Framework.PlatformServices;
using Microsoft.Bot.Builder.Dialogs;

namespace Linqua.Bot
{
    public class BotDataBagSettingsService : ISettingsService
    {
        private readonly IBotDataBag dataBag;

        public BotDataBagSettingsService(IBotDataBag dataBag)
        {
            this.dataBag = dataBag;
        }

        #region Implementation of ISettingsService

        public bool TryGetValue(string key, out object value)
        {
            return dataBag.TryGetValue(key, out value);
        }

        public void SetValue(string key, object value)
        {
            dataBag.SetValue(key, value);
        }

        public bool RemoveValue(string key)
        {
            return dataBag.RemoveValue(key);
        }

        public void Clear()
        {
            dataBag.Clear();
        }

        public int Count => dataBag.Count;

        #endregion
    }
}