using System.Collections.Generic;
using System.Composition;

namespace Framework.PlatformServices
{
    [Export(typeof(ILocalSettingsService))]
    [Shared]
    public class LocalSettingsService : ILocalSettingsService
    {
        private static IDictionary<string, object> Values => Windows.Storage.ApplicationData.Current.LocalSettings.Values;

        #region Implementation of ISettingsService

        public int Count
        {
            get { return Values.Count; }
        }

        public bool TryGetValue(string key, out object value)
        {
            return Values.TryGetValue(key, out value);
        }

        public void SetValue(string key, object value)
        {
            Values[key] = value;
        }

        public bool RemoveValue(string key)
        {
            return Values.Remove(key);
        }

        public void Clear()
        {
            Values.Clear();
        }

        #endregion
    }
}