using System.Collections.Generic;
using System.Composition;
using Windows.Storage;

namespace Framework.PlatformServices
{
    [Export(typeof(IRoamingSettingsService))]
    [Shared]
    public class RoamingSettingsService : IRoamingSettingsService
    {
        private IDictionary<string, object> Values { get; } = ApplicationData.Current.RoamingSettings.Values;

        #region Implementation of ISettingsService

        public int Count => Values.Count;

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